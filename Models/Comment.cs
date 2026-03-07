using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MUMbackend.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }
        [Column("UserId", TypeName = "bigint")]
        public int UserId { get; set; }
        [Column("SongId", TypeName = "bigint")]
        public int SongId { get; set; }
        public string Content { get; set; } = string.Empty;
        [Column("ParentId", TypeName = "bigint")]
        public int? ParentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
