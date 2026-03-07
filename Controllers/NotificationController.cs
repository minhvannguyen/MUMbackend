using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Hubs;
using MUMbackend.Models;
using MUMbackend.Services;

namespace MUMbackend.Controllers
{
    [Route("api/notification/[action]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationController(NotificationService service, AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _service = service;
            _context = context;
            _hubContext = hubContext;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetNotifications()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Join(_context.Users,
                    n => n.UserId,
                    u => u.Id,
                    (n, u) => new NotificationResponseDto
                    {
                        Id = n.Id,
                        Message = n.Message,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        ActorUserId = n.UserId,
                        ActorUsername = u.Username,
                        ActorAvatar = u.AvatarUrl
                    })
                .ToListAsync();

            return Ok(ApiResponse<List<NotificationResponseDto>>.Ok(
                "Lấy danh sách thông báo thành công!",
                notifications
            ));
        }

        [HttpPut]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            // realtime push
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("NotificationsMarkedAllRead");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteNotification(int id)
        {
            var userId = GetUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy thông báo"));
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            // realtime update
            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("NotificationDeleted", id);

            return Ok(ApiResponse<string>.Ok("Xoá thông báo thành công"));
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse<string>>> DeleteAllNotifications()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);

            await _context.SaveChangesAsync();

            await _hubContext.Clients
                .User(userId.ToString())
                .SendAsync("AllNotificationsDeleted");

            return Ok(ApiResponse<string>.Ok("Đã xoá toàn bộ thông báo"));
        }
    }
}
