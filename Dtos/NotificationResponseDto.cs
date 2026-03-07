namespace MUMbackend.Dtos
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }

        public string Message { get; set; } = "";

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ActorUserId { get; set; }

        public string ActorUsername { get; set; } = "";

        public string? ActorAvatar { get; set; }
    }
}
