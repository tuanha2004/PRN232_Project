using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Jobs
{
    public class UpdateJobRequest
    {
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5-200 ký tự")]
        public string? Title { get; set; }

        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mô tả phải từ 10-2000 ký tự")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Địa điểm không được quá 200 ký tự")]
        public string? Location { get; set; }

        [Range(0, 999999999, ErrorMessage = "Lương phải từ 0 đến 999,999,999")]
        public decimal? Salary { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [RegularExpression(@"^(Open|Closed)$", ErrorMessage = "Status phải là: Open hoặc Closed")]
        public string? Status { get; set; }
    }
}
