using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Applications
{
    public class CreateApplicationRequest
    {
        [Required(ErrorMessage = "JobId là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "JobId không hợp lệ")]
        public int JobId { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? Phone { get; set; }

        [StringLength(20, ErrorMessage = "Năm học không được quá 20 ký tự")]
        [RegularExpression("^(Year 1|Year 2|Year 3|Year 4)$",
            ErrorMessage = "Năm học phải là: Year 1, Year 2, Year 3, Year 4")]
        public string? StudentYear { get; set; }

        [StringLength(20, ErrorMessage = "Loại công việc không được quá 20 ký tự")]
        [RegularExpression("^(Full-time|Part-time|Internship)$",
            ErrorMessage = "Loại công việc phải là: Full-time, Part-time, hoặc Internship")]
        public string? WorkType { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}
