using Common.Utils;
using Model.RequestModel.Common;
using Model.ResponseModel;
using Model.ResponseModel.Common;

namespace DomainService.Interfaces.Account
{
    public interface IAuthService
    {
        Task<object> Login(string userName, string? password, UserDeviceRequest userDevice);

        Task<BaseResponse<LoginResponse>> GetNewTokenByRefreshToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress, string currentAccessToken);

        bool RevokeToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress);

        bool SendOTPLoginToPhone(SendOTPLoginRequest model, DeviceInfoRequest deviceInfo);

        Task<BaseResponse<LoginResponse>> LoginByOTP(LoginByOTPRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
        Task<object> GenerateOTPForLogin(DeviceInfoRequest deviceInfo);

        string GetQRLogin(DeviceInfoRequest deviceInfo);

        bool VerifyQRCode(Guid userId, LoginByQrCodeRequest model);

        Task<LoginResponse> WaitVerifyQrCode(LoginByQrCodeRequest model, DeviceInfoRequest deviceInfo, string ipAddress);

        void ClearBlackListSms(ClearBlackListSmsRequest model);

        Task<object> GetInfoGoogle(string accessToken);

        Task<object> GetRedirectUrl(int platform, int? type);

        Task<LoginResponse> LoginWithMsToken(string token, string ipAddress, int platform, int? type);

        Task<object> RevokeTokenOneDevice(Guid currentUserId, string currentUserName, RefreshTokenRequest model, string UDID, string ipAddress);

        Task<object> RevokeTokenAllDevice(Guid curentUserId, string currentUserName, string ipAddress);

        Task<bool> VerifyPermissionOfUser(Guid currentUserId, string code, string crud);
    }
}