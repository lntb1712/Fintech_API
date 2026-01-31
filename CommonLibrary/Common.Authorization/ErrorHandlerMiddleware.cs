using Common.Authorization.Utils;
using Common.Utils;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Common.Authorization;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILog log = LogManager.GetLogger(typeof(ErrorHandlerMiddleware));


    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    private readonly string[] _headersToLog = new[]
    {
            "accept",
            "X_DEVICE_UDID",
            "X_TIMESTAMP",
            "X_NONCE",
            "X_SIGNATURE",
            "Content-Type",
            "Authorization" // thêm để lấy token nếu cần
        };

    public async Task Invoke(HttpContext context)
    {
        // Intercept the response
        var originalBodyStream = context.Response.Body;
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        string curlCommand = string.Empty;
        try
        {
            curlCommand = await BuildCurlCommand(context);

            await _next(context);

            var response = context.Response;
            var httpStatusCode = (HttpStatusCode)response.StatusCode;

            string userId = GetUserIdFromToken(context) ?? "anonymous";
            string apiRequested = context.Request.Path.Value ?? "";
            string paramsOrBody = curlCommand;
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {

                Common.Utils.Utils.WriteLogRequest(log, "INFO", userId, paramsOrBody ?? "", context.Request);
            }
            if (context.Response.StatusCode >= 400)
            {
                Common.Utils.Utils.WriteLogRequest(log, "ERROR", userId, paramsOrBody ?? "", context.Request);
                var dataRes = new BaseResponse<object>
                {
                    StatusCode = response.StatusCode,
                    Message = httpStatusCode.ToString(),

                };

                response.ContentType = "application/json";
                response.Body.SetLength(0);
                await response.WriteAsync(JsonSerializer.Serialize(dataRes));
            }

        }
        catch (Exception error)
        {
            var response = context.Response;
            // Write log
            var httpStatusCode = (HttpStatusCode)response.StatusCode;
            string userId = GetUserIdFromToken(context) ?? "anonymous";
            var apiRequested = context.Request.Path.Value ?? "";
            var paramsOrBody = curlCommand;
            Common.Utils.Utils.WriteLogRequest(log, "ERROR", userId, paramsOrBody ?? "", context.Request, error.Message);

            var dataRes = new BaseResponse<object>
            {
                Message = error?.Message
            };

            switch (error)
            {
                case AppException e:
                    // Custom application error
                    dataRes.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case KeyNotFoundException e:
                    // Not found error
                    dataRes.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case KeyExistsException e:
                    // Key exists error
                    dataRes.StatusCode = (int)HttpStatusCode.Conflict;
                    break;
                case UnauthorizedAccessException e:
                    // Unauthorized error
                    dataRes.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                default:
                    // Unhandled error
                    dataRes.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            response.StatusCode = dataRes.StatusCode;
            response.ContentType = "application/json";
            await response.WriteAsync(JsonSerializer.Serialize(dataRes));
        }
        finally
        {
            // Copy the modified response to the original response body stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }
    private string GetUserIdFromToken(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        var token = authHeader["Bearer ".Length..].Trim();

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type is "Id" or "sub" or "nameid")?.Value;
        }
        catch
        {
            return null;
        }
    }
    private async Task<string> BuildCurlCommand(HttpContext context)
    {
        var req = context.Request;
        var sb = new StringBuilder();

        // Method + URL
        string url = $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
        sb.Append($"curl -X {req.Method} '{url}'");

        // Headers
        foreach (var header in _headersToLog)
        {
            if (req.Headers.TryGetValue(header, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                sb.Append($" -H '{header}: {value}'");
            }
        }

        // Body (nếu có)
        string bodyContent = string.Empty;
        if ((req.ContentLength ?? 0) > 0 || new[] { "POST", "PUT", "PATCH", "DELETE" }.Contains(req.Method.ToUpper()))
        {
            req.EnableBuffering();
            req.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
            bodyContent = await reader.ReadToEndAsync();
            req.Body.Seek(0, SeekOrigin.Begin); // reset lại để middleware khác đọc được
        }

        if (!string.IsNullOrWhiteSpace(bodyContent))
        {
            string escapedBody = bodyContent.Replace("'", "'\\''");

            if (IsJson(bodyContent))
            {
                try
                {
                    var json = JToken.Parse(bodyContent);
                    escapedBody = json.ToString(Newtonsoft.Json.Formatting.None).Replace("'", "'\\''");
                }
                catch { /* fallback to raw */ }
            }

            sb.Append($" -d '{escapedBody}'");
        }

        // Quan trọng: KHÔNG thêm \ và xuống dòng nữa → chuỗi curl nằm trên 1 dòng duy nhất
        return sb.ToString();
    }
    private static bool IsJson(string input)
    {
        input = input.Trim();
        return (input.StartsWith("{") && input.EndsWith("}")) ||
               (input.StartsWith("[") && input.EndsWith("]"));
    }

}
