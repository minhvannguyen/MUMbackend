using System.Net.Mail;
using System.Net;

namespace MUMbackend.Services.Auth
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config["Email:Smtp:Host"];
            var smtpPort = int.Parse(_config["Email:Smtp:Port"]);
            var smtpUser = _config["Email:Smtp:User"];
            var smtpPass = _config["Email:Smtp:Pass"];

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(smtpUser, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}
