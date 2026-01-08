using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã xác nhận là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác nhận phải có 6 chữ số")]
        public string ResetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
