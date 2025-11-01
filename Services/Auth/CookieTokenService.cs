using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace MUMbackend.Services.Auth
{
    public class CookieTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieTokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
        }

        public string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("RefreshToken")?.Value;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
