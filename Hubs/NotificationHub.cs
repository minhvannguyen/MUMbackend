using Microsoft.AspNetCore.SignalR;

namespace MUMbackend.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(int userId, object notification)
        {
            await Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", notification);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"user_{userId}"
                );
            }

            Console.WriteLine("User connected: " + userId);

            await base.OnConnectedAsync();
        }
    }
}
