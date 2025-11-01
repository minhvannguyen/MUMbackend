using MUMbackend.Data;
using MUMbackend.Models;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Models.Auth;

namespace MUMbackend.Services.Auth
{
    public class RefreshTokenService
    {
        private readonly AppDbContext _context;

        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(User user)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7) // 7 ngày
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked);
        }

        public async Task InvalidateTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }
}
