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
        [Authorize] // Yêu cầu đăng nhập
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(ApiResponse<UserDto>.Fail("Không tìm thấy user!"));

            return Ok(ApiResponse<UserDto>.Ok("Lấy thông tin user thành công!", UserMapper.ToDto(user)));
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
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
            long id,
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

            // 3️⃣ Nếu có ảnh mới → upload vào wwwroot/uploads/profileImages
            if (avatar != null)
            {
                var imagePath = await fileService.UploadImageAsync(avatar, "profileImages");
                existingUser.AvatarUrl = imagePath;
            }

            // 4️⃣ Cập nhật các thông tin khác
            UserMapper.ToEntityUpdate(existingUser, userDto);

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
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy user để xóa!"));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok("Xóa user thành công!"));
        }
    }
}
