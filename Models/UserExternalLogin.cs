using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    [Table("UserExternalLogins")]
    public class UserExternalLogin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [ForeignKey("User")]
        [Column("UserId", TypeName = "bigint")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = "Google"; // "Google"

        [Required]
        [MaxLength(128)]
        public string ProviderKey { get; set; } = default!; // Google 'sub'

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Picture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = default!;
    }
}