namespace MUMbackend.Dtos
{
    public class LikeDto
    {
        public int UserId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
    }
}
