using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Models;
using MUMbackend.Dtos;
using MUMbackend.Mappers;
using MUMbackend.Services;
using MUMbackend.Services.Auth;
using Microsoft.AspNetCore.Authorization;

namespace MUMbackend.Controllers
{
    [Route("api/users/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        public UserController(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = new PasswordService();
        }

        // ✅ Lấy tất cả user
        [HttpGet]
        [Authorize] // Yêu cầu đăng nhập
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            if (!users.Any())
                return Ok(ApiResponse<IEnumerable<UserDto>>.Ok("Không có user nào!", new List<UserDto>()));

            var userDtos = users.Select(UserMapper.ToDto).ToList();
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok("Lấy thông tin user thành công!", userDtos));
        }

        // ✅ Lấy user theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy user!"));

            var totalSongs = await _context.Songs
                .CountAsync(s => s.ArtistId == id);

            var totalPlaylists = await _context.Playlists
                .CountAsync(p => p.UserId == id);

            var totalFollowing = await _context.Follows
                .CountAsync(f => f.FollowerId == id);

            var totalFollowers = await _context.Follows
                .CountAsync(f => f.FollowingId == id);

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                Email = user.Email,
                Bio = user.Bio,
                TotalSongs = totalSongs,
                TotalPlaylists = totalPlaylists,
                TotalFollowing = totalFollowing,
                TotalFollowers = totalFollowers
            };

            return Ok(ApiResponse<UserDto>.Ok("Lấy thông tin user thành công!", userDto));
        }

        // ✅ Tạo user mới
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userCreateDto.Email))
                return BadRequest(ApiResponse<UserDto>.Fail("Email đã tồn tại!"));

            var user = UserMapper.ToEntityCreate(userCreateDto);
            user.Password = _passwordService.Hash(userCreateDto.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<UserDto>.Ok("Tạo user thành công!", UserMapper.ToDto(user)));
        }

        // ✅ Cập nhật user
        [HttpPut("{id}")]
        [Authorize] // Yêu cầu đăng nhập
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
            int id,
            [FromForm] UserDto userDto,
            IFormFile? avatar,
            [FromServices] IFileService fileService)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy user để cập nhật!"));

            // 2️⃣ Kiểm tra email trùng
            if (!string.Equals(existingUser.Email, userDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email == userDto.Email && u.Id != id);
                if (emailExists)
                    return BadRequest(ApiResponse<UserDto>.Fail("Email đã tồn tại!"));
            }

            // 4️⃣ Cập nhật các thông tin khác
            UserMapper.ToEntityUpdate(existingUser, userDto);

            // 3️⃣ Nếu có ảnh mới → upload vào wwwroot/uploads/profileImages
            if (avatar != null)
            {
                var imagePath = await fileService.UploadImageAsync(avatar, "profileImages");
                existingUser.AvatarUrl = imagePath;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.Fail($"Lỗi khi cập nhật dữ liệu: {ex.InnerException?.Message ?? ex.Message}"));
            }

            return Ok(ApiResponse<UserDto>.Ok("Cập nhật user thành công!", UserMapper.ToDto(existingUser)));
        }


        // ✅ Xóa user
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy user để xóa!"));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok("Xóa user thành công!"));
        }

        // ✅ Tìm kiếm user theo tên
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> SearchUsersByName(
            string keyword,
            int page = 1,
            int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(ApiResponse<PagedResult<UserDto>>.Fail("Keyword không được để trống!"));

            var query = _context.Users
                .Where(u => EF.Functions.Like(u.Username, $"%{keyword}%"));

            var total = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = await query
        .OrderBy(u => u.Username)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => UserMapper.ToDto(u))
        .ToListAsync();

            var result = new PagedResult<UserDto>
            {
                Data = userDtos,
                Total = total,
                Page = page,
                PageSize = pageSize
            };

            return Ok(ApiResponse<PagedResult<UserDto>>
                .Ok("Tìm kiếm user thành công!", result));
        }
    }
}
