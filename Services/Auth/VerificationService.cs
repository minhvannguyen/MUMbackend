using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Data;
using MUMbackend.Models.Auth;
using MUMbackend.Models;

namespace MUMbackend.Services.Auth
{
    public class VerificationService
    {
        private readonly AppDbContext _context;
        private readonly GmailService _emailService;
        private readonly TokenService _tokenService;

        public VerificationService(AppDbContext context, GmailService emailService, TokenService tokenService)
        {
            _context = context;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        public async Task SendVerificationCodeAsync(string email)
        {
            var code = new Random().Next(100000, 999999).ToString(); // mã 6 số

            var entity = new VerificationCodes
            {
                Email = email,
                Code = code,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10)
            };

            _context.VerificationCodes.Add(entity);
            await _context.SaveChangesAsync();

            // 2️⃣ Gửi OTP qua Gmail API
            var gmailService = new GmailService();
            await gmailService.SendOtpEmailAsync(email, code);
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var record = await _context.VerificationCodes
                .Where(v => v.Email == email && !v.IsUsed)
                .OrderByDescending(v => v.ExpiredAt)
                .FirstOrDefaultAsync();

            if (record == null || record.Code != code || record.ExpiredAt < DateTime.UtcNow)
                return false;

            record.IsUsed = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool, List<Claim>)> VerifyCodeGenToken(string email, string code)
        {
            var record = await _context.VerificationCodes
                .Where(v => v.Email == email && !v.IsUsed)
                .OrderByDescending(v => v.ExpiredAt)
                .FirstOrDefaultAsync();

            if (record == null || record.Code != code || record.ExpiredAt < DateTime.UtcNow)
                return (false, null);

            // Tìm user để tạo token
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return (false, null);

            record.IsUsed = true;
            await _context.SaveChangesAsync();

            // Tạo temp token với thời hạn 5 phút
            var accessToken = _tokenService.GenerateAccessToken(user);

            // ✅ Tạo claims cho cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("AccessToken", accessToken),
            };

            


            return (true, claims);
        }
    }
}
