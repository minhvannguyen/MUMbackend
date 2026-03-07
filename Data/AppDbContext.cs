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
        public DbSet<Song> Songs { get; set; }
        public DbSet<SongGenre> SongGenres { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSongs> PlaylistSongs { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<ListeningHistories> ListeningHistories { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<SavedPlaylist> SavedPlaylists { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình composite key cho SongGenre
            modelBuilder.Entity<SongGenre>()
                .HasKey(sg => new { sg.SongId, sg.GenreId });

            modelBuilder.Entity<PlaylistSongs>()
                .HasKey(ps => new { ps.PlaylistId, ps.SongId });

            modelBuilder.Entity<PlaylistSongs>()
                .HasOne(ps => ps.Playlist)
                .WithMany(p => p.PlaylistSongs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlaylistSongs>()
                .HasOne(ps => ps.Song)
                .WithMany(s => s.PlaylistSongs)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.TargetType, l.TargetId })
                .IsUnique();
            modelBuilder.Entity<Follow>(entity =>
            {
                // ✅ Composite Primary Key
                entity.HasKey(f => new { f.FollowerId, f.FollowingId });

                // Follower
                entity.HasOne(f => f.Follower)
                      .WithMany(u => u.Following)
                      .HasForeignKey(f => f.FollowerId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Following
                entity.HasOne(f => f.Following)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(f => f.FollowingId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<SavedPlaylist>()
                .HasKey(sp => new { sp.UserId, sp.PlaylistId });

            modelBuilder.Entity<SavedPlaylist>()
                .HasOne(sp => sp.User)
                .WithMany(u => u.SavedPlaylists)
                .HasForeignKey(sp => sp.UserId);

            modelBuilder.Entity<SavedPlaylist>()
                .HasOne(sp => sp.Playlist)
                .WithMany(p => p.SavedByUsers)
                .HasForeignKey(sp => sp.PlaylistId);
        }

    }
}
