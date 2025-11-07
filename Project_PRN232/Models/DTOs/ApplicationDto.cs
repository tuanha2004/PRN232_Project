namespace Project_PRN232.Models.DTOs
{
    public class ApplicationDto
    {
        public int ApplicationId { get; set; }
        public int? StudentId { get; set; }
        public int? JobId { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? Status { get; set; }
        public string? Phone { get; set; }
        public string? StudentYear { get; set; }
        public string? WorkType { get; set; }
        public string? Notes { get; set; }

        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }

        public JobDto? Job { get; set; }
    }
}
