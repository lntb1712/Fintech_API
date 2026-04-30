using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Model.RequestModel;
using Model.RequestModel.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;

namespace Common.Authorization
{

    public class CustomHeaderParameter : IOperationFilter  // Hoặc tên class của bạn
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            // 1. Existing: X_DEVICE_UDID (optional, with default GUID)
            var deviceUdidParam = new OpenApiParameter
            {
                Name = APIResourceRequest.X_DEVICE_UDID,
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("00000000-0000-0000-0000-000000000000")  // Default test UUID
                },
               
            };
            operation.Parameters.Insert(0, deviceUdidParam);
//#if !DEBUG
//            // 2. NEW: X_TIMESTAMP (required, Unix seconds)
//            var timestampParam = new OpenApiParameter
//            {
//                Name = "X_TIMESTAMP",  // Hoặc thêm constant vào APIResourceRequest nếu chưa có
//                In = ParameterLocation.Header,
//                Required = true,
//                Schema = new OpenApiSchema
//                {
//                    Type = "integer",
//                    Example = new OpenApiLong(DateTimeOffset.Now.ToUnixTimeSeconds())  // Dynamic example: current time
//                },
                
//            };

//            // 3. NEW: X_NONCE (required, unique random string)
//            var nonceParam = new OpenApiParameter
//            {
//                Name = "X_NONCE",
//                In = ParameterLocation.Header,
//                Required = true,
              
//                Schema = new OpenApiSchema
//                {
//                    Type = "string",
//                    MinLength = 1,
//                    MaxLength = 255,
//                    Example = new OpenApiString(Guid.NewGuid().ToString("N"))  // Example: no-hyphen GUID
//                },
                
//            };

//            // 4. NEW: X_SIGNATURE (required, HMAC-SHA256 base64)
//            var signatureParam = new OpenApiParameter
//            {
//                Name = "X_SIGNATURE",
//                In = ParameterLocation.Header,
//                Required = true,
//                Description = "HMAC-SHA256 signature (base64-encoded) of the message '{username}{password}{UDID}{timestamp}{nonce}' using shared secret key. Validates request integrity.",
//                Schema = new OpenApiSchema
//                {
//                    Type = "string",
//                    Pattern = "^[A-Za-z0-9+/]*={0,2}$",  // Base64 pattern
//                    MinLength = 1,
//                    Example = new OpenApiString("SampleBase64HMACSignatureHere==")  // Placeholder; in real, compute it
//                },
               
//            };

//            // Add all parameters at the beginning (order: UDID first, then security fields)

//            operation.Parameters.Insert(1, timestampParam);
//            operation.Parameters.Insert(2, nonceParam);
//            operation.Parameters.Insert(3, signatureParam);
//#endif
        }
    }
}

