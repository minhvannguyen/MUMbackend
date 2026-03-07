using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MUMbackend.Services
{
    public interface IFollowService
    {
        Task FollowAsync(int followerId, int followingId);
        Task UnfollowAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<List<FollowUserDto>> GetFollowersAsync(int userId);
        Task<List<FollowUserDto>> GetFollowingAsync(int userId);
    }

    public class FollowService : IFollowService
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public FollowService(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task FollowAsync(int followerId, int followingId)
        {
            if (followerId == followingId)
                throw new Exception("Không thể follow chính mình");

            var exists = await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (exists) return;

            _context.Follows.Add(new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var message = $" đã follow bạn";

            // 🔔 Trigger Notification
            await _notificationService.CreateNotification(
                followingId,
                followingId,
                message
            );
        }

        public async Task UnfollowAsync(int followerId, int followingId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (follow == null) return;

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<List<FollowUserDto>> GetFollowersAsync(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Select(f => new FollowUserDto
                {
                    UserId = f.Follower.Id,
                    Username = f.Follower.Username,
                    Avatar = f.Follower.AvatarUrl
                })
                .ToListAsync();
        }

        public async Task<List<FollowUserDto>> GetFollowingAsync(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => new FollowUserDto
                {
                    UserId = f.Following.Id,
                    Username = f.Following.Username,
                    Avatar = f.Following.AvatarUrl
                })
                .ToListAsync();
        }
    }


}
