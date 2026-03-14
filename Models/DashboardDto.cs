namespace MUMbackend.Models
{
    public class DashboardResponseDto
    {
        public int TotalSongs { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalReports { get; set; }

        public List<RecentSongDto> RecentSongs { get; set; }
        public List<RecentUserDto> RecentUsers { get; set; }
    }

    public class RecentSongDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class RecentUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
