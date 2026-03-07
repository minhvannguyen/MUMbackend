using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    [Table("Genres")]
    public class Genre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "int")]
        public int Id { get; set; }

        [Required]
        [Column("Name", TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        [Required]
        [Column("Slug", TypeName = "nvarchar(255)")]
        public string Slug { get; set; }
    }
}