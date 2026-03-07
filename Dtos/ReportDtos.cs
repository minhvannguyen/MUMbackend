namespace MUMbackend.Dtos
{
    public class CreateReportDto
    {
        public int TargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ReportResponseDto
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        public int TargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateReportStatusDto
    {
        public string Status { get; set; } = string.Empty; // Resolved / Rejected
    }
}