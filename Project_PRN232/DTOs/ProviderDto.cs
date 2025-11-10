namespace Project_PRN232.DTOs
{
    public class CreateJobDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal? Salary { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class UpdateJobDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public decimal? Salary { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }
    }

    public class ProviderStatisticsDto
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int OpenJobs { get; set; }
        public int ClosedJobs { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int TotalAssignedStudents { get; set; }
    }
}
