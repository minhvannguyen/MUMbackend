namespace MUMbackend.Dtos.Comments
{
    public class CreateCommentDto
    {
        public int UserId { get; set; }
        public int SongId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
