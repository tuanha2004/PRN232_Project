using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Applications
{
    public class UpdateApplicationStatusRequest
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [RegularExpression(@"^(Pending|Approved|Rejected)$", 
            ErrorMessage = "Trạng thái phải là: Pending, Approved, hoặc Rejected")]
        public string Status { get; set; } = string.Empty;
    }
}
