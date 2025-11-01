using MUMbackend.Models;
using Microsoft.EntityFrameworkCore;
using MUMbackend.Models.Auth;


namespace MUMbackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }  
        public DbSet<VerificationCodes> VerificationCodes { get; set; }
        public DbSet<UserExternalLogin> UserExternalLogins { get; set; }

    }
}
