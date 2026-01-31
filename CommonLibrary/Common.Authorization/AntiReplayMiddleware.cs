using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Authorization
{
    public class AntiReplayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;
        private readonly string _secretKey = "thanhbinhle1712"; // Lấy từ Config

        public AntiReplayMiddleware(RequestDelegate next, IMemoryCache memoryCache)
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task Invoke(HttpContext context)
        {
            // 1. Bỏ qua các request không cần check (ví dụ file tĩnh, swagger...)
            if (IsWhitelisted(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // 2. Lấy thông tin từ Header
            if (!context.Request.Headers.TryGetValue("X_TIMESTAMP", out var timestampVal) ||
                !context.Request.Headers.TryGetValue("X_NONCE", out var nonceVal) )
                //||
                //!context.Request.Headers.TryGetValue("X-Signature", out var signatureVal))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing Security Headers");
                return;
            }

            long timestamp = long.Parse(timestampVal);
            string nonce = nonceVal.ToString();
            //string clientSignature = signatureVal.ToString();

            // 3. Kiểm tra thời gian (Time Window) - Ví dụ: 3 phút
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Math.Abs(now - timestamp) > 180)
            {
                context.Response.StatusCode = 400; // Hoặc 401/403
                await context.Response.WriteAsync("Request Expired (Replay Attack)");
                return;
            }

            // 4. Kiểm tra Nonce (Chống lặp lại trong thời gian ngắn)
            string cacheKey = $"nonce_{nonce}";
            if (_memoryCache.TryGetValue(cacheKey, out _))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Nonce used (Replay Attack)");
                return;
            }

            // 5. Verify Signature (Tính toàn vẹn dữ liệu)
            // Cần enable buffering để đọc body nhiều lần
            context.Request.EnableBuffering();

            string requestBody = "";
            // Chỉ đọc body nếu có (POST, PUT...)
            if (context.Request.ContentLength > 0)
            {
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                // Reset vị trí đọc về 0 để Controller phía sau có thể đọc lại
                context.Request.Body.Position = 0;
            }

            //// Tạo chuỗi raw để hash: Method + Path + Timestamp + Nonce + Body
            //// Cấu trúc này Client và Server phải thống nhất 100%
            //string rawData = $"{context.Request.Method.ToUpper()}{context.Request.Path}{timestamp}{nonce}{requestBody}";

            //string computedSignature = ComputeHmac(rawData, _secretKey);

            //if (computedSignature != clientSignature)
            //{
            //    context.Response.StatusCode = 401;
            //    await context.Response.WriteAsync("Invalid Signature");
            //    return;
            //}

            // 6. Lưu Nonce vào cache để đánh dấu đã dùng
            // Thời gian hết hạn cache = Time Window (ví dụ 3 phút)
            _memoryCache.Set(cacheKey, true, TimeSpan.FromSeconds(180));

            // Tiếp tục request
            await _next(context);
        }

        private string ComputeHmac(string message, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var hash = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private bool IsWhitelisted(string path)
        {
            // Cho phép Swagger hoặc các trang health check đi qua
            if (path.Contains("/swagger") || path.Contains("/health")) return true;
            return false;
        }
    }
}
