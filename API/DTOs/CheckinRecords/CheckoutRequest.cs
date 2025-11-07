using System.ComponentModel.DataAnnotations;

namespace API.DTOs.CheckinRecords
{
    public class CheckoutRequest
    {
        [Required(ErrorMessage = "Checkin ID là bắt buộc")]
        public int CheckinId { get; set; }
    }
}
