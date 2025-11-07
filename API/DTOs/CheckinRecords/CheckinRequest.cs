using System.ComponentModel.DataAnnotations;

namespace API.DTOs.CheckinRecords
{
    public class CheckinRequest
    {
        [Required(ErrorMessage = "Job ID là bắt buộc")]
        public int JobId { get; set; }
    }
}
