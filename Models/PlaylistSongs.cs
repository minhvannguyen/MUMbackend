using System.ComponentModel.DataAnnotations.Schema;
using MUMbackend.Models;

public class PlaylistSongs
{
    [Column("PlaylistId", TypeName = "bigint")]
    public int PlaylistId { get; set; }
    [Column("SongId", TypeName = "bigint")]
    public int SongId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Playlist Playlist { get; set; }
    public Song Song { get; set; }
}
