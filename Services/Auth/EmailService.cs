using System.Net;
using System.Net.Mail;

namespace MUMbackend.Services.Auth
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var fromEmail = _config["Email:From"];
                var password = _config["Email:Password"];

                var message = new MailMessage();
                message.From = new MailAddress(fromEmail, "MUM");
                message.To.Add(toEmail);
                message.Subject = "Mã OTP xác thực";
                message.Body = $"Xin chào,\n\nMã OTP của bạn là: {otp}\n\nHết hạn sau 5 phút.";

                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send mail error: {ex.Message}");
                throw;
            }
        }
    }
}