using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace TaskManagmentSystem.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;

                using (var client = new SmtpClient(_settings.Host, _settings.Port))
                {
                    client.EnableSsl = _settings.EnableSsl;
                    client.UseDefaultCredentials = false;
                    if (!string.IsNullOrWhiteSpace(_settings.Username))
                    {
                        client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                    }

                    await client.SendMailAsync(message);
                }
            }
        }
    }
}


