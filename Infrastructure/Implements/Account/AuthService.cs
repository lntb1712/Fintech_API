using Common.Authorization;
using Common.Authorization.Utils;
using Common.Constant;
using Common.Settings;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Account;
using Entity;
using Entity.Entities;
using Entity.Entities.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Auth.Validation;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using Model.RequestModel.Common;
using Model.ResponseModel;
using Model.ResponseModel.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Infrastructure.Implements.Account
{
    public class AuthService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IJwtUtils _jwtUtils, IOptions<StrJWT> strJwt, IOptions<AppSettings> appSettings, IConfiguration config, IHttpContextAccessor httpContextAccessor) : BaseService(unitOfWork, memoryCache), IAuthService
    {
        private readonly IConfiguration _config = config;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private ExchangeService exchangeService;
        private readonly StrJWT _strJwt = strJwt.Value;
        private readonly AppSettings _appSettings = appSettings.Value;
        private const string DOMAIN_NAME = "@vietnamairlines.com";

        private string ComputeHmac(string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hash);
        }
        public async Task<object> Login(string userName, string? password, UserDeviceRequest userDevice)
        {
            var email = $"{userName}{DOMAIN_NAME}";
            var account = await _unitOfWork.Repository<SysAccount>()
                                           .FirstOrDefaultAsync(s => ((s.Email ?? "").Equals(email) ||
                                                               (s.PhoneNumber ?? "").Equals(userName)) && s.IsDeleted != true)
                                           ?? throw new AppException("Tên tài khoản hoặc mật khẩu bị lỗi!");
            #region Chống replay

            //var now = DateTimeOffset.Now.ToUnixTimeSeconds();

            //if (Math.Abs(now - userDevice.Timestamp) > 180)
            //    throw new AppException("Request quá hạn");

            //// Kiểm tra nonce trong memory cache
            //var cacheKey = $"login_nonce_{userDevice.Nonce}";
            //if (_memoryCache.TryGetValue(cacheKey, out _))
            //    throw new AppException("Nonce đã được sử dụng");

            //// Tạo signature server side
            //var message = $"{userName}{password}{userDevice.DeviceUUID}{userDevice.Timestamp}{userDevice.Nonce}";
            //var computedSignature = ComputeHmac(message, _strJwt.Key);
            //if (computedSignature != userDevice.Signature)
            //    throw new AppException("Signature không hợp lệ");

            //// Đánh dấu nonce đã dùng
            //_memoryCache.Set(cacheKey, true, TimeSpan.FromMinutes(5));

#endregion

            if (RegexUtilities.IsValidEmail(email))
            {
#if !DEBUG
                var isAuth = AuthenticateEmail(userName, password ?? "");
                if (!isAuth) throw new AppException("Tên tài khoản hoặc mật khẩu bị lỗi!");

#endif
                var deviceExist = await _unitOfWork.Repository<SysDevice>()
                                                   .FirstOrDefaultAsync(s => s.UserId == account.Id && s.IsActive == true&&
                                                                        s.UDID == userDevice.DeviceUUID);

                var accesToken = _jwtUtils.GenerateToken(account.Id, account.FullName ?? "", userDevice.DeviceUUID, account.UserName);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var refreshToken = _jwtUtils.GenerateRefreshToken(account.Id, account.FullName ?? "", account.UserName, userDevice.DeviceUUID,
                    _strJwt.Key, _strJwt.Issuer, _strJwt.Audience, ipAddress!);

                #region check device

                var deviceInfo = new DeviceInfoRequest
                {
                    UDID = userDevice.DeviceUUID ?? "",
                    DeviceName = userDevice.DeviceName,
                    OSName = userDevice.DeviceOS,
                    OSVersion = userDevice.DevicePlatform
                };

                await CheckDevice(account.Id, refreshToken, deviceInfo);
                await CheckUserDevice(account.Id, accesToken, refreshToken, deviceInfo);
                #endregion check device

                var loginResponse = new LoginResponse
                {
                    UserId = account.Id,
                };
                loginResponse.SetToken(accesToken);
                loginResponse.SetRefreshToken(refreshToken.Token);

                return Utils.CreateResponseModel(loginResponse);
            }
            else if (RegexUtilities.IsValidPhone(userName))
            {
#if !DEBUG
                var deviceInfo = new DeviceInfoRequest
                {
                    UDID = userDevice.DeviceUUID ?? "",
                    DeviceName = userDevice.DeviceName,
                    OSName = userDevice.DeviceOS,
                    OSVersion = userDevice.DevicePlatform
                };

                var isSendOtp = SendOTPLoginToPhone(new SendOTPLoginRequest { UserPhone = userName }, deviceInfo);

                return Utils.CreateResponseModel(isSendOtp);
#else
                return Utils.CreateResponseModel(true);
#endif
            }

            throw new AppException("Tên tài khoản hoặc mật khẩu bị lỗi!");
        }

        public async Task<BaseResponse<LoginResponse>> GetNewTokenByRefreshToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress, string currentAccessToken)
        {
            var user = await _unitOfWork.Repository<SysAccount>()
                        .Where(a => a.IsDeleted != true)
                        .Join(
                            _unitOfWork.Repository<SysDevice>()
                                .Where(d => d.RefreshToken == model.RefreshToken &&
                                            d.UDID == deviceInfo.UDID),
                            a => a.Id,
                            d => d.UserId,
                            (a, d) => new { Account = a, Device = d }
                        )
                        .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException(StatusMessage.DataNotFound);

            var device = user.Device;

            // Check refresh token validity
            if (device.RfTokenExpiryTime <= DateTime.UtcNow ||
                device.RfTokenRevokedTime != null ||
                device.RfTokenRevokedByIp != null ||
                device.RfTokenCreatedByIp != ipAddress)
                throw new AppException(StatusMessage.DataInputInvalid);

            // **Invalidate old access token using MemoryCache**
            if (!string.IsNullOrEmpty(currentAccessToken))
            {
                var expiresIn = device.RfTokenExpiryTime - DateTime.Now;
                _memoryCache.Set($"revoked_{currentAccessToken}", true, expiresIn);
            }

            // Generate new refresh token
            string skey = _strJwt.Key!;
            string issuer = _strJwt.Issuer!;
            string audience = _strJwt.Audience!;
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(
                user.Account.Id,
                user.Account.FullName,
                user.Account.UserName,
                deviceInfo.UDID,
                skey,
                issuer,
                audience,
                ipAddress
            );

            device.RefreshToken = newRefreshToken.Token;
            device.RfTokenCreateTime = newRefreshToken.CreateTime;
            device.RfTokenExpiryTime = newRefreshToken.Expires;
            device.RfTokenCreatedByIp = newRefreshToken.CreatedByIp;
            device.RfTokenRevokedTime = null;
            device.RfTokenRevokedByIp = null;

            _unitOfWork.Repository<SysDevice>().Update(device);
            _unitOfWork.Repository<SysAccount>().Update(user.Account);
            await _unitOfWork.SaveChangesAsync();

            // Generate new access token
            var jwtToken = _jwtUtils.GenerateToken(user.Account.Id, user.Account.FullName, user.Account.UserName, deviceInfo.UDID);
            var res = new LoginResponse();
            res.UserId = user.Account.Id;
            res.SetToken(jwtToken);
            res.SetRefreshToken(newRefreshToken.Token);

            return Utils.CreateResponseModel(res);
        }

        public async Task<object> RevokeTokenOneDevice(Guid currentUserId, string currentUserName, RefreshTokenRequest model, string UDID, string ipAddress)
        {
            // Kiểm tra có thiết bị khác cùng UDID nhưng khác refresh token không
            var existDevice = await _unitOfWork.Repository<SysDevice>()
                .FirstOrDefaultAsync(x => x.IsDeleted != true && x.UDID == UDID && x.RefreshToken != model.RefreshToken);

            if (existDevice != null)
            {
                throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Device"));
            }

            // Tìm đúng thiết bị cần revoke
            var device = await _unitOfWork.Repository<SysDevice>()
                .FirstOrDefaultAsync(x => x.IsDeleted != true && x.UDID == UDID && x.RefreshToken == model.RefreshToken);

            if (device == null)
                throw new KeyNotFoundException("Device not found or already revoked.");

            // Đánh dấu thu hồi token
            device.RfTokenRevokedTime = DateTime.Now;
            device.RfTokenRevokedByIp = ipAddress;
            device.UpdatedById = currentUserId;
            device.UpdatedDate = DateTime.Now;

            // Cho token hết hạn ngay lập tức
            device.RfTokenExpiryTime = DateTime.Now; 

            // (Tuỳ chọn) Xoá refresh token khỏi DB để tránh reuse
            device.RefreshToken = "";

            _unitOfWork.Repository<SysDevice>().Update(device);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<object> RevokeTokenAllDevice(Guid curentUserId, string currentUserName, string ipAddress)
        {
            var existListDevice = await _unitOfWork.Repository<SysDevice>()
                .Where(x => x.IsDeleted != true && x.UserId == curentUserId).ToListAsync();
            existListDevice.ForEach(x => {
                x.RfTokenRevokedTime = DateTime.Now;
                x.RfTokenRevokedByIp = ipAddress;
                x.UpdatedById = curentUserId;
                x.UpdatedDate = DateTime.Now;
                x.RfTokenExpiryTime = DateTime.Now;
                x.RefreshToken = "";
            });
            _unitOfWork.Repository<SysDevice>().UpdateRange(existListDevice);
            var res = await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(res > 0);
        }

        public bool RevokeToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            /*var user = _unitOfWork.Repository<ViagsUser>().Include(s => s.Devices).SingleOrDefault(
                s => s.IsDelete != true && s.Devices.Any(t => t.UDID == deviceInfo.UDID && (t.RefreshToken == model.RefreshToken)));

            // return false if no user found with token
            if (user == null) throw new KeyNotFoundException("Token not found");

            var device = user.Devices.Single(x => x.UDID == deviceInfo.UDID && x.RefreshToken == model.RefreshToken);

            // return false if token is not active
            if (device.RfTokenExpiryTime <= DateTime.UtcNow || device.RfTokenRevokedTime != null ||
                device.RfTokenRevokedByIp != null || device.RfTokenCreatedByIp != ipAddress)
                throw new AppException("Token already expires");

            // revoke token and save
            device.RfTokenRevokedTime = DateTime.UtcNow;
            device.RfTokenRevokedByIp = ipAddress;
            _unitOfWork.Repository<Entity.Entities.Device>().Update(device);
            _unitOfWork.Repository<ViagsUser>().Update(user);
            _unitOfWork.SaveChangesAsync();
            _unitOfWork.Dispose();*/

            return true;
        }

        public bool SendOTPLoginToPhone(SendOTPLoginRequest model, DeviceInfoRequest deviceInfo)
        {
            if (!RegexUtilities.IsValidPhone(model.UserPhone)) throw new AppException("Phone is wrong!");
            var otpCode = Utils.GenerateOneTimeOTP();
            var modelOtp = new ModelOtp
            {
                Code = otpCode,
                UDID = deviceInfo.UDID
            };

            Dictionary<string, int> dataBlackList;
            int numSent = 0;

            if (_memoryCache.TryGetValue(CacheKey.BlackListSms, out dataBlackList))
            {
                if (dataBlackList.TryGetValue(model.UserPhone, out numSent) && numSent >= 3)
                    throw new AppException("Too many sms sent, please try again in 24 hours!");
            }
            else
            {
                dataBlackList = new Dictionary<string, int>();
            }

            var res = SmsUtils.SendOTPToPhone(model.UserPhone, otpCode, _appSettings.SmsToken, _appSettings.SmsServiceUrl);
            if (res)
            {
                numSent += 1;
                dataBlackList[model.UserPhone] = numSent;
                _memoryCache.Set(CacheKey.BlackListSms, dataBlackList, CacheTime.BlackList);
            }

            _memoryCache.Set(model.UserPhone, modelOtp, CacheTime.OTP);
            return res;
        }

        public async Task<BaseResponse<LoginResponse>> LoginByOTP(LoginByOTPRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            ////Check phone to get user
            var user = await _unitOfWork.Repository<SysAccount>()
                       .Where(a => a.IsDeleted != true)
                       .Join(
                           _unitOfWork.Repository<SysDevice>()
                               .Where(d => d.IsDeleted != true &&
                                           d.UDID == deviceInfo.UDID),
                           a => a.Id,
                           d => d.UserId,
                           (a, d) => new
                           {
                               Account =a
                           }
                       )
                       .FirstOrDefaultAsync();
            if (user == null) throw new AppException("Device is not found or not register!");
#if !DEBUG
            ////Check otp
            //ModelOtp otpSaved;
            //if (!_memoryCache.TryGetValue(model.UserPhone, out otpSaved)) throw new AppException("Phone is wrong or OTP was expired, please re-enter the OTP");
            //otpSaved.NumCheck += 1;
            //if (otpSaved.NumCheck > 3) throw new AppException("OTP has been entered too many times, please re-enter the OTP");
            //if (otpSaved.Expire < DateTime.Now) throw new AppException("OTP was expired, please re-enter the OTP");
            //if (otpSaved.Code != model.OTP) throw new AppException("OTP is wrong!");
            //if (otpSaved.UDID != deviceInfo.UDID) throw new AppException("Device is wrong!");

#else
            if (_memoryCache.TryGetValue($"auth_{deviceInfo.UDID}_{model.OTP}", out _))
            {
                // Xóa cache sau khi dùng OTP
                _memoryCache.Remove($"auth_{deviceInfo.UDID}_{model.OTP}");
            }
            else { throw new AppException("OTP is wrong!"); }
                
#endif
            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.Account.Id, user.Account.FullName, user.Account.UserName, deviceInfo.UDID, skey,
                issuer, audience, ipAddress);

            #region check device

            await CheckDevice(user.Account.Id, refreshToken, deviceInfo);

            #endregion check device

            var jwtToken = _jwtUtils.GenerateToken(user.Account.Id, user.Account.FullName, deviceInfo.UDID, user.Account.UserName);
            
            var res = new LoginResponse();
            res.UserId = user.Account.Id;
            res.SetToken(jwtToken);
            res.SetRefreshToken(refreshToken.Token);

            return Utils.CreateResponseModel(res,1);
        }

        public string GetQRLogin(DeviceInfoRequest deviceInfo)
        {
            var qr = Guid.NewGuid().ToString();
            var qrCode = Utils.Encode64(qr);
            var modelQr = new ModelOtp
            {
                Code = qrCode,
                UDID = deviceInfo.UDID
            };
            _memoryCache.Set(qr, modelQr, CacheTime.QrCode);
            return qrCode;
        }

        public bool VerifyQRCode(Guid userId, LoginByQrCodeRequest model)
        {
            /*var qr = Utils.Decode64(model.QrCode);
            var user = _unitOfWork.Repository<ViagsUser>().FirstOrDefault(s =>
                s.Id == userId && (s.IsDelete == null || s.IsDelete == false));//s.Phone == userPhone
            if (user == null) throw new AppException("User is not found!");

            //Check otp
            ModelOtp modelQr;
            if (!_memoryCache.TryGetValue(qr, out modelQr)) throw new AppException("Phone is wrong or OTP was expired, please re-enter the OTP");
            modelQr.NumCheck += 1;
            if (modelQr.NumCheck > 3) throw new AppException("OTP has been entered too many times, please re-enter the OTP");
            if (modelQr.Expire < DateTime.Now) throw new AppException("OTP was expired, please re-enter the OTP");
            if (modelQr.Code != model.QrCode) throw new AppException("OTP is wrong!");

            modelQr.userVerifiedId = user.Id;
            modelQr.IsVerify = true;*/
            return true;
        }

        //Nhac front end tang t/g time out call api
        public async Task<LoginResponse> WaitVerifyQrCode(LoginByQrCodeRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            /*var qr = Utils.Decode64(model.QrCode);
            while (!CheckVerifyQrCode(qr, deviceInfo.UDID))
            {
                await Task.Delay(2000);
            }

            ModelOtp modelQr;
            if (!_memoryCache.TryGetValue(qr, out modelQr)) throw new AppException("QRCode was expired");
            //Check phone to get user
            var user = _unitOfWork.Repository<ViagsUser>()
                .FirstOrDefault(s => s.Id == modelQr.userVerifiedId && (s.IsDelete == null || s.IsDelete == false));//s.Phone == userPhone
            if (user == null) throw new AppException("Phone is not found or not register!");

            //Response
            var device = _unitOfWork.Repository<Device>().FirstOrDefault(s => s.UDID == deviceInfo.UDID);

            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.Id, user.UserName, deviceInfo.UDID, skey,
                issuer, audience, ipAddress);

            if (device == null)
            {
                device = new Entity.Entities.Device
                {
                    UserId = user.Id,
                    UDID = deviceInfo.UDID,
                    OSVersion = deviceInfo.OSVersion,
                    OSName = deviceInfo.OSName,
                    DeviceType = deviceInfo.DeviceType,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceDescription = deviceInfo.DeviceDescription,
                    IsActive = true,
                    RefreshToken = refreshToken.Token,
                    RfTokenCreateTime = refreshToken.CreateTime,
                    RfTokenExpiryTime = refreshToken.Expires,
                    RfTokenCreatedByIp = refreshToken.CreatedByIp,

                    CreatedDate = DateTime.Now,
                    CreatedUser = user.Id,
                };
                _unitOfWork.Repository<Device>().AddAsync(device);
            }
            else
            {
                device.UserId = user.Id;
                device.IsActive = true;
                device.RefreshToken = refreshToken.Token;
                device.RfTokenCreateTime = refreshToken.CreateTime;
                device.RfTokenExpiryTime = refreshToken.Expires;
                device.RfTokenCreatedByIp = refreshToken.CreatedByIp;
                device.RfTokenRevokedTime = null;
                device.RfTokenRevokedByIp = null;
                device.IsDelete = false;

                device.UpdatedDate = DateTime.Now;
                device.UpdatedUser = user.Id;

                _unitOfWork.Repository<Entity.Entities.Device>().Update(device);
            }
            _unitOfWork.Dispose();

            var jwtToken = _jwtUtils.GenerateToken(user.Id, user.UserName, deviceInfo.UDID);*/
            var res = new LoginResponse();
            /*res.SetToken(jwtToken);
            res.SetRefreshToken(refreshToken.Token);*/

            return res;
        }

        public void ClearBlackListSms(ClearBlackListSmsRequest model)
        {
            var keyBlackList = "BlackListSms";
            if (model.UserPhone != null)
            {
                Dictionary<string, int> dataBlackList;
                if (_memoryCache.TryGetValue(keyBlackList, out dataBlackList))
                {
                    dataBlackList.Remove(model.UserPhone);
                }
            }
            else
                _memoryCache.Remove(keyBlackList);
        }

        private bool CheckVerifyQrCode(string keyGetQrCode, string UDID)
        {
            //Check otp
            ModelOtp qrSaved;
            if (!_memoryCache.TryGetValue(keyGetQrCode, out qrSaved)) throw new AppException("QRCode was expired");
            if (qrSaved.Expire < DateTime.Now) throw new AppException("QRCode was expired");
            if (qrSaved.UDID != UDID) throw new AppException("Wrong device!");

            return qrSaved.IsVerify;
        }

        public async Task<object> GetInfoGoogle(string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<InfoGoogleResponse>(content);

                var userExist = await _unitOfWork.Repository<SysAccount>()
                                                 .FirstOrDefaultAsync(u => (u.Email ?? "").Equals(info.Email) && u.IsDeleted != true);

                if (userExist == null)
                    throw new AppException("Tài khoản chưa tồn tại trong hệ thống, vui lòng liên hệ admin!");

                var jwtToken = _jwtUtils.GenerateToken(userExist.Id, userExist.FullName, Guid.Empty.ToString(), userExist.UserName);
                string? skey = _strJwt.Key;
                string? issuer = _strJwt.Issuer;
                string? audience = _strJwt.Audience;
                var refreshToken = _jwtUtils.GenerateRefreshToken(userExist.Id, userExist.FullName, userExist.UserName, Guid.Empty.ToString(), skey,
                    issuer, audience, "");

                var retUser = new LoginResponse
                {
                    UserId = userExist.Id
                };

                retUser.SetToken(jwtToken);
                retUser.SetRefreshToken(refreshToken.Token);

                return Utils.CreateResponseModel(retUser);
            }
        }

        public async Task<object> GetRedirectUrl(int platform, int? type)
        {
            if (platform == 1 && type == null)
            {
                throw new AppException("Type must be specified for mobile platform");
            }
            
            string redirectUri = platform switch
            {
                0 => _config["MicrosoftGraph:RedirectUri"], // Web
                1 when type == 0 => _config["MicrosoftGraph:MobileRedirectUri:Android"], // Mobile - Android
                1 when type == 1 => _config["MicrosoftGraph:MobileRedirectUri:IOS"],     // Mobile - iOS
                _ => _config["MicrosoftGraph:RedirectUri"]
            };
            var clientId = _config["MicrosoftGraph:ClientId"];
            var tenantId = _config["MicrosoftGraph:TenantId"];
            var scope = _config["MicrosoftGraph:Scope"];
            // var scope = "openid profile email";

            var state = Guid.NewGuid().ToString(); // Optional: bảo vệ CSRF

            var loginUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
                           $"?client_id={clientId}" +
                           $"&response_type=code" +
                           $"&redirect_uri={redirectUri}" +
                           $"&response_mode=query" +
                           $"&scope={scope}" +
                           $"&state={state}";

            return new { url = loginUrl };
        }

        public async Task<LoginResponse> LoginWithMsToken(string code, string ipAddress, int platform = 0, int? type = 0)
        {
            var http = new HttpClient();
            
            var tenantId = _config["MicrosoftGraph:TenantId"];
            
            if (platform == 1 && type == null)
            {
                throw new AppException("Type must be specified for mobile platform");
            }
            
            string redirectUri = platform switch
            {
                0 => _config["MicrosoftGraph:RedirectUri"], // Web
                1 when type == 0 => _config["MicrosoftGraph:MobileRedirectUri:Android"], // Mobile - Android
                1 when type == 1 => _config["MicrosoftGraph:MobileRedirectUri:IOS"],     // Mobile - iOS
                _ => _config["MicrosoftGraph:RedirectUri"]
            };

            // Bước 1: Đổi authorization code để lấy access_token từ Microsoft
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post,
                $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token");

            var formData = new Dictionary<string, string>
            {
                ["client_id"] = _config["MicrosoftGraph:ClientId"],
                ["client_secret"] = _config["MicrosoftGraph:ClientSecret"], 
                ["code"] = code,
                ["redirect_uri"] = redirectUri, 
                ["grant_type"] = "authorization_code",
                ["scope"] = _config["MicrosoftGraph:Scope"]
            };

            tokenRequest.Content = new FormUrlEncodedContent(formData);
            var tokenResponse = await http.SendAsync(tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                throw new Exception($"Không thể lấy access token từ Microsoft: {errorContent}");
            }

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<MicrosoftTokenResponse>(tokenContent);

            if (string.IsNullOrEmpty(tokenData.access_token))
                throw new Exception("Access token không hợp lệ");

            var userInfo = Utils.DecodeMicrosoftAccessToken(tokenData.access_token);
            
            var user = _unitOfWork
                           .Repository<SysAccount>()
                           .FirstOrDefault(user => user.Email.ToLower() == userInfo.Email.ToLower() && user.IsDeleted != true)
                       ?? throw new ApplicationException("U are not allow to access this route");
            
            var jwtToken = _jwtUtils.GenerateToken(user.Id, user.FullName, user.UserName, ipAddress);
            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.Id, user.FullName, user.UserName,ipAddress, skey,
                issuer, audience, ipAddress);

            var retUser = new LoginResponse();
            retUser.UserId = user.Id;
            retUser.SetToken(jwtToken);
            retUser.SetRefreshToken(refreshToken.Token);
            return retUser;
        }


        #region Private Methods

        private bool AuthenticateEmail(string userName, string password)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP1)
            {
                Url = new Uri("https://mail.vietnamairlines.com/EWS/Exchange.asmx"),
                Credentials = new WebCredentials(userName, password)
            };

            // Replace "emailToCheck" with the email address you want to validate
            var emailToCheck = $"{userName}{DOMAIN_NAME}";

            // Perform a search in the Exchange server's directory
            var resolutions = exchangeService.ResolveName(emailToCheck, ResolveNameSearchLocation.DirectoryOnly, true);

            return resolutions.Count > 0;
        }

        private async System.Threading.Tasks.Task CheckDevice(Guid userId, RfTokenResponse refreshToken, DeviceInfoRequest deviceInfo)
        {
            var device = await _unitOfWork.Repository<SysDevice>()
                                               .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive == true &&
                                                                    s.UDID == deviceInfo.UDID);
            if (device == null)
            {
                device = new SysDevice
                {
                    UserId = userId,
                    UDID = deviceInfo.UDID,
                    OSVersion = deviceInfo.OSVersion,
                    OSName = deviceInfo.OSName,
                    DeviceType = deviceInfo.DeviceType,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceDescription = deviceInfo.DeviceDescription,
                    IsActive = true,
                    RefreshToken = refreshToken.Token,
                    RfTokenCreateTime = refreshToken.CreateTime,
                    RfTokenExpiryTime = refreshToken.Expires,
                    RfTokenCreatedByIp = refreshToken.CreatedByIp,
                    CreatedDate = DateTime.Now,
                    CreatedById = userId,
                };
                _unitOfWork.Repository<SysDevice>().Add(device);
            }
            else
            {
                device.UserId = userId;
                device.IsActive = true;
                device.RefreshToken = refreshToken.Token;
                device.RfTokenCreateTime = refreshToken.CreateTime;
                device.RfTokenExpiryTime = refreshToken.Expires;
                device.RfTokenCreatedByIp = refreshToken.CreatedByIp;
                device.RfTokenRevokedTime = null;
                device.RfTokenRevokedByIp = null;
                device.IsDeleted = false;
                device.UpdatedDate = DateTime.Now;
                device.UpdatedById = userId;

                _unitOfWork.Repository<SysDevice>().Update(device);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        private async System.Threading.Tasks.Task CheckUserDevice(Guid userId,string accessToken, RfTokenResponse refreshToken, DeviceInfoRequest deviceInfo)
        {
            var device = await _unitOfWork.Repository<SysUserDevice>()
                                               .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActivate == true &&
                                                                    s.DeviceUUID == deviceInfo.UDID);
            if (device == null)
            {
                device = new SysUserDevice
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DeviceUUID = deviceInfo.UDID,
                    SessionToken = "",
                    SessionExpiredDate = new DateTime(1753, 1, 1),
                    RefreshToken = refreshToken.Token,
                    RefreshExpiredDate = refreshToken.Expires,
                    PushToken = accessToken,
                    LastLogin = DateTime.UtcNow,
                    IsActivate = true,
                    OTPDate = new DateTime(1753, 1, 1),
                    CreatedDate = DateTime.Now,
                    CreatedById = userId,

                };
                _unitOfWork.Repository<SysUserDevice>().Add(device);
            }
            else
            {
                device.UserId = userId;
                device.RefreshToken = refreshToken.Token;
                device.PushToken = accessToken;
                device.IsDeleted = false;
                device.LastLogin = DateTime.UtcNow;
                device.UpdatedDate = DateTime.Now;
                device.UpdatedById = userId;

                _unitOfWork.Repository<SysUserDevice>().Update(device);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<object> GenerateOTPForLogin(DeviceInfoRequest deviceInfo)
        {
            var cooldownKey = $"auth_coolDown_{deviceInfo.UDID}";

            // 1️⃣ Kiểm tra cooldown trước
            if (_memoryCache.TryGetValue(cooldownKey, out _))
            {
                return Utils.CreateResponseModel("Vui lòng chờ trước khi lấy OTP mới.", 0);
            }

            // 2️⃣ Tạo OTP mới
            var newOTP = new OTPResponse
            {
                OTP = Utils.GenerateOneTimeOTP()
            };

            // 3️⃣ Set cooldown (30s)
            _memoryCache.Set(cooldownKey, true, TimeSpan.FromSeconds(30));

            // 4️⃣ Lưu OTP hợp lệ trong 5 phút
            _memoryCache.Set($"auth_{deviceInfo.UDID}_{newOTP.OTP}", true, TimeSpan.FromMinutes(5));

            return Utils.CreateResponseModel(newOTP, 1);
        }

        public async Task<bool> VerifyPermissionOfUser(Guid currentUserId, string code, string crud)
        {

            var results = _unitOfWork.ExecuteStoreProcedure<PermissionResponse>(FintechContext.USP_GetPermission,
            new Hashtable { ["@UserId"] = currentUserId, ["@Code"] = code }).FirstOrDefault();

            if (results == null)
                return false;

            // Normalize CRUD input
            crud = crud?.Trim().ToUpper();

            return crud switch
            {
                "C" => results.C,
                "R" => results.R,
                "U" => results.U,
                "D" => results.D,
                _ => false
            };
        }



        #endregion Private Methods
    }
}
