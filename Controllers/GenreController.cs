using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Models;
using MUMbackend.Dtos;
using MUMbackend.Mappers;
using Microsoft.AspNetCore.Authorization;

namespace MUMbackend.Controllers
{
    [Route("api/genres/[action]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GenreController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy tất cả thể loại
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GenreDto>>>> GetGenres()
        {
            var genres = await _context.Genres.ToListAsync();

            if (!genres.Any())
                return Ok(ApiResponse<IEnumerable<GenreDto>>.Ok("Không có thể loại nào!", new List<GenreDto>()));

            var genreDtos = genres.Select(GenreMapper.ToDto).ToList();
            return Ok(ApiResponse<IEnumerable<GenreDto>>.Ok("Lấy danh sách thể loại thành công!", genreDtos));
        }

        // ✅ Lấy tất cả thể loại
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<GenreDto>>>> GetAllGenres([FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var query = _context.Genres.AsNoTracking();

            var totalItems = await query.CountAsync();

            var genres = await query
                .OrderByDescending(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var genreDtos = genres.Select(GenreMapper.ToDto).ToList();

            var pageGenres = new PagedResponse<GenreDto>(
                genreDtos,
                page,
                pageSize,
                totalItems
            );
            
            return Ok(ApiResponse<PagedResponse<GenreDto>>.Ok("Lấy danh sách thể loại thành công!", pageGenres));
        }

        // ✅ Lấy thể loại theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GenreDto>>> GetGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound(ApiResponse<GenreDto>.Fail("Không tìm thấy thể loại!"));

            return Ok(ApiResponse<GenreDto>.Ok("Lấy thông tin thể loại thành công!", GenreMapper.ToDto(genre)));
        }

        // ✅ Lấy thể loại theo Slug
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<ApiResponse<GenreDto>>> GetGenreBySlug(string slug)
        {
            var genre = await _context.Genres
                .FirstOrDefaultAsync(g => g.Slug == slug);

            if (genre == null)
                return NotFound(ApiResponse<GenreDto>.Fail("Không tìm thấy thể loại!"));

            return Ok(ApiResponse<GenreDto>.Ok("Lấy thông tin thể loại thành công!", GenreMapper.ToDto(genre)));
        }

        // ✅ Tìm kiếm thể loại theo tên
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<GenreDto>>>> SearchGenres([FromQuery] string keyword, int page = 1,
            int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(ApiResponse<IEnumerable<GenreDto>>.Fail("Từ khóa tìm kiếm không được để trống!"));
            
            var query = _context.Genres
                .Where(u => EF.Functions.Like(u.Name, $"%{keyword}%"));

            var totalItems = await query.CountAsync();

            var genres = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var genreDtos = genres.Select(GenreMapper.ToDto).ToList();

            var pageGenres = new PagedResponse<GenreDto>(
                genreDtos,
                page,
                pageSize,
                totalItems
            );
            return Ok(ApiResponse<PagedResponse<GenreDto>>.Ok($"Tìm thấy {genres.Count} thể loại!", pageGenres));
        }

        // ✅ Tạo thể loại mới
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<GenreDto>>> CreateGenre([FromBody] GenreDto genreCreateDto)
        {
            // Kiểm tra tên thể loại đã tồn tại chưa
            if (await _context.Genres.AnyAsync(g => g.Name == genreCreateDto.Name))
                return BadRequest(ApiResponse<GenreDto>.Fail("Tên thể loại đã tồn tại!"));

            // Nếu có slug, kiểm tra slug trùng
            if (!string.IsNullOrEmpty(genreCreateDto.Slug))
            {
                if (await _context.Genres.AnyAsync(g => g.Slug == genreCreateDto.Slug))
                    return BadRequest(ApiResponse<GenreDto>.Fail("Slug đã tồn tại!"));
            }

            var genre = GenreMapper.ToEntityCreate(genreCreateDto);

            // Nếu slug tự động tạo, đảm bảo không trùng
            if (string.IsNullOrEmpty(genreCreateDto.Slug))
            {
                string baseSlug = genre.Slug;
                int counter = 1;
                while (await _context.Genres.AnyAsync(g => g.Slug == genre.Slug))
                {
                    genre.Slug = $"{baseSlug}-{counter}";
                    counter++;
                }
            }

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<GenreDto>.Ok("Tạo thể loại thành công!", GenreMapper.ToDto(genre)));
        }

        // ✅ Cập nhật thể loại
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<GenreDto>>> UpdateGenre(int id, [FromBody] GenreDto genreDto)
        {
            var existingGenre = await _context.Genres.FindAsync(id);
            if (existingGenre == null)
                return NotFound(ApiResponse<GenreDto>.Fail("Không tìm thấy thể loại để cập nhật!"));

            // Kiểm tra tên trùng (trừ chính nó)
            if (!string.Equals(existingGenre.Name, genreDto.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Genres.AnyAsync(g => g.Name == genreDto.Name && g.Id != id))
                    return BadRequest(ApiResponse<GenreDto>.Fail("Tên thể loại đã tồn tại!"));
            }

            // Kiểm tra slug trùng (trừ chính nó)
            if (!string.IsNullOrEmpty(genreDto.Slug))
            {
                if (!string.Equals(existingGenre.Slug, genreDto.Slug, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _context.Genres.AnyAsync(g => g.Slug == genreDto.Slug && g.Id != id))
                        return BadRequest(ApiResponse<GenreDto>.Fail("Slug đã tồn tại!"));
                }
            }

            GenreMapper.ToEntityUpdate(existingGenre, genreDto);

            // Đảm bảo slug không trùng sau khi update
            if (string.IsNullOrEmpty(genreDto.Slug) && genreDto.Name != null)
            {
                string baseSlug = existingGenre.Slug;
                int counter = 1;
                string originalSlug = existingGenre.Slug;
                while (await _context.Genres.AnyAsync(g => g.Slug == existingGenre.Slug && g.Id != id))
                {
                    existingGenre.Slug = $"{baseSlug}-{counter}";
                    counter++;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse<GenreDto>.Fail($"Lỗi khi cập nhật dữ liệu: {ex.InnerException?.Message ?? ex.Message}"));
            }

            return Ok(ApiResponse<GenreDto>.Ok("Cập nhật thể loại thành công!", GenreMapper.ToDto(existingGenre)));
        }

        // ✅ Xóa thể loại
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy thể loại để xóa!"));

            // Kiểm tra xem có bài hát nào đang sử dụng thể loại này không
            var songCount = await _context.SongGenres
                .Where(sg => sg.GenreId == id)
                .CountAsync();

            if (songCount > 0)
                return BadRequest(ApiResponse<object>.Fail($"Không thể xóa thể loại này! Đang có {songCount} bài hát sử dụng thể loại này."));

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok("Xóa thể loại thành công!"));
        }
    }
}