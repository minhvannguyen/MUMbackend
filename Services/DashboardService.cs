using MUMbackend.Data;
using MUMbackend.Models;
using Microsoft.EntityFrameworkCore;


namespace MUMbackend.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context) { 
            _context = context;
        }
        public async Task<DashboardResponseDto> GetDashboardAsync()
        {
            var totalSongs = await _context.Songs.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalPlaylists = await _context.Playlists.CountAsync();
            var totalReports = await _context.Reports.CountAsync();

            var recentSongs = await _context.Songs
                .OrderByDescending(s => s.UploadedAt)
                .Take(20)
                .Select(s => new RecentSongDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    CreatedAt = s.UploadedAt,
                })
                .ToListAsync();

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(20)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                })
                .ToListAsync();

            return new DashboardResponseDto
            {
                TotalSongs = totalSongs,
                TotalUsers = totalUsers,
                TotalPlaylists = totalPlaylists,
                TotalReports = totalReports,
                RecentSongs = recentSongs,
                RecentUsers = recentUsers
            };
        }
    }
}
