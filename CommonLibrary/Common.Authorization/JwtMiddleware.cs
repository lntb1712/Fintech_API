using Common.Authorization.Utils;
using Common.Constant;
using Common.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;

namespace Common.Authorization
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _memoryCache;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings,IMemoryCache memoryCache)
        {
            _appSettings = appSettings.Value;     
            _memoryCache = memoryCache;
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var accessToken = context.Request.Query["access_token"];
            var path = context.Request.Path;

            // Nếu là SignalR hub token
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/applicationHub"))
            {
                token = accessToken;
                context.Request.Headers["Authorization"] = "Bearer " + accessToken;
            }

            if (!string.IsNullOrEmpty(token))
            {
                if (_appSettings.EnableMicrosoftCheckJwt)
                {
                    await HandleValidateTokenMicrosoft(context, jwtUtils, token);
                }
                else
                {
                    await HandleValidateTokenCustom(context, jwtUtils, token);
                }
            }

            // Tiếp tục pipeline, endpoint có [AllowAnonymous] sẽ vẫn chạy
            await _next(context);
        }

        #region Private Methods

        private async Task HandleValidateTokenCustom(HttpContext context, IJwtUtils jwtUtils, string token)
        {
            // Check token có bị revoke trong MemoryCache
            if (_memoryCache.TryGetValue($"revoked_{token}", out _))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token revoked");
                return;
            }
            var headerUDID = context.Request.Headers["X_DEVICE_UDID"].ToString();
            var checkJTI = await jwtUtils.ValidateTokenWithJTICheck(token, headerUDID);

            if (checkJTI == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("jti check failed");
                return;
            }
            // Validate token + check revoke + check expire trong DB
            var userId = await jwtUtils.ValidateTokenWithRevokeCheck(token, headerUDID);
            if (userId != null)
            {
                context.Items["User"] = userId;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token invalid or expired");
                return;
            }
        }

        private async Task HandleValidateTokenMicrosoft(HttpContext context, IJwtUtils jwtUtils, string token)
        {
            var account = await jwtUtils.ValidateTokenMicrosoft(token);
            if (account != null)
            {
                context.Items[ContextItemsKey.ID] = account.Id;
                context.Items[ContextItemsKey.Code] = account.Code;
                context.Items[ContextItemsKey.FullName] = account.FullName;
                context.Items[ContextItemsKey.UserName] = account.UserName;
                context.Items[ContextItemsKey.Email] = account.Email;
                context.Items[ContextItemsKey.EnableMicrosoftCheckJwt] = true;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Microsoft token invalid");
                return;
            }
        }

        #endregion
    }
}
