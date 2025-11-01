namespace MUMbackend.Dtos
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Role { get; set; }
    }
}
