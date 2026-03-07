using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models.Auth
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsRevoked { get; set; } = false;

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
