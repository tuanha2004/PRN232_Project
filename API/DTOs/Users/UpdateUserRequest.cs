using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Users
{
    public class UpdateUserRequest
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2-100 ký tự")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string? Address { get; set; }

        [RegularExpression("^(Provider|Student)$",
            ErrorMessage = "Vai trò phải là: Provider hoặc Student")]
        public string? Role { get; set; }

        [RegularExpression("^(Active|Inactive)$",
            ErrorMessage = "Trạng thái phải là: Active hoặc Inactive")]
        public string? Status { get; set; }
    }
}
