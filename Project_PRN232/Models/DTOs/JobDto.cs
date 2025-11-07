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
        public DateTime? UpdatedAt { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderEmail { get; set; }
        public string? CompanyName { get; set; }

        public List<ApplicationDto>? Applications { get; set; }
        public List<JobAssignmentDto>? JobAssignments { get; set; }
    }
    
    public class JobAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentPhone { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string? Status { get; set; }
    }
}
