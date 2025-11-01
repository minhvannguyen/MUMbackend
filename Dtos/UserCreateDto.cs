using System.ComponentModel.DataAnnotations;

namespace MUMbackend.Dtos
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Tên người dùng không được để trống!")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        public string Password { get; set; }

        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
    }
}
