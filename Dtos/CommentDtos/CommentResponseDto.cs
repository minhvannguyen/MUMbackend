namespace MUMbackend.Dtos.Comments
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarUser { get; set; }
        public int SongId { get; set; }
        public string Content { get; set; }
        public int? ParentId { get; set; }
        public string ParentUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public List<CommentResponseDto> Children { get; set; } = new();
    }
}
