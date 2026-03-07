namespace MUMbackend.Dtos
{
    public class UserDto
    {
        public int? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Role { get; set; }

        public int? TotalSongs { get; set; }
        public int? TotalPlaylists { get; set; }
        public int? TotalFollowing { get; set; }
        public int? TotalFollowers { get; set; }
    }
}
