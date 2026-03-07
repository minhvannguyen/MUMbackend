using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    public class Like
    {
        [Key]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [Required]
        [Column("UserId", TypeName = "bigint")]
        public int UserId { get; set; }

        [Required]
        public string TargetType { get; set; } = string.Empty;
        [Required]
        [Column("TargetId", TypeName = "bigint")]
        public int TargetId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
