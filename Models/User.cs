using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
       [Column("Id", TypeName = "bigint")]
        public int Id { get; set; }

        [Required]
        [Column("Username", TypeName = "nvarchar(100)")]
        public string Username { get; set; }

        [Required]
        [Column("Email", TypeName = "nvarchar(255)")]
        public string Email { get; set; }

        [Required]
        [Column("Password", TypeName = "nvarchar(255)")]
        public string Password { get; set; }

        [Column("AvatarUrl", TypeName = "nvarchar(255)")]
        public string? AvatarUrl { get; set; }

        [Column("Bio", TypeName = "nvarchar(max)")]
        public string? Bio { get; set; }

        [Column("Role", TypeName = "nvarchar(20)")]
        public string Role { get; set; } = "USER";

        [Column("CreatedAt", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column("UpdatedAt", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
        [Column("IsActive", TypeName = "bit")]
        public bool? IsActive { get; set; } = true;

        public ICollection<Follow> Followers { get; set; }
        public ICollection<Follow> Following { get; set; }
        public ICollection<SavedPlaylist> SavedPlaylists { get; set; }
    }
}
