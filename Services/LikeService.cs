using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Models;

namespace MUMbackend.Services
{
    public class LikeService
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public LikeService(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> ToggleLikeAsync(LikeDto dto)
        {
            var existing = await _context.Likes
                .FirstOrDefaultAsync(l =>
                    l.UserId == dto.UserId &&
                    l.TargetType == dto.TargetType &&
                    l.TargetId == dto.TargetId);

            if (existing != null)
            {
                _context.Likes.Remove(existing);
                await _context.SaveChangesAsync();
                return false; // unlike
            }

            var like = new Like
            {
                UserId = dto.UserId,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            int? ownerId = null;
            string message = "";

                var song = await _context.Songs
                    .Where(s => s.Id == dto.TargetId)
                    .Select(s => new { s.ArtistId, s.Title })
                    .FirstOrDefaultAsync();

                if (song != null)
                {
                    ownerId = song.ArtistId;
                    message = $" đã thích bài hát \"{song.Title}\" của bạn";
                }
            

            // 🔔 Trigger notification
            if (ownerId != null && ownerId != dto.UserId)
            {
                await _notificationService.CreateNotification(
                    ownerId.Value,
                    dto.UserId,
                    message
                );
            }

            return true; // liked
        }

        public async Task<bool> IsLikedAsync(long userId, string targetType, long targetId)
        {
            return await _context.Likes.AnyAsync(l =>
                l.UserId == userId &&
                l.TargetType == targetType &&
                l.TargetId == targetId);
        }

        public async Task<int> CountLikesAsync(string targetType, long targetId)
        {
            return await _context.Likes
                .CountAsync(l => l.TargetType == targetType && l.TargetId == targetId);
        }


    }
}
