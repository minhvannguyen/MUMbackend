using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    [Table("SongGenres")]
    public class SongGenre
    {
        [Key]
        [Column("SongId", TypeName = "bigint")]
        public int SongId { get; set; }

        [Key]
        public int GenreId { get; set; }

        // Navigation properties
        public virtual Song? Song { get; set; }
        // public virtual Genre? Genre { get; set; }
    }
}