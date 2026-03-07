using System.ComponentModel.DataAnnotations;

namespace MUMbackend.Dtos
{
    public class SongCreateDto
    {
        [Required(ErrorMessage = "Tiêu đề bài hát không được để trống!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ArtistId không được để trống!")]
        public int ArtistId { get; set; }

        public string? FileUrl { get; set; }
        public string? CoverUrl { get; set; }
        public int? Duration { get; set; }
        public bool Private { get; set; }
        public List<int>? GenreIds { get; set; } // Thêm danh sách thể loại

    }
}
