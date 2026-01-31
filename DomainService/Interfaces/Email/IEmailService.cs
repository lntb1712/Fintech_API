namespace DomainService.Interfaces.Email;

public interface IEmailService
{
    string ReadTemplateMail(string fileName);
    void SendSingleEmail(string toEmail, string subject, string body);
    void SendMultipleEmails(List<string> toEmails, string subject, string body);
    Task SendMultipleEmailAppointments(List<string> toEmails, DateTime startTime, string subject, string body, Guid? dayWorkingItemId = null);
}