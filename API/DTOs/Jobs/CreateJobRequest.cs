using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Jobs
{
    public class CreateJobRequest
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5-200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mô tả phải từ 10-2000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa điểm không được quá 200 ký tự")]
        public string Location { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "Lương phải từ 0 đến 999,999,999")]
        public decimal? Salary { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProviderId không hợp lệ")]
        public int? ProviderId { get; set; }
    }
}
