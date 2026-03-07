using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    public class Playlist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [Column("UserId", TypeName = "bigint")]
        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? CoverUrl { get; set; }
        [Column("TotalViews", TypeName = "bigint")]
        public int TotalViews { get; set; }
        public int? SaveCount { get; set; }

        public bool IsPublic { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Navigation
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Many-to-many
        public ICollection<PlaylistSongs> PlaylistSongs { get; set; } = new List<PlaylistSongs>();
        public ICollection<SavedPlaylist> SavedByUsers { get; set; }
    }
}
