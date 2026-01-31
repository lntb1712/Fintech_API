using Common.UnitOfWork.UnitOfWorkPattern;
using DomainService.Interfaces.Email;
using Entity.Entities.Account;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace Infrastructure.Implements.Email;

public class EmailService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, GraphServiceClient graphClient, IConfiguration configuration) : BaseService(unitOfWork, memoryCache), IEmailService
{
    private readonly GraphServiceClient _graphClient = graphClient;
    private readonly IConfiguration _configuration = configuration;

    public string ReadTemplateMail(string fileName)
    {
        string templateFolderPath =
            Path.Combine(AppContext.BaseDirectory, "ResponseModel", "Templates", $"{fileName}.html");
        return templateFolderPath;
    }

       public void SendSingleEmail(string toEmail, string subject, string body)
        {
            SendEmailViaGraph(toEmail, subject, body).Wait();
        }

        public void SendMultipleEmails(List<string> toEmails, string subject, string body)
        {
            foreach (var email in toEmails)
            {
                if (string.IsNullOrWhiteSpace(email)) continue;
                SendEmailViaGraph(email, subject, body).Wait();
            }
        }

        public async Task SendMultipleEmailAppointments(List<string> toEmails, DateTime startTime, string subject,
            string body, Guid? dayWorkingItemId = null)
        {
            var tasks = toEmails
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => SendAppointmentEmailAsync(email, subject, body, startTime));

            var results = await Task.WhenAll(tasks); 
            
            // if (dayWorkingItemId != null)
            // {
            //     foreach (var (email, eventId) in results)
            //     {
            //         if (eventId == null) continue;
            //
            //         var employee = _unitOfWork.Repository<SysAccount>().FirstOrDefault(x => x.Email == email);
            //         if (employee == null) continue;
            //
            //         var participant = _unitOfWork.Repository<NPPDayWorkingParticipant>()
            //             .FirstOrDefault(x => x.ParticipantId == employee.Id && x.DayWorkingItemId == dayWorkingItemId);
            //
            //         if (participant != null)
            //         {
            //             participant.EventId = eventId;
            //             _unitOfWork.Repository<NPPDayWorkingParticipant>().Update(participant);
            //         }
            //     }
            //
            //     await _unitOfWork.SaveChangesAsync(); 
            // }
        }

    
        private async Task SendEmailViaGraph(string toEmail, string subject, string htmlContent,
            string[]? ccEmails = null)
        {
            var fromEmail = _configuration["GraphApiSettings:SenderEmail"];
            var displayName = _configuration["GraphApiSettings:SenderName"];

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = htmlContent
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient { EmailAddress = new EmailAddress { Address = toEmail } }
                },
                CcRecipients = ccEmails != null
                    ? ccEmails.Select(cc => new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = cc }
                    }).ToList()
                    : new List<Recipient>(),
                From = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = fromEmail,
                        Name = displayName
                    }
                }
            };

            await _graphClient.Users[fromEmail]
                .SendMail
                .PostAsync(new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                });

            Console.WriteLine($"Graph Email sent to {toEmail}");
        }

        public async Task<(string toEmail, string? eventId)> SendAppointmentEmailAsync(string toEmail, string subject, string bodyContent,
            DateTime startTime, DateTime? endTime = null, string[]? ccEmails = null, Guid? dayWorkingItemId = null)
        {
            var fromEmail = _configuration["GraphApiSettings:SenderEmail"];
            var displayName = _configuration["GraphApiSettings:SenderName"];

            var attendees = new List<Attendee>
            {
                new Attendee
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = toEmail,
                        Name = "Người nhận"
                    },
                    Type = AttendeeType.Required
                }
            };

            // Thêm CC nếu có
            if (ccEmails != null)
            {
                foreach (var cc in ccEmails)
                {
                    attendees.Add(new Attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = cc,
                            Name = cc
                        },
                        Type = AttendeeType.Optional
                    });
                }
            }

            var actualEndTime = endTime ?? startTime.AddHours(1);

            var appointmentEvent = new Event
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = bodyContent
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = "Asia/Ho_Chi_Minh"
                },
                End = new DateTimeTimeZone
                {
                    DateTime = actualEndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = "Asia/Ho_Chi_Minh"
                },
                Location = new Location
                {
                    DisplayName = "Họp trực tuyến qua Teams"
                },
                Attendees = attendees,
                IsOnlineMeeting = true,
                OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness
            };

            try
            {
                var createdEvent = await _graphClient.Users[fromEmail].Events.PostAsync(appointmentEvent);
                var eventId = createdEvent?.Id;

                Console.WriteLine($"✅ Đã gửi lời mời họp tới {toEmail}, eventId: {eventId}");
                return (toEmail, eventId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi gửi lời mời họp: {ex.Message}");
            }

            return (null, null);
        }

}