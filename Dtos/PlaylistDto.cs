namespace MUMbackend.Dtos
{
    public class PlaylistCreateDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public bool IsPublic { get; set; }
    }

    public class PlaylistUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class PlaylistResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Creator { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public int TotalViews {  get; set; }
        public int? SongCount { get; set; }
        public int? SaveCount { get; set; }
        public bool IsPublic { get; set; }
        public bool IsSaved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
