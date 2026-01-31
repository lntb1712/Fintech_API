using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities;

[Table("Template_Sys_AppVersion")]
public class SysAppVersion
{
    public Guid Id { get; set; }

    public string? AppName { get; set; }

    public string AppPlatform { get; set; } = null!;

    public int MinVersion { get; set; }

    public int AppVersion { get; set; }

    public string VersionName { get; set; } = null!;

    public string? ReleaseNotes { get; set; }

    public string? DownloadPath { get; set; }

    public bool IsAwaitingApproval { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool IsForceDev { get; set; }
    public bool IsForceUpdate { get; set; }
}