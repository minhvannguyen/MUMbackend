using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Models;
using MUMbackend.Dtos;
using MUMbackend.Mappers;
using MUMbackend.Services;
using Microsoft.AspNetCore.Authorization;
using MUMbackend.Dtos.Auth;

namespace MUMbackend.Controllers
{
    [Route("api/songs/[action]")]
    [ApiController]
    public class SongController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SongController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả bài hát với pagination
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<SongDto>>>> GetNewSongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1)
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("Số trang phải lớn hơn 0!"));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("Số phần tử mỗi trang phải từ 1 đến 100!"));

            var totalItems = await _context.Songs.CountAsync();

            if (totalItems == 0)
            {
                var emptyPagedResponse = new PagedResponse<SongDto>(new List<SongDto>(), page, pageSize, 0);
                return Ok(ApiResponse<PagedResponse<SongDto>>.Ok("Không có bài hát nào!", emptyPagedResponse));
            }

            var songs = await _context.Songs
                .OrderByDescending(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var songDtos = new List<SongDto>();

            foreach (var song in songs)
            {
                // Lấy tên và avatar nghệ sĩ
                var artist = await _context.Users.FindAsync(song.ArtistId);
                var artistName = artist?.Username;
                var artistAvatar = artist?.AvatarUrl ?? "";

                // Lấy thể loại
                var genreIds = await _context.SongGenres
                    .Where(sg => sg.SongId == song.Id)
                    .Select(sg => sg.GenreId)
                    .ToListAsync();

                var genreNames = new List<string>();
                if (genreIds.Any())
                {
                    genreNames = await _context.Genres
                        .Where(g => genreIds.Contains(g.Id))
                        .Select(g => g.Name)
                        .ToListAsync();
                }

                // Truyền thêm artistAvatar
                songDtos.Add(SongMapper.ToDtoWithFullInfo(
                    song,
                    artistName,
                    artistAvatar,   // ← CHUYỀN THÊM
                    genreIds,
                    genreNames
                ));
            }

            var pagedResponse = new PagedResponse<SongDto>(songDtos, page, pageSize, totalItems);
            return Ok(ApiResponse<PagedResponse<SongDto>>.Ok("Lấy danh sách bài hát thành công!", pagedResponse));
        }


        // ✅ Lấy bài hát theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SongDto>>> GetSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound(ApiResponse<SongDto>.Fail("Không tìm thấy bài hát!"));

            // Tăng views khi xem
            song.Views++;
            await _context.SaveChangesAsync();

            // ✅ Lấy tên nghệ sĩ
            var artist = await _context.Users.FindAsync(song.ArtistId);
            var artistName = artist?.Username;
            var artistAvatar = artist?.AvatarUrl ?? "";

            // ✅ Lấy danh sách GenreIds và GenreNames
            var genreIds = await _context.SongGenres
                .Where(sg => sg.SongId == id)
                .Select(sg => sg.GenreId)
                .ToListAsync();

            var genreNames = new List<string>();
            if (genreIds.Any())
            {
                genreNames = await _context.Genres
                    .Where(g => genreIds.Contains(g.Id))
                    .Select(g => g.Name)
                    .ToListAsync();
            }

            return Ok(ApiResponse<SongDto>.Ok("Lấy thông tin bài hát thành công!",
                SongMapper.ToDtoWithFullInfo(song, artistName, artistAvatar, genreIds, genreNames)));
        }

        // ✅ Lấy bài hát theo ArtistId
        [HttpGet("artist/{artistId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SongDto>>>> GetSongsByArtist(int artistId)
        {
            var songs = await _context.Songs
                .Where(s => s.ArtistId == artistId)
                .ToListAsync();

            if (!songs.Any())
                return Ok(ApiResponse<IEnumerable<SongDto>>.Ok("Không có bài hát nào của nghệ sĩ này!", new List<SongDto>()));

            // ✅ Lấy tên nghệ sĩ
            var artist = await _context.Users.FindAsync(artistId);
            var artistName = artist?.Username;
            var artistAvatar = artist?.AvatarUrl ?? "";

            // ✅ Load tên thể loại cho từng bài hát
            var songDtos = new List<SongDto>();
            foreach (var song in songs)
            {
                var genreIds = await _context.SongGenres
                    .Where(sg => sg.SongId == song.Id)
                    .Select(sg => sg.GenreId)
                    .ToListAsync();

                var genreNames = new List<string>();
                if (genreIds.Any())
                {
                    genreNames = await _context.Genres
                        .Where(g => genreIds.Contains(g.Id))
                        .Select(g => g.Name)
                        .ToListAsync();
                }

                songDtos.Add(SongMapper.ToDtoWithFullInfo(song, artistName, artistAvatar, genreIds, genreNames));
            }

            return Ok(ApiResponse<IEnumerable<SongDto>>.Ok("Lấy danh sách bài hát thành công!", songDtos));
        }

        // ✅ Tìm kiếm bài hát theo tiêu đề
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PagedResponse<SongDto>>>> SearchSongs(
    [FromQuery] string keyword,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("Từ khóa tìm kiếm không được để trống!"));

            var query = _context.Songs
                .Where(s => s.Title.Contains(keyword));

            var totalItems = await query.CountAsync();

            var songs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var songDtos = new List<SongDto>();

            foreach (var song in songs)
            {
                var artist = await _context.Users.FindAsync(song.ArtistId);
                var artistName = artist?.Username;
                var artistAvatar = artist?.AvatarUrl ?? "";

                var genreIds = await _context.SongGenres
                    .Where(sg => sg.SongId == song.Id)
                    .Select(sg => sg.GenreId)
                    .ToListAsync();

                var genreNames = await _context.Genres
                    .Where(g => genreIds.Contains(g.Id))
                    .Select(g => g.Name)
                    .ToListAsync();

                songDtos.Add(
                    SongMapper.ToDtoWithFullInfo(
                        song,
                        artistName,
                        artistAvatar,
                        genreIds,
                        genreNames
                    )
                );
            }

            var pagedResponse = new PagedResponse<SongDto>(
                songDtos,
                page,
                pageSize,
                totalItems
            );

            return Ok(
                ApiResponse<PagedResponse<SongDto>>.Ok(
                    $"Tìm thấy {totalItems} bài hát!",
                    pagedResponse
                )
            );
        }

        // ✅ Tạo bài hát mới
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SongDto>>> CreateSong(
            [FromForm] SongCreateDto songCreateDto,
            IFormFile? audioFile,
            IFormFile? coverImage,
            [FromServices] IFileService fileService)
        {
            // Upload audio file nếu có
            if (audioFile != null)
            {
                var audioPath = await fileService.UploadImageAsync(audioFile, "songs");
                songCreateDto.FileUrl = audioPath;
            }

            // Upload cover image nếu có
            if (coverImage != null)
            {
                var coverPath = await fileService.UploadImageAsync(coverImage, "songCovers");
                songCreateDto.CoverUrl = coverPath;
            }

            var song = SongMapper.ToEntityCreate(songCreateDto);
            _context.Songs.Add(song);
            await _context.SaveChangesAsync(); // Lưu để có SongId

            // Xử lý thể loại bài hát - CHỈ LẤY THỂ LOẠI ĐẦU TIÊN
            if (songCreateDto.GenreIds != null && songCreateDto.GenreIds.Any())
            {
                var genreId = songCreateDto.GenreIds.First(); // Chỉ lấy thể loại đầu tiên

                var songGenre = new SongGenre
                {
                    SongId = song.Id,
                    GenreId = genreId
                };

                _context.SongGenres.Add(songGenre);
                await _context.SaveChangesAsync();

                // Trả về danh sách chỉ có 1 GenreId
                var genreIds = new List<int> { genreId };
                return Ok(ApiResponse<SongDto>.Ok("Tạo bài hát thành công!", SongMapper.ToDtoWithGenres(song, genreIds)));
            }

            return Ok(ApiResponse<SongDto>.Ok("Tạo bài hát thành công!", SongMapper.ToDto(song)));
        }

        // ✅ Cập nhật bài hát
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SongDto>>> UpdateSong(
            int id,
            [FromForm] SongDto songDto,
            IFormFile? audioFile,
            IFormFile? coverImage,
            [FromServices] IFileService fileService)
        {
            var existingSong = await _context.Songs.FindAsync(id);
            if (existingSong == null)
                return NotFound(ApiResponse<SongDto>.Fail("Không tìm thấy bài hát để cập nhật!"));

            // Upload audio file mới nếu có
            if (audioFile != null)
            {
                var audioPath = await fileService.UploadImageAsync(audioFile, "songs");
                songDto.FileUrl = audioPath;
            }

            // Upload cover image mới nếu có
            if (coverImage != null)
            {
                var coverPath = await fileService.UploadImageAsync(coverImage, "songCovers");
                songDto.CoverUrl = coverPath;
            }

            SongMapper.ToEntityUpdate(existingSong, songDto);

            // Xử lý cập nhật thể loại bài hát - CHỈ LẤY THỂ LOẠI ĐẦU TIÊN
            if (songDto.GenreIds != null && songDto.GenreIds.Any())
            {
                // Xóa tất cả thể loại cũ của bài hát
                var existingGenres = await _context.SongGenres
                    .Where(sg => sg.SongId == id)
                    .ToListAsync();

                _context.SongGenres.RemoveRange(existingGenres);

                // Thêm thể loại mới - CHỈ THÊM THỂ LOẠI ĐẦU TIÊN
                var genreId = songDto.GenreIds.First(); // Chỉ lấy thể loại đầu tiên

                var newSongGenre = new SongGenre
                {
                    SongId = id,
                    GenreId = genreId
                };

                _context.SongGenres.Add(newSongGenre);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse<SongDto>.Fail($"Lỗi khi cập nhật dữ liệu: {ex.InnerException?.Message ?? ex.Message}"));
            }

            // Lấy GenreId hiện tại để trả về
            var currentGenreIds = await _context.SongGenres
                .Where(sg => sg.SongId == id)
                .Select(sg => sg.GenreId)
                .ToListAsync();

            if (currentGenreIds.Any())
            {
                return Ok(ApiResponse<SongDto>.Ok("Cập nhật bài hát thành công!", SongMapper.ToDtoWithGenres(existingSong, currentGenreIds)));
            }

            return Ok(ApiResponse<SongDto>.Ok("Cập nhật bài hát thành công!", SongMapper.ToDto(existingSong)));
        }

        // ✅ Xóa bài hát
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);

            if (song == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy bài hát để xóa!"));

            // 🔹 Xóa tất cả record trong PlaylistSongs trước
            var playlistSongs = await _context.PlaylistSongs
                .Where(ps => ps.SongId == id)
                .ToListAsync();

            if (playlistSongs.Any())
            {
                _context.PlaylistSongs.RemoveRange(playlistSongs);
            }

            // 🔹 Sau đó xóa bài hát
            _context.Songs.Remove(song);

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok("Xóa bài hát thành công!"));
        }

        // ✅ Lấy bài hát phổ biến (theo views)
        [HttpGet("popular")]
        public async Task<ActionResult<ApiResponse<PagedResponse<SongDto>>>> GetPopularSongs(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            if (page < 1)
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("Số trang phải lớn hơn 0!"));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("Số phần tử mỗi trang phải từ 1 đến 100!"));

            // Tổng số bài hát
            var totalItems = await _context.Songs.CountAsync();

            if (totalItems == 0)
            {
                var empty = new PagedResponse<SongDto>(new List<SongDto>(), page, pageSize, 0);
                return Ok(ApiResponse<PagedResponse<SongDto>>.Ok("Không có bài hát nào!", empty));
            }

            // ⚡ Query tối ưu: 1 lần duy nhất
            var songs = await _context.Songs
                .OrderByDescending(s => s.Views)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                        .FirstOrDefault(),

                    Genres = _context.SongGenres
                        .Where(sg => sg.SongId == s.Id)
                        .Join(
                            _context.Genres,
                            sg => sg.GenreId,
                            g => g.Id,
                            (sg, g) => new { g.Id, g.Name }
                        )
                        .ToList()
                })
                .ToListAsync();

            // Map sang DTO
            var songDtos = songs.Select(x =>
                SongMapper.ToDtoWithFullInfo(
                    x.Song,
                    x.ArtistName,
                    x.ArtistAvatar,
                    x.Genres.Select(g => g.Id).ToList(),
                    x.Genres.Select(g => g.Name).ToList()
                )
            ).ToList();

            var pagedResponse = new PagedResponse<SongDto>(songDtos, page, pageSize, totalItems);

            return Ok(ApiResponse<PagedResponse<SongDto>>.Ok("Lấy danh sách bài hát phổ biến thành công!", pagedResponse));
        }

        [HttpPost]
        public async Task<IActionResult> IncrementView([FromQuery] int songId)
        {
            if (songId <= 0)
                return BadRequest("SongId không hợp lệ");

            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
        UPDATE Songs
        SET Views = Views + 1
        WHERE Id = {songId}
    ");

            if (rows == 0)
                return NotFound("Không tìm thấy bài hát");

            return Ok(new
            {
                status = 200,
                message = "Tăng view bài hát thành công"
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResponse<SongDto>>>> GetFavoriteSongs(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            if (userId <= 0)
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("UserId không hợp lệ!"));

            if (page < 1)
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("Số trang phải lớn hơn 0!"));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<PagedResponse<SongDto>>.Fail("Số phần tử mỗi trang phải từ 1 đến 100!"));

            // Query join Likes + Songs
            var query = _context.Likes
                .Where(l => l.UserId == userId && l.TargetType == "Song")
                .Join(_context.Songs,
                    like => like.TargetId,
                    song => song.Id,
                    (like, song) => new { like, song })
                .OrderByDescending(x => x.like.CreatedAt);

            var totalItems = await query.CountAsync();

            if (totalItems == 0)
            {
                var empty = new PagedResponse<SongDto>(new List<SongDto>(), page, pageSize, 0);
                return Ok(ApiResponse<PagedResponse<SongDto>>.Ok("Chưa có bài hát yêu thích!", empty));
            }

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Song = x.song,
                    ArtistName = _context.Users
                        .Where(u => u.Id == x.song.ArtistId)
                        .Select(u => u.Username)
                        .FirstOrDefault(),

                    ArtistAvatar = _context.Users
                        .Where(u => u.Id == x.song.ArtistId)
                        .Select(u => u.AvatarUrl)
                        .FirstOrDefault(),

                    Genres = _context.SongGenres
                        .Where(sg => sg.SongId == x.song.Id)
                        .Join(_context.Genres,
                            sg => sg.GenreId,
                            g => g.Id,
                            (sg, g) => new { g.Id, g.Name })
                        .ToList()
                })
                .ToListAsync();

            var songDtos = data.Select(x =>
                SongMapper.ToDtoWithFullInfo(
                    x.Song,
                    x.ArtistName,
                    x.ArtistAvatar,
                    x.Genres.Select(g => g.Id).ToList(),
                    x.Genres.Select(g => g.Name).ToList()
                )
            ).ToList();

            var pagedResponse = new PagedResponse<SongDto>(songDtos, page, pageSize, totalItems);

            return Ok(ApiResponse<PagedResponse<SongDto>>.Ok(
                "Lấy danh sách bài hát yêu thích thành công!",
                pagedResponse));
        }

    }
}