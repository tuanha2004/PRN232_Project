namespace Project_PRN232.DTOs
{
    public class AttendanceRecordDto
    {
        public int CheckinId { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public int? JobId { get; set; }
        public string? JobTitle { get; set; }
        public DateTime? CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public double? WorkDuration { get; set; }
        public string? Status { get; set; }
    }

    public class AttendanceDetailDto
    {
        public int CheckinId { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentPhone { get; set; }
        public int? JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobLocation { get; set; }
        public DateTime? CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public double? WorkDuration { get; set; }
        public string? Status { get; set; }
    }

    public class AttendanceStatisticsDto
    {
        public int TotalCheckins { get; set; }
        public int CompletedCheckins { get; set; }
        public int InProgressCheckins { get; set; }
        public double TotalWorkHours { get; set; }
        public int MonthlyCheckins { get; set; }
        public double AverageWorkHoursPerCheckin { get; set; }
        public List<TopStudentDto>? TopStudents { get; set; }
    }

    public class TopStudentDto
    {
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public int TotalCheckins { get; set; }
        public int CompletedCheckins { get; set; }
        public double TotalWorkHours { get; set; }
    }

    public class DailySummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalCheckins { get; set; }
        public int CompletedCheckins { get; set; }
        public int UniqueStudents { get; set; }
        public double TotalWorkHours { get; set; }
    }
}
