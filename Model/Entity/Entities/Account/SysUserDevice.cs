using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities;

[Table("Template_Sys_UserDevice")]
public class SysUserDevice : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceUUID { get; set; } = null!;
    public string? AppName { get; set; }
    public string? AppBuild { get; set; }
    public string? AppVersion { get; set; }
    public string? AppLanguage { get; set; }
    public string SessionToken { get; set; } = null!;
    public DateTime SessionExpiredDate { get; set; }
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshExpiredDate { get; set; }
    public string? PushToken { get; set; }
    public string? DevicePlatform { get; set; }
    public string? DeviceOS { get; set; }
    public string? DeviceModel { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceLocation { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool? IsNotify { get; set; }
    public bool? IsActivate { get; set; }
    public bool? IsMainDevice { get; set; }
    public string? OTPCode { get; set; }
    public DateTime? OTPDate { get; set; }
    public int? OTPCount { get; set; }
    public int? OTPFailures { get; set; }
    public string? QRCode { get; set; }
}