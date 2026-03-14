using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MUMbackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MUMbackend.Services.Auth
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        private readonly RefreshTokenService _refreshTokenService;

        public TokenService(IConfiguration config, RefreshTokenService refreshTokenService)
        {
            _config = config;
            _refreshTokenService = refreshTokenService;
        }

        public string GenerateAccessToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ phải là Id
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokenPairAsync(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user);
            return (accessToken, refreshToken.Token);
        }

        
    }
}
