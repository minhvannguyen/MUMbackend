using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MUMbackend.Models
{
    public class Notification
    {
        [Key]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }
        [Column("UserId", TypeName = "bigint")]
        public int UserId { get; set; }
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
