using System.ComponentModel.DataAnnotations;

namespace Project_PRN232.DTOs
{
    public class CreateApplicationRequest
    {
        [Required(ErrorMessage = "JobId là bắt buộc")]
        public int JobId { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm học là bắt buộc")]
        public string StudentYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại công việc là bắt buộc")]
        public string WorkType { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}
