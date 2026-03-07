using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    [Table("Songs")]
    public class Song
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [Required]
        [Column("Title", TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Required]
        [Column("ArtistId", TypeName = "bigint")]
        public int ArtistId { get; set; }

        [Column("FileUrl", TypeName = "nvarchar(500)")]
        public string? FileUrl { get; set; }

        [Column("CoverUrl", TypeName = "nvarchar(500)")]
        public string? CoverUrl { get; set; }

        [Column("Duration", TypeName = "int")]
        public int? Duration { get; set; } // Duration in seconds

        [Column("UploadedAt", TypeName = "datetime")]
        public DateTime? UploadedAt { get; set; }

        [Column("Views", TypeName = "bigint")]
        public int Views { get; set; } = 0;

        [Column("Private", TypeName = "bit")]
        public bool Private { get; set; } = false;

        // Many-to-many
        public virtual ICollection<PlaylistSongs> PlaylistSongs { get; set; } = new List<PlaylistSongs>();
        public virtual ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();


        // Navigation property (nếu có Artist model)
        // public virtual User? Artist { get; set; }
    }
}