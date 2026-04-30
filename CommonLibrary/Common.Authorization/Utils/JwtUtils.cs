using Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Model.ResponseModel.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Entity.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace Common.Authorization.Utils
{
    public class JwtUtils : IJwtUtils
    {
        private readonly StrJWT _strJWT;
        private readonly MicrosoftSettings _microsoftSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _memoryCache;

        public JwtUtils(
            IOptions<StrJWT> strJwt,
            IOptions<MicrosoftSettings> microsoftSettings,
            IUnitOfWork unitOfWork,
            IMemoryCache memoryCache)
        {
            _strJWT = strJwt.Value;
            _microsoftSettings = microsoftSettings.Value;
            _unitOfWork = unitOfWork;
            _memoryCache = memoryCache;
        }

        private const string DOMAIN_MAIL = "@gmail.com";
        private const string TENANT_KEY = "tid"; // tenant id
        private const string UNIQUE_NAME = "unique_name"; // email
        private const string PREFERRER_USERNAME = "preferred_username";
        private const string EXP_KEY = "exp"; // expire time
        private const string JTI_SALT = "thanhbinhle1712"; // Secret key for hashing

        // Generic hash function for sensitive strings like JTI and UDID
        private string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input + JTI_SALT);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public string GenerateToken(Guid userId, string? fullName, string UDID, string userName)
        {
            string? skey = _strJWT.Key;
            string? issuer = _strJWT.Issuer;
            string? audience = _strJWT.Audience;
            var jti = Guid.NewGuid().ToString();
            var hashJti = HashString(jti);
            var hashUdid = HashString(UDID);
            var key = Encoding.ASCII.GetBytes(skey ?? string.Empty);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, userName),
                    new Claim("Id", userId.ToString()),
                    new Claim("UDID", hashUdid ),
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Name, fullName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Email, userName + DOMAIN_MAIL),
                    new Claim(JwtRegisteredClaimNames.Jti, hashJti)
                }),
                Expires = DateTime.UtcNow.AddDays(14), // Reduced to 30 minutes for access token
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };


            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public Guid? ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_strJWT.Key ?? string.Empty);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _strJWT.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _strJWT.Audience,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                Guid userId = Guid.Empty;
                Guid.TryParse(jwtToken.Claims.First(x => x.Type == "Id").Value, out userId);
                if (userId == Guid.Empty) return null;
                // return user id from JWT token if validation successful
                return userId;
            }
            catch (Exception ex)
            {
                // return null if validation fails
                return null;
            }
        }

        public RfTokenResponse GenerateRefreshToken(Guid userId, string? fullName, string userName, string UDID, string skey, string Issuer, string Audience, string ipAddress)
        {
            var key = Encoding.ASCII.GetBytes(skey);
            var jti = Guid.NewGuid().ToString();
            var hashJti = HashString(jti);
            var hashUdid = HashString(UDID);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, userName),
                    new Claim("Id", userId.ToString()),
                    new Claim("UDID", hashUdid),
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Name, fullName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Email, userName + DOMAIN_MAIL),
                    new Claim(JwtRegisteredClaimNames.Jti, hashJti)
                }),
                Expires = DateTime.UtcNow.AddDays(14), // Refresh token 14 days
                Issuer = Issuer,
                Audience = Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            // Do NOT set in cache on generation; only set when revoking elsewhere
            // var jtiRfTokenKey = $"revoke_Rf_{hashJti}";
            // _memoryCache.Set(jtiRfTokenKey, true, DateTime.UtcNow.AddDays(14));

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return new RfTokenResponse
            {
                Token = jwtToken,
                Expires = DateTime.UtcNow.AddDays(14),
                CreateTime = DateTime.Now,
                CreatedByIp = ipAddress
            };
        }

        public async Task<SysAccount?> ValidateTokenMicrosoft(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            // Decode token to get: tid, unique_name, exp
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var tid = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == TENANT_KEY)?.Value;
            var uniqueName = jsonToken?.Claims
                .FirstOrDefault(claim => claim.Type == UNIQUE_NAME || claim.Type == PREFERRER_USERNAME)?.Value;
            var exp = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == EXP_KEY)?.Value;

            if (string.IsNullOrWhiteSpace(tid) || string.IsNullOrWhiteSpace(uniqueName) ||
                string.IsNullOrWhiteSpace(exp))
                return null;

            // Check exp. if exp < now return null
            if (!long.TryParse(exp, out long expUnix) || DateTimeOffset.FromUnixTimeSeconds(expUnix) < DateTimeOffset.Now)
                return null;

            // Check tid in microsoft settings
            if (tid != _microsoftSettings.TenantId)
                return null;

            // Get user from cache or DB
            var accounts = await GetAccounts();
            // Append domain to match stored email
            return accounts?.FirstOrDefault(a => a.Email == uniqueName + DOMAIN_MAIL);
        }

        public async Task<Guid?> ValidateTokenWithJTICheck(string token, string udidFromHeader)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_strJWT.Key ?? string.Empty);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _strJWT.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _strJWT.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                var jtiHashFromToken = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var udidHashFromToken = jwtToken.Claims.FirstOrDefault(x => x.Type == "UDID")?.Value;

                if (!Guid.TryParse(userIdClaim, out Guid userId)) return null;

                // Check for revocation via JTI
                if (!string.IsNullOrEmpty(jtiHashFromToken))
                {
                    var jtiKey = $"revoke_{jtiHashFromToken}";
                    if (_memoryCache.TryGetValue(jtiKey, out _))
                    {
                        return null; // Token revoked
                    }
                }

                // Check device UDID if provided in header
                if (!string.IsNullOrEmpty(udidFromHeader) && !string.IsNullOrEmpty(udidHashFromToken))
                {
                    var udidHashFromHeader = HashString(udidFromHeader);
                    if (udidHashFromToken != udidHashFromHeader)
                    {
                        // Log suspicious activity
                        // await LogSuspiciousActivity(userId, udidFromHeader, "UDID mismatch detected");
                        return null;
                    }
                }

                return userId;
            }
            catch (Exception ex)
            {
                // Log error
                return null;
            }
        }

        public async Task<Guid?>  ValidateTokenWithRevokeCheck(string token , string headerUDID)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_strJWT.Key);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _strJWT.Issuer,       // set issuer
                    ValidateAudience = true,
                    ValidAudience = _strJWT.Audience,   // set audience
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtToken.Claims.First(x => x.Type == "Id").Value;

                if (!Guid.TryParse(userIdClaim, out Guid userId)) return null;
                
                // Kiểm tra device trong DB
                var device = await _unitOfWork.Repository<SysDevice>()
                    .FirstOrDefaultAsync(x => x.UDID == headerUDID && x.IsDeleted != true);

                if (device == null) return null;
                if (device.RfTokenRevokedTime != null || device.RfTokenExpiryTime <= DateTime.Now)
                    return null; // token đã revoke hoặc hết hạn

                return userId;
            }
            catch
            {
                return null;
            }
        }

        #region Private Methods

        private async Task<List<SysAccount>?> GetAccounts()
        {
            if (_memoryCache.TryGetValue(CacheKey.UsersCache, out List<SysAccount>? cacheEntry))
                return cacheEntry;

            var accounts = await _unitOfWork.Repository<SysAccount>()
                .Where(a => a.IsDeleted != true)
                .ToListAsync();

            _memoryCache.Set(CacheKey.UsersCache, accounts, TimeSpan.FromDays(1));
            return accounts;
        }

        #endregion
    }
}