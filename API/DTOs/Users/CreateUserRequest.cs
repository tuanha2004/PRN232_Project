using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Users
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2-100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [RegularExpression("^(Admin|User|Manager)$",
            ErrorMessage = "Vai trò phải là: Admin, User, hoặc Manager")]
        public string Role { get; set; } = "User";

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$",
            ErrorMessage = "Số điện thoại không hợp lệ (VD: 0123456789 hoặc +84123456789)")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5-200 ký tự")]
        public string Address { get; set; } = string.Empty;
    }
}
