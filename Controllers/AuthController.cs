using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MUMbackend.Dtos.Auth;
using MUMbackend.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Dtos;
using MUMbackend.Mappers;
using MUMbackend.Models;

namespace MUMbackend.Controllers
{
    [ApiController]
    [Route("api/auth/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthController(AuthService authService, TokenService tokenService, RefreshTokenService refreshTokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var auth = await _authService.LoginAsync(req);
            if (auth == null) return Unauthorized(new { message = "Email hoặc mật khẩu sai!" });
            
            // ✅ Tạo claims cho cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, auth.UserId.ToString()), // ✔ ID
                new Claim(ClaimTypes.Email, auth.Email),
                new Claim(ClaimTypes.Name, auth.Username),
                new Claim(ClaimTypes.Role, auth.Role),
                new Claim("AccessToken", auth.AccessToken),
                new Claim("RefreshToken", auth.RefreshToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Cookie persistent
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60) // Thời gian hết hạn
            };

            // ✅ Đăng nhập và tạo cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // ✅ Trả về thông tin user (không trả token)
            return Ok(new
            {
                success = true,
                message = "Đăng nhập thành công!",
                user = new
                {
                    username = auth.Username,
                    email = auth.Email,
                    role = auth.Role
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // ✅ Xóa cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "Đăng xuất thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            // ✅ Lấy refresh token từ claims
            var refreshTokenClaim = User.FindFirst("RefreshToken")?.Value;
            if (string.IsNullOrEmpty(refreshTokenClaim))
                return Unauthorized(new { message = "Không tìm thấy refresh token!" });

            var existingToken = await _refreshTokenService.GetRefreshTokenAsync(refreshTokenClaim);
            if (existingToken == null || existingToken.ExpiryDate < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã hết hạn." });
            }

            // Thu hồi token cũ
            await _refreshTokenService.InvalidateTokenAsync(existingToken);

            // Tạo token mới
            var (newAccess, newRefresh) = await _tokenService.GenerateTokenPairAsync(existingToken.User);

            // ✅ Cập nhật claims với token mới
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, existingToken.User.Email),
                new Claim(ClaimTypes.Name, existingToken.User.Username),
                new Claim(ClaimTypes.Role, existingToken.User.Role),
                new Claim("AccessToken", newAccess),
                new Claim("RefreshToken", newRefresh)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok(new { success = true, message = "Token đã được làm mới!" });
        }

        [HttpPost]
        public async Task<IActionResult> SentOtpRegister([FromBody] string req)
        {
            var result = await _authService.RegisterAsync(req);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SentOtpForgotPass([FromBody] string req)
        {
            var result = await _authService.ForgotPassAsync(req);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var result = await _authService.ChangePasswordAsync(req);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePasswordForgot(string email, string newPass)
        {
            var result = await _authService.ChangePasswordForgotAsync(email, newPass);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost]
public async Task<IActionResult> GoogleIdToken([FromBody] GoogleTokenRequest request)
{
    if (request?.IdToken == null || string.IsNullOrWhiteSpace(request.IdToken))
        return BadRequest(new { message = "Missing id_token" });

    var settings = new GoogleJsonWebSignature.ValidationSettings
    {
        Audience = new[] { HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["Google:ClientId"] }
    };

    GoogleJsonWebSignature.Payload payload;
    try
    {
        payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
    }
    catch
    {
        return Unauthorized(new { message = "Invalid Google id_token" });
    }

    if (payload?.EmailVerified != true)
        return Unauthorized(new { message = "Email is not verified" });

    var sub = payload.Subject;           // GoogleId (stable)
    var email = payload.Email;
    var name = payload.Name;
    var picture = payload.Picture;

    var user = await _authService.FindUserByExternalAsync("Google", sub)
               ?? await _authService.LinkOrCreateExternalUserAsync("Google", sub, email, name, picture);

    var (accessToken, refreshToken) = await _tokenService.GenerateTokenPairAsync(user);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("AccessToken", accessToken),
        new Claim("RefreshToken", refreshToken),
        new Claim("Provider", "Google"),
        new Claim("ProviderKey", sub)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var props = new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
    };
    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity), props);

    return Ok(new
    {
        success = true,
        user = new { username = user.Username, email = user.Email, role = user.Role }
    });
}
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Claim value: " + userIdClaim);

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Chưa đăng nhập!" });

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Token không hợp lệ!" });

            var meDto = await _authService.MeDtoAsync(userId);

            return Ok(new ApiResponse<UserDto>(
                true,
                "Lấy thông tin người dùng thành công!",
                meDto
            ));
        }

    }
}
