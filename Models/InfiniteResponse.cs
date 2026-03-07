namespace MUMbackend.Models
{
    public class InfiniteResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public double? NextScore { get; set; }
        public int? NextSongId { get; set; }
        public bool HasMore { get; set; }
    }
}
