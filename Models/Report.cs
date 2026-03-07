using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [Required]
        [Column("ReporterId", TypeName = "bigint")]
        public int ReporterId { get; set; }

        [Required]
        [Column("TargetId", TypeName = "bigint")]
        public int TargetId { get; set; }

        [Required]
        [MaxLength(20)]
        public string TargetType { get; set; } = string.Empty; // song | user | playlist

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending | Resolved | Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}