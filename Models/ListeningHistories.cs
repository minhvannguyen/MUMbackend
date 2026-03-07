using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    public class ListeningHistories
    {
        public int Id { get; set; }
        [Column("UserId", TypeName = "bigint")]
        public int UserId { get; set; }
        [Column("SongId", TypeName = "bigint")]
        public int SongId { get; set; }
        public DateTime PlayedAt { get; set; }

        public User User { get; set; }
        public Song Song { get; set; }
    }
}
