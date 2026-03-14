using MUMbackend.Data;
using MUMbackend.Models;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Dtos;
using MUMbackend.Mappers;

namespace MUMbackend.Services
{
    public class PlaylistService
    {
        private readonly AppDbContext _context;

        public PlaylistService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Playlist>> GetAllAsync()
        {
            return await _context.Playlists
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Playlist?> GetByIdAsync(int id)
        {
            return await _context.Playlists.FindAsync(id);
        }

        public async Task<Playlist> CreateAsync(Playlist playlist)
        {
            // 🔸 Kiểm tra trùng tên playlist của cùng user
            bool exists = await _context.Playlists
                .AnyAsync(p => p.UserId == playlist.UserId && p.Name == playlist.Name);

            if (exists)
            {
                throw new InvalidOperationException("Playlist name already exists for this user.");
            }

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
            return playlist;
        }

        // PlaylistService.cs
        public async Task<(IEnumerable<PlaylistResponseDto> Items, int TotalItems)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = from p in _context.Playlists
                        join u in _context.Users
                        on p.UserId equals u.Id
                        select new PlaylistResponseDto
                        {
                            Id = p.Id,
                            UserId = p.UserId,
                            Name = p.Name,
                            Description = p.Description,
                            CoverUrl = p.CoverUrl,
                            IsPublic = p.IsPublic,
                            SaveCount = p.SaveCount,
                            TotalViews = p.TotalViews,
                            SongCount = p.PlaylistSongs.Count(),
                            CreatedAt = p.CreatedAt,
                            Creator = u.Username   // 🔥 lấy tên creator
                        };

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<PagedResponse<object>> GetByUserIdAsync(
    int ownerUserId,   // user tạo playlist
    int page,
    int pageSize,
    int? currentUserId  // user đang đăng nhập (để check isSaved)
)
        {
            var query = _context.Playlists
                .Where(p => p.UserId == ownerUserId)
                .AsNoTracking();

            var totalItems = await query.CountAsync();

            if (totalItems == 0)
            {
                return new PagedResponse<object>(
                    new List<object>(),
                    page,
                    pageSize,
                    0);
            }

            var playlists = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.Name,
                    p.Description,
                    p.CoverUrl,
                    p.CreatedAt,
                    p.IsPublic,
                    p.TotalViews,
                    p.SaveCount,
                    SongCount = p.PlaylistSongs.Count()
                })
                .ToListAsync();

            var playlistIds = playlists.Select(p => p.Id).ToList();

            HashSet<int> savedIds = new();

            if (currentUserId.HasValue)
            {
                savedIds = (await _context.SavedPlaylists
                    .Where(sp => sp.UserId == currentUserId.Value &&
                                 playlistIds.Contains(sp.PlaylistId))
                    .Select(sp => sp.PlaylistId)
                    .ToListAsync())
                    .ToHashSet();
            }

            var result = playlists.Select(p => new
            {
                id = p.Id,
                userId = p.UserId,
                name = p.Name,
                description = p.Description,
                coverUrl = p.CoverUrl,
                createdAt = p.CreatedAt,
                isPublic = p.IsPublic,
                totalViews = p.TotalViews,
                saveCount = p.SaveCount,
                songCount = p.SongCount,
                isSaved = currentUserId.HasValue && savedIds.Contains(p.Id)
            });

            return new PagedResponse<object>(
                result,
                page,
                pageSize,
                totalItems);
        }

        public async Task<bool> UpdateAsync(int id, PlaylistUpdateDto playlistUpdateDto)
        {
            var existing = await _context.Playlists.FindAsync(id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(playlistUpdateDto);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null) return false;

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddSongToPlaylistAsync(int playlistId, int songId)
        {
            var exists = await _context.PlaylistSongs
                .AnyAsync(ps => ps.PlaylistId == playlistId &&
                                ps.SongId == songId);

            if (exists)
                throw new Exception("Bài hát đã tồn tại.");

            await _context.PlaylistSongs.AddAsync(new PlaylistSongs
            {
                PlaylistId = playlistId,
                SongId = songId
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            var deleted = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId &&
                             ps.SongId == songId)
                .ExecuteDeleteAsync();

            if (deleted == 0)
                throw new Exception("Bài hát không tồn tại trong playlist.");
        }

        public async Task<IEnumerable<SongDto>> GetSongsInPlaylistAsync(int playlistId)
        {
            // 1. Lấy ra danh sách SongId từ bảng PlaylistSongs
            var songIds = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .Select(ps => ps.SongId)
                .ToListAsync();

            if (!songIds.Any())
                return new List<SongDto>();


            // 2. Lấy danh sách bài hát và thông tin nghệ sĩ
            var songs = await _context.Songs
                .Where(s => songIds.Contains(s.Id))
                .Select(s => new
                {
                    Song = s,
                    ArtistName = _context.Users
                        .Where(u => u.Id == s.ArtistId)
                        .Select(u => u.Username)
                        .FirstOrDefault(),
                    ArtistAvatar = _context.Users
                        .Where(u => u.Id == s.ArtistId)
                        .Select(u => u.AvatarUrl)
                        .FirstOrDefault()
                })
                .ToListAsync();


            // 3. Lấy toàn bộ thể loại của các bài hát trong 1 query
            var genresMap = await _context.SongGenres
                .Where(sg => songIds.Contains(sg.SongId))
                .GroupBy(sg => sg.SongId)
                .Select(g => new
                {
                    SongId = g.Key,
                    GenreIds = g.Select(x => x.GenreId).ToList()
                })
                .ToListAsync();

            // Tạo dictionary để lookup nhanh
            var genreIdLookup = genresMap.ToDictionary(x => x.SongId, x => x.GenreIds);

            // Lấy tên genre
            var allGenreIds = genresMap.SelectMany(g => g.GenreIds).Distinct().ToList();

            var genreNameLookup = await _context.Genres
                .Where(g => allGenreIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g.Name);


            // 4. Build SongDto trả về
            var songDtos = new List<SongDto>();

            foreach (var item in songs)
            {
                var s = item.Song;

                var genreIds = genreIdLookup.ContainsKey(s.Id)
                    ? genreIdLookup[s.Id]
                    : new List<int>();

                var genreNames = genreIds
                    .Where(id => genreNameLookup.ContainsKey(id))
                    .Select(id => genreNameLookup[id])
                    .ToList();

                songDtos.Add(SongMapper.ToDtoWithFullInfo(
                    s,
                    item.ArtistName ?? "Không rõ",
                    item.ArtistAvatar,
                    genreIds,
                    genreNames
                ));
            }

            return songDtos;
        }

        // 🔥 Lấy top playlist theo tổng views các bài hát
        public async Task<PagedResponse<object>> GetTopPlaylistsAsync(
    int page,
    int pageSize,
    int? userId)
        {
            var query = _context.Playlists
                .Where(p => p.IsPublic) // nên lọc public
                .AsNoTracking();

            var totalItems = await query.CountAsync();

            if (totalItems == 0)
                return new PagedResponse<object>(
                    new List<object>(),
                    page,
                    pageSize,
                    0);

            var playlists = await query
                .OrderByDescending(p => p.TotalViews)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.Name,
                    p.Description,
                    p.CoverUrl,
                    p.CreatedAt,
                    p.IsPublic,
                    p.TotalViews,
                    p.SaveCount,
                    SongCount = p.PlaylistSongs.Count()
                })
                .ToListAsync();

            var playlistIds = playlists.Select(p => p.Id).ToList();

            HashSet<int> savedIds = new();

            if (userId.HasValue)
            {
                savedIds = (await _context.SavedPlaylists
                    .Where(sp => sp.UserId == userId.Value &&
                                 playlistIds.Contains(sp.PlaylistId))
                    .Select(sp => sp.PlaylistId)
                    .ToListAsync())
                    .ToHashSet();
            }

            var result = playlists.Select(p => new
            {
                id = p.Id,
                userId = p.UserId,
                name = p.Name,
                description = p.Description,
                coverUrl = p.CoverUrl,
                createdAt = p.CreatedAt,
                isPublic = p.IsPublic,
                totalViews = p.TotalViews,
                saveCount = p.SaveCount,
                songCount = p.SongCount,
                isSaved = userId.HasValue && savedIds.Contains(p.Id)
            });

            return new PagedResponse<object>(
                result,
                page,
                pageSize,
                totalItems);
        }

        public async Task IncreaseSongViewAsync(int songId, int? playlistId = null)
        {
            // 1️⃣ tăng view bài hát
            await _context.Songs
                .Where(s => s.Id == songId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Views, x => x.Views + 1));

            // 2️⃣ nếu nghe từ playlist → tăng playlist view
            if (playlistId.HasValue)
            {
                await _context.Playlists
                    .Where(p => p.Id == playlistId.Value)
                    .ExecuteUpdateAsync(p =>
                        p.SetProperty(x => x.TotalViews, x => x.TotalViews + 1));
            }
        }



        public async Task<bool> IncrementViewAsync(int playlistId)
        {
            if (playlistId <= 0)
                return false;

            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Playlists
            SET TotalViews = TotalViews + 1
            WHERE Id = {playlistId}
        ");

            return rows > 0;
        }

        public async Task<PagedResponse<object>> SearchByNameAsync(
    string keyword,
    int page,
    int pageSize,
    int? userId)
        {
            keyword = keyword?.Trim();

            var query = _context.Playlists
                .Where(p => p.IsPublic &&
                            EF.Functions.Like(p.Name, $"%{keyword}%"))
                .AsNoTracking();

            var totalItems = await query.CountAsync();

            var playlists = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    userId = p.UserId,
                    creator = p.User.Username, // 🔥 lấy creator
                    name = p.Name,
                    description = p.Description,
                    coverUrl = p.CoverUrl,
                    createdAt = p.CreatedAt,
                    isPublic = p.IsPublic,
                    totalViews = p.TotalViews,
                    saveCount = p.SaveCount,

                    songCount = p.PlaylistSongs.Count(),

                    isSaved = userId != null &&
                        _context.SavedPlaylists
                            .Any(sp => sp.UserId == userId &&
                                       sp.PlaylistId == p.Id)
                })
                .ToListAsync();

            return new PagedResponse<object>(
                playlists,
                page,
                pageSize,
                totalItems);
        }
    }
    }
