using System.ComponentModel.DataAnnotations;

namespace Project_PRN232.DTOs
{
    public class ChangePasswordRequest
    {
        public string? OldPassword { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
