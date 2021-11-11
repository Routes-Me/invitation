using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InvitationsService.Abstraction;
using InvitationsService.Models.ResponseModel;
using InvitationsService.Models.Common;

namespace InvitationsService.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(InvitationsDto invitationDto, string link);
    }
    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly InvitationEmailSettings _emailSettings;
        public IBackgroundTaskQueue Queue { get; }
        public EmailService(IBackgroundTaskQueue queue, ILogger<EmailService> logger, IServiceScopeFactory serviceScopeFactory, IOptions<InvitationEmailSettings> invitationEmailSettings)
        {
            _logger = logger;
            Queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSettings = invitationEmailSettings.Value;
        }

        public Task SendEmailAsync(InvitationsDto invitationDto, string link)
        {
            Queue.QueueBackgroundWorkItem(async token =>
            {
                var guid = Guid.NewGuid().ToString();

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    await SendEmail(invitationDto, link);
                }

                _logger.LogInformation("Queued Background Task {Guid} is complete.", guid);
            });
            return Task.CompletedTask;
        }

        private Task SendEmail(InvitationsDto invitationDto, string link)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.PW),
                EnableSsl = true,
            };

            MailMessage msg = new MailMessage();
            msg.IsBodyHtml = true;
            var stringTemplateHtml = GetContentTextFile("InvitationMail.html").Replace("hrefOfInvitation", link).Replace("inviteeName", invitationDto.RecipientName);
            msg.Body = stringTemplateHtml;

            msg.From = new MailAddress(_emailSettings.From);
            msg.To.Add(invitationDto.Address);
            msg.Subject = _emailSettings.Subject;

            client.Send(msg);
            return Task.CompletedTask;
        }

        private string GetContentTextFile(string filename)
        {
            return File.ReadAllText($"Resources/{filename}");
        }
    }
}