using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities;

[Table("Fintech_Sys_Notification")]
public class SysNotification : BaseEntity
{
    public Guid Id { get; set; }
    public string? Source { get; set; } // module gửi: Task, News, Sprint, Project, Department
    public Guid? SourceId { get; set; } // id của module gửi
    public Guid ReceiverId { get; set; }
    public string? Receiver { get; set; }
    public string? Title { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadTime { get; set; }
    public bool IsPinned { get; set; }
    public bool IsSaved { get; set; }
}