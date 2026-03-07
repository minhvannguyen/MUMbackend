namespace MUMbackend.Dtos
{
    public class SongDto
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public int? ArtistId { get; set; }
        public string? ArtistAvatar {  get; set; }
        public string? ArtistName { get; set; } // ✅ Thêm tên nghệ sĩ
        public string? FileUrl { get; set; }
        public string? CoverUrl { get; set; }
        public int? Duration { get; set; }
        public DateTime? UploadedAt { get; set; }
        public int? Views { get; set; }
        public bool? Private { get; set; }
        public List<int>? GenreIds { get; set; }
        public List<string>? GenreNames { get; set; } // ✅ Thêm danh sách tên thể loại
    }
}
