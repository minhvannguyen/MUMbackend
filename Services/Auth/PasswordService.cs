using Microsoft.AspNetCore.Identity;
using MUMbackend.Models;

namespace MUMbackend.Services.Auth
{
    public class PasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        public bool Verify(User user, string password)
        {
            var result = _hasher.VerifyHashedPassword(user, user.Password, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
