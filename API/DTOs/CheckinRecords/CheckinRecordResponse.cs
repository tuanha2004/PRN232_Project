namespace API.DTOs.CheckinRecords
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
        public string Status { get; set; } = string.Empty; // "Checked In", "Checked Out"
        public double? WorkedHours { get; set; } // Số giờ làm việc
    }
}
