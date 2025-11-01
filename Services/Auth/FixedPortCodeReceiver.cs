using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Requests;

namespace MUMbackend.Services.Auth
{
    public class FixedPortCodeReceiver : ICodeReceiver
    {
        private readonly int _port;

        public FixedPortCodeReceiver(int port = 5500)
        {
            _port = port;
        }

        public string RedirectUri => $"http://127.0.0.1:{_port}/authorize/";

        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(
            AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken)
        {
            string urlWithRedirect = url.Build().ToString();

            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(RedirectUri);
                listener.Start();

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = urlWithRedirect,
                    UseShellExecute = true
                });

                var context = await listener.GetContextAsync();
                var response = context.Response;
                string responseString = "<html><body>Authorization successful. You can close this tab now.</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();

                var result = new AuthorizationCodeResponseUrl(context.Request.Url.Query);
                if (!string.IsNullOrEmpty(result.Error))
                    throw new Exception($"OAuth authorization error: {result.Error}");

                return result;
            }
        }
    }
}
