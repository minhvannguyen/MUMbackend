using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Hubs;
using MUMbackend.Models;

namespace MUMbackend.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateNotification(
    int userId,
    int actorUserId,
    string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // 🔎 Lấy thông tin actor
            var actor = await _context.Users
                .Where(u => u.Id == actorUserId)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.AvatarUrl
                })
                .FirstOrDefaultAsync();

            var response = new NotificationResponseDto
            {
                Id = notification.Id,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ActorUserId = actor?.Id ?? 0,
                ActorUsername = actor?.Username ?? "",
                ActorAvatar = actor?.AvatarUrl
            };

            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", response);
        }
    }
}
