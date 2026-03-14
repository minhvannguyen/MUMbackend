using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MUMbackend.Dtos.Auth;
using MUMbackend.Services.Auth;
using MUMbackend.Models;
using MUMbackend.Services;

namespace MUMbackend.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly DashboardService _dashboardService;

        public AdminAuthController(AuthService authService, DashboardService dashboardService)
        {
            _authService = authService;
            _dashboardService = dashboardService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var auth = await _authService.LoginAsync(req);

            if (auth == null || auth.Role != "Admin")
                return Unauthorized(new { message = "Không phải admin!" });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, auth.Email),
                new Claim(ClaimTypes.Name, auth.Username),
                new Claim(ClaimTypes.Role, auth.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var props = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props
            );

            return Ok(new
            {
                success = true,
                user = new
                {
                    auth.Username,
                    auth.Email,
                    auth.Role
                }
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                username = User.Identity?.Name,
                role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _dashboardService.GetDashboardAsync();

            return Ok(ApiResponse<DashboardResponseDto>
                .Ok("Lấy dữ liệu dashboard thành công", data));
        }
    }

}