using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    [Table("SavedPlaylists")]
    public class SavedPlaylist
    {
        [Column("UserId", TypeName = "bigint")]
        public int UserId { get; set; }
        [Column("PlaylistId", TypeName = "bigint")]
        public int PlaylistId { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Playlist Playlist { get; set; }
    }
}