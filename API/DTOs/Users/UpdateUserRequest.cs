using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Users
{
    public class UpdateUserRequest
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2-100 ký tự")]
        public string? FullName { get; set; }

        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$",
            ErrorMessage = "Số điện thoại không hợp lệ (VD: 0123456789 hoặc +84123456789)")]
        public string? Phone { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5-200 ký tự")]
        public string? Address { get; set; }

        [RegularExpression("^(Admin|User|Manager)$",
            ErrorMessage = "Vai trò phải là: Admin, User, hoặc Manager")]
        public string? Role { get; set; }

        [RegularExpression("^(Active|Inactive|Suspended)$",
            ErrorMessage = "Trạng thái phải là: Active, Inactive, hoặc Suspended")]
        public string? Status { get; set; }
    }
}
