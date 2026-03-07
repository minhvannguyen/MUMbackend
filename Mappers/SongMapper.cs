using MUMbackend.Dtos;
using MUMbackend.Dtos.Auth;
using MUMbackend.Models;

namespace MUMbackend.Mappers
{
    public static class SongMapper
    {
        public static SongDto ToDto(Song song)
        {
            return new SongDto
            {
                Id = song.Id,
                Title = song.Title,
                ArtistId = song.ArtistId,
                FileUrl = song.FileUrl,
                CoverUrl = song.CoverUrl,
                Duration = song.Duration,
                UploadedAt = song.UploadedAt,
                Views = song.Views,
                Private = song.Private,
            };
        }

        // ✅ Thêm method mới để map với tên nghệ sĩ và thể loại
        public static SongDto ToDtoWithNames(Song song, string? artistName, List<string>? genreNames)
        {
            return new SongDto
            {
                Id = song.Id,
                Title = song.Title,
                ArtistId = song.ArtistId,
                ArtistName = artistName,
                FileUrl = song.FileUrl,
                CoverUrl = song.CoverUrl,
                Duration = song.Duration,
                UploadedAt = song.UploadedAt,
                Views = song.Views,
                Private = song.Private,
                GenreNames = genreNames
            };
        }

        public static SongDto ToDtoWithGenres(Song song, List<int> genreIds)
        {
            return new SongDto
            {
                Id = song.Id,
                Title = song.Title,
                ArtistId = song.ArtistId,
                FileUrl = song.FileUrl,
                CoverUrl = song.CoverUrl,
                Duration = song.Duration,
                UploadedAt = song.UploadedAt,
                Views = song.Views,
                Private = song.Private,
                GenreIds = genreIds
            };
        }

        // ✅ Thêm method mới để map với cả ID và tên
        public static SongDto ToDtoWithFullInfo(Song song, string? artistName, string? artistAvatar, List<int>? genreIds, List<string>? genreNames)
        {
            return new SongDto
            {
                Id = song.Id,
                Title = song.Title,
                ArtistId = song.ArtistId,
                ArtistAvatar = artistAvatar,
                ArtistName = artistName,
                FileUrl = song.FileUrl,
                CoverUrl = song.CoverUrl,
                Duration = song.Duration,
                UploadedAt = song.UploadedAt,
                Views = song.Views,
                Private = song.Private,
                GenreIds = genreIds,
                GenreNames = genreNames
            };
        }

        public static void ToEntityUpdate(Song song, SongDto dto)
        {
            song.Title = dto.Title ?? song.Title;
            song.ArtistId = dto.ArtistId ?? song.ArtistId;
            song.FileUrl = dto.FileUrl ?? song.FileUrl;
            song.CoverUrl = dto.CoverUrl ?? song.CoverUrl;
            song.Duration = dto.Duration ?? song.Duration;
            song.Private = dto.Private ?? song.Private;
        }

        public static Song ToEntityCreate(SongCreateDto dto)
        {
            return new Song
            {
                Title = dto.Title,
                ArtistId = dto.ArtistId,
                FileUrl = dto.FileUrl,
                CoverUrl = dto.CoverUrl,
                Duration = dto.Duration,
                Private = dto.Private,
                UploadedAt = DateTime.Now,
                Views = 0,
            };
        }
    }
}