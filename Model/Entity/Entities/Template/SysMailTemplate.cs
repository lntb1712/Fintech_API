using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities;

[Table("Template_Sys_MailTemplate")]
public class SysMailTemplate : BaseEntity
{
    public Guid Id { get; set; }
    public required string MailCode { get; set; }
    public string? MailTitle { get; set; }
    public required string MailSubject { get; set; }
    public required string MailContent { get; set; }
}