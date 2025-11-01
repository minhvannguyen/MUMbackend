using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using GmailAPI = Google.Apis.Gmail.v1.GmailService;

namespace MUMbackend.Services.Auth
{
    public class GmailService
    {
        private readonly string[] Scopes = { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend };
        private readonly string ApplicationName = "EmailVerificationApp";

        public async Task SendOtpEmailAsync(string toEmail, string otp)
{
    try
    {
        UserCredential credential;
        using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true),
                new Google.Apis.Auth.OAuth2.LocalServerCodeReceiver()); // hoặc FixedPortCodeReceiver(5500)
        }

        var service = new GmailAPI(new Google.Apis.Services.BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        var msg = new MailMessage();
        msg.From = new MailAddress("minhvannghuyen@gmail.com", "MUM");
        msg.To.Add(toEmail);
        msg.Subject = "Mã xác thực đăng ký tài khoản";
        msg.Body = $"Xin chào,\n\nMã OTP xác thực của bạn là: {otp}\n\nMã có hiệu lực trong 5 phút.";
        msg.IsBodyHtml = false;

        var mimeMessage = MimeKit.MimeMessage.CreateFromMailMessage(msg);
        var buffer = new MemoryStream();
        mimeMessage.WriteTo(buffer);
        string rawMessage = Convert.ToBase64String(buffer.ToArray())
            .Replace('+', '-').Replace('/', '_').Replace("=", "");

        var gmailMessage = new Message { Raw = rawMessage };

        await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();
    }
    catch (Google.Apis.Auth.OAuth2.Responses.TokenResponseException ex)
    {
        Console.WriteLine($"Token error: {ex.Error} ");
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error while sending OTP: {ex.Message}");
        throw;
    }
}
    }
}
