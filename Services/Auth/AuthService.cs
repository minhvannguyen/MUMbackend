using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Dtos;
using MUMbackend.Dtos.Auth;
using MUMbackend.Mappers;
using MUMbackend.Models;

namespace MUMbackend.Services.Auth
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly TokenService _tokenService;
        private readonly VerificationService _verificationService;

        public AuthService(AppDbContext context, PasswordService passwordService, TokenService tokenService, VerificationService verificationService)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _verificationService = verificationService;
        }

        public async Task<ApiResponse<object>> RegisterAsync(String email)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return new ApiResponse<object>(false, "Email đã tồn tại!");

            await _verificationService.SendVerificationCodeAsync(email);

            return new ApiResponse<object>(true, "Mã xác nhận đã được gửi tới email của bạn!");
        }

        public async Task<ApiResponse<object>> ForgotPassAsync(String email)
        {
            if (!await _context.Users.AnyAsync(u => u.Email == email))
                return new ApiResponse<object>(false, "Email không tồn tại!");

            await _verificationService.SendVerificationCodeAsync(email);

            return new ApiResponse<object>(true, "Mã xác nhận đã được gửi tới email của bạn!");
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !_passwordService.Verify(user, request.Password))
                return null;

            // ✅ tạo cặp token
            var (accessToken, refreshToken) = await _tokenService.GenerateTokenPairAsync(user);
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<ApiResponse<object>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return new ApiResponse<object>(false, "Không tìm thấy người dùng!");

            if (!_passwordService.Verify(user, request.OldPassword))
                return new ApiResponse<object>(false, "Mật khẩu cũ không đúng!");

            user.Password = _passwordService.Hash(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResponse<object>(true, "Đổi mật khẩu thành công!");
        }

        public async Task<ApiResponse<object>> ChangePasswordForgotAsync(string email, string newPass)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return new ApiResponse<object>(false, "Không tìm thấy người dùng!");

            user.Password = _passwordService.Hash(newPass);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResponse<object>(true, "Đổi mật khẩu thành công!");
        }

        

// ...

public async Task<User?> FindUserByExternalAsync(string provider, string providerKey)
{
    var link = await _context.UserExternalLogins
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey);

    return link?.User;
}

public async Task<User> LinkOrCreateExternalUserAsync(string provider, string providerKey, string? email, string? name, string? picture)
{
    var existing = await _context.UserExternalLogins
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey);

    if (existing != null) return existing.User;

    User? user = null;
    if (!string.IsNullOrWhiteSpace(email))
    {
        user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    if (user == null)
    {
        user = new User
        {
            Email = email ?? $"{providerKey}@placeholder.local",
            Username = !string.IsNullOrWhiteSpace(name) ? name : (email?.Split('@').FirstOrDefault() ?? "user"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Password = "EXTERNAL_LOGIN"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    _context.UserExternalLogins.Add(new UserExternalLogin
    {
        UserId = user.Id,
        Provider = provider,
        ProviderKey = providerKey,
        Email = email,
        Name = name,
        Picture = picture,
        CreatedAt = DateTime.UtcNow
    });
    await _context.SaveChangesAsync();

    return user;
}

        public async Task<UserDto?> MeDtoAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                if (user == null) return null;

            var userDto = UserMapper.ToDto(user);

            return userDto;
        }
    }
}
