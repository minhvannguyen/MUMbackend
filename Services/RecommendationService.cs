using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Mappers;
using MUMbackend.Models;

namespace MUMbackend.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _context;

        public RecommendationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<SongDto>> GetRecommendationsAsync(
    int userId,
    int page = 1,
    int pageSize = 25)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;

            var resultSongs = new List<Song>();

            var hasHistory = await _context.ListeningHistories
                .AnyAsync(x => x.UserId == userId);

            if (!hasHistory)
            {
                var trendingCold = await _context.Songs
                    .Where(s => !s.Private)
                    .OrderByDescending(s => s.Views)
                    .ToListAsync();

                var totalCold = trendingCold.Count;

                var pagedCold = trendingCold
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var coldDtos = await MapSongsToDto(pagedCold);

                return new PagedResponse<SongDto>(
                    coldDtos,
                    page,
                    pageSize,
                    totalCold
                );
            }

            // 1️⃣ 5 liked random
            var likedSongIds = await _context.Likes
                .Where(x => x.UserId == userId && x.TargetType == "Song")
                .Select(x => x.TargetId)
                .ToListAsync();

            var likedSongs = await _context.Songs
                .Where(s => likedSongIds.Contains(s.Id) && !s.Private)
                .ToListAsync();

            resultSongs.AddRange(likedSongs
                .OrderBy(x => Guid.NewGuid())
                .Take(5));

            // 2️⃣ 5 most listened
            var mostListenedIds = await _context.ListeningHistories
                .Where(x => x.UserId == userId)
                .GroupBy(x => x.SongId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .ToListAsync();

            var mostListenedSongs = await _context.Songs
                .Where(s => mostListenedIds.Contains(s.Id))
                .ToListAsync();

            resultSongs.AddRange(mostListenedSongs.Take(5));

            // 3️⃣ trending
            var trending = await _context.Songs
                .Where(s => !s.Private)
                .OrderByDescending(s => s.Views)
                .ToListAsync();

            resultSongs.AddRange(trending.Take(10));

            // 4️⃣ new by favorite genres
            var topGenreIds = await _context.ListeningHistories
                .Where(x => x.UserId == userId)
                .Join(_context.SongGenres,
                      lh => lh.SongId,
                      sg => sg.SongId,
                      (lh, sg) => sg.GenreId)
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => g.Key)
                .ToListAsync();

            var newSongs = await _context.SongGenres
                .Where(sg => topGenreIds.Contains(sg.GenreId))
                .Join(_context.Songs,
                      sg => sg.SongId,
                      s => s.Id,
                      (sg, s) => s)
                .Where(s => !s.Private)
                .OrderByDescending(s => s.UploadedAt)
                .ToListAsync();

            resultSongs.AddRange(newSongs.Take(5));

            // Remove duplicate
            var distinctSongs = resultSongs
                .DistinctBy(s => s.Id)
                .ToList();

            var totalItems = distinctSongs.Count;

            var pagedSongs = distinctSongs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var songDtos = await MapSongsToDto(pagedSongs);

            return new PagedResponse<SongDto>(
                songDtos,
                page,
                pageSize,
                totalItems
            );
        }

        // ✅ Mapping tối ưu tránh N+1
        private async Task<List<SongDto>> MapSongsToDto(List<Song> songs)
        {
            var songIds = songs.Select(s => s.Id).ToList();
            var artistIds = songs.Select(s => s.ArtistId).Distinct().ToList();

            var artists = await _context.Users
                .Where(u => artistIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var songGenres = await _context.SongGenres
                .Where(sg => songIds.Contains(sg.SongId))
                .ToListAsync();

            var genreIds = songGenres.Select(sg => sg.GenreId).Distinct().ToList();

            var genres = await _context.Genres
                .Where(g => genreIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id);

            var result = new List<SongDto>();

            foreach (var song in songs)
            {
                var artist = artists.ContainsKey(song.ArtistId)
                    ? artists[song.ArtistId]
                    : null;

                var currentSongGenres = songGenres
                    .Where(sg => sg.SongId == song.Id)
                    .Select(sg => sg.GenreId)
                    .ToList();

                var genreNames = currentSongGenres
                    .Where(id => genres.ContainsKey(id))
                    .Select(id => genres[id].Name)
                    .ToList();

                result.Add(SongMapper.ToDtoWithFullInfo(
                    song,
                    artist?.Username,
                    artist?.AvatarUrl ?? "",
                    currentSongGenres,
                    genreNames
                ));
            }

            return result;
        }
        public async Task<InfiniteResponse<SongDto>> GetRecommendationsAsync(
            int userId,
            double? lastScore,
            int? lastSongId,
            int limit = 20)
        {
            var now = DateTime.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);

            // 🔥 1️⃣ Lấy top genre user hay nghe
            var topGenreIds = await _context.ListeningHistories
                .Where(x => x.UserId == userId)
                .Join(_context.SongGenres,
                      lh => lh.SongId,
                      sg => sg.SongId,
                      (lh, sg) => sg.GenreId)
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToListAsync();
            // 🔥 2️⃣ Build query tính score hoàn toàn trong SQL
            var query = _context.Songs
            .Where(s => !s.Private)
            .Select(s => new
            {
                Song = s,

                LikeScore = _context.Likes
                    .Any(l => l.UserId == userId
                           && l.TargetType == "Song"
                           && l.TargetId == s.Id)
                    ? 50
                    : 0,

                ListenScore = _context.ListeningHistories
                    .Where(lh => lh.UserId == userId && lh.SongId == s.Id)
                    .Count() * 5,

                GenreScore = _context.SongGenres
                    .Any(sg => sg.SongId == s.Id &&
                               topGenreIds.Contains(sg.GenreId))
                    ? 10
                    : 0,

                ViewScore = s.Views / 1000.0,

                NewScore = s.UploadedAt >= sevenDaysAgo ? 5 : 0
            })
    .Select(x => new
    {
        x.Song,
        Score = x.LikeScore +
                            x.ListenScore +
                            x.GenreScore +
                            x.ViewScore +
                            x.NewScore
    });

            // 🔥 3️⃣ Cursor filter (infinite scroll)
            if (lastScore.HasValue && lastSongId.HasValue)
            {
                query = query.Where(x =>
                    x.Score < lastScore.Value ||
                    (x.Score == lastScore.Value && x.Song.Id < lastSongId.Value));
            }
            // 🔥 4️⃣ Order + Take
            var items = await query
                        .OrderByDescending(x => x.Score)
                        .ThenByDescending(x => x.Song.Id)
                        .Take(limit)
                        .ToListAsync();

            var songs = items.Select(x => x.Song).ToList();

            // 🔥 5️⃣ Map DTO tránh N+1
            var songDtos = await MapSongsToDto(songs);

            var lastItem = items.LastOrDefault();

            return new InfiniteResponse<SongDto>
            {
                Items = songDtos,
                NextScore = lastItem?.Score,
                NextSongId = lastItem?.Song.Id,
                HasMore = items.Count == limit
            };
        }

        public async Task<(List<SongDto> Items, double? NextScore, int? NextId, bool HasMore)>
            GetGuestRecommendationsAsync(double? lastScore, int? lastSongId, int limit = 20)
        {
            var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);

            // 🔥 Popular query
            var popularQuery =
                from s in _context.Songs
                where !s.Private
                let likeCount = _context.Likes.Count(l => l.TargetType == "Song" && l.TargetId == s.Id)
                let score = (double)s.Views + (likeCount * 5)
                select new
                {
                    Song = s,
                    Score = score
                };

            if (lastScore.HasValue && lastSongId.HasValue)
            {
                popularQuery = popularQuery
                    .Where(x => x.Score < lastScore.Value
                             || (x.Score == lastScore.Value && x.Song.Id < lastSongId.Value));
            }

            var popularSongs = await popularQuery
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Song.Id)
                .Take((int)(limit * 0.8)) // 80% popular
                .ToListAsync();

            // 🆕 New songs
            var newSongs = await _context.Songs
                .Where(s => !s.Private && s.UploadedAt >= twoWeeksAgo)
                .OrderByDescending(s => s.UploadedAt)
                .Take(limit - popularSongs.Count)
                .ToListAsync();

            // 🎧 Trộn 3-4 popular + 1 new
            var finalList = new List<Song>();
            int newIndex = 0;

            for (int i = 0; i < popularSongs.Count; i++)
            {
                finalList.Add(popularSongs[i].Song);

                if ((i + 1) % 3 == 0 && newIndex < newSongs.Count)
                {
                    finalList.Add(newSongs[newIndex]);
                    newIndex++;
                }
            }

            finalList = finalList.DistinctBy(s => s.Id).Take(limit).ToList();

            var dtos = await MapSongsToDto(finalList);

            var lastItem = popularSongs.LastOrDefault();

            return (
                dtos,
                lastItem?.Score,
                lastItem?.Song.Id,
                popularSongs.Count == (int)(limit * 0.8)
            );
        }
    }
}