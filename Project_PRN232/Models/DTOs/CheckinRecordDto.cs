namespace Project_PRN232.Models.DTOs
{
    public class CheckinRecordResponse
    {
        public int CheckinId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public DateTime? CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double? WorkedHours { get; set; }
    }

    public class CheckinRequest
    {
        public int JobId { get; set; }
    }

    public class CheckoutRequest
    {
        public int CheckinId { get; set; }
    }
}
