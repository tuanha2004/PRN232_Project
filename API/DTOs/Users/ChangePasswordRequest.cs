using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Users
{
    public class ChangePasswordRequest
    {
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu cũ phải từ 6-100 ký tự")]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6-100 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
