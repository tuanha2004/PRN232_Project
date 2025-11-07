namespace Project_PRN232.Models.DTOs
{
    public class JobDto
    {
        public int JobId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public decimal? Salary { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderEmail { get; set; }
    }
}
