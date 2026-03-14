using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MUMbackend.Models.Auth
{
    [Table("VerificationCodes")]
    public class VerificationCodes
    {
        [Key]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }

        public DateTime ExpiredAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
