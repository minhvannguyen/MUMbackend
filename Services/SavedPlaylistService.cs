using Google;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Models;
using MUMbackend.Services;

public class SavedPlaylistService : ISavedPlaylistService
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;

    public SavedPlaylistService(AppDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> ToggleSaveAsync(int userId, int playlistId)
    {
        // 1️⃣ Lấy thông tin playlist
        var playlist = await _context.Playlists
            .Where(p => p.Id == playlistId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.UserId
            })
            .FirstOrDefaultAsync();

        if (playlist == null)
            throw new Exception("Playlist not found");

        // 2️⃣ Kiểm tra đã save chưa
        var isSaved = await _context.SavedPlaylists
            .AnyAsync(sp => sp.UserId == userId && sp.PlaylistId == playlistId);

        // 3️⃣ Nếu đã save → Unsave
        if (isSaved)
        {
            await _context.SavedPlaylists
                .Where(sp => sp.UserId == userId && sp.PlaylistId == playlistId)
                .ExecuteDeleteAsync();

            await _context.Playlists
                .Where(p => p.Id == playlistId)
                .ExecuteUpdateAsync(p =>
                    p.SetProperty(x => x.SaveCount, x => x.SaveCount - 1));

            return false;
        }

        // 4️⃣ Save playlist
        await _context.SavedPlaylists.AddAsync(new SavedPlaylist
        {
            UserId = userId,
            PlaylistId = playlistId
        });

        await _context.Playlists
            .Where(p => p.Id == playlistId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.SaveCount, x => x.SaveCount + 1));

        await _context.SaveChangesAsync();

        // 🔔 Không gửi thông báo nếu tự save playlist của mình
        if (playlist.UserId == userId)
            return true;

        string message = $" đã lưu playlist \"{playlist.Name}\" của bạn";

        // 6️⃣ Trigger realtime notification
        await _notificationService.CreateNotification(
            playlist.UserId, // người nhận
            userId,          // actor
            message
        );

        return true;
    }

    public async Task<bool> IsSavedAsync(int userId, int playlistId)
    {
        return await _context.SavedPlaylists
            .AnyAsync(x => x.UserId == userId && x.PlaylistId == playlistId);
    }

    public async Task<List<PlaylistResponseDto>> GetSavedPlaylistsAsync(int userId)
    {
        return await _context.SavedPlaylists
            .Where(x => x.UserId == userId)
            .Select(x => new PlaylistResponseDto
            {
                Id = x.Playlist.Id,
                UserId = x.Playlist.UserId,
                Name = x.Playlist.Name,
                Description = x.Playlist.Description,
                CoverUrl = x.Playlist.CoverUrl,
                TotalViews = x.Playlist.TotalViews,
                SaveCount = x.Playlist.SaveCount,
                IsPublic = x.Playlist.IsPublic,
                CreatedAt = x.Playlist.CreatedAt,
                SongCount = x.Playlist.PlaylistSongs.Count(),
                IsSaved = true // 👈 luôn true vì đang lấy từ SavedPlaylists
            })
            .ToListAsync();
    }
}