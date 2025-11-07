# 📋 Tài liệu Validation - Data Annotations

## 🎯 Tổng quan

Hệ thống sử dụng **Data Annotations** để validate dữ liệu trước khi xử lý trong services. Tất cả các DTO đều có validation rules để đảm bảo tính toàn vẹn dữ liệu.

---

## 🔍 Các loại Validation đã implement

### 1. **Required Validation** - Bắt buộc
```csharp
[Required(ErrorMessage = "Tiêu đề công việc là bắt buộc")]
public string Title { get; set; }
```

### 2. **StringLength Validation** - Độ dài chuỗi
```csharp
[StringLength(200, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5-200 ký tự")]
public string Title { get; set; }
```

### 3. **Range Validation** - Khoảng giá trị
```csharp
[Range(0, 999999999, ErrorMessage = "Mức lương phải từ 0 đến 999,999,999")]
public decimal? Salary { get; set; }
```

### 4. **RegularExpression Validation** - Regex pattern
```csharp
[RegularExpression("^(Full-time|Part-time|Internship|Contract)$", 
    ErrorMessage = "Loại công việc không hợp lệ")]
public string JobType { get; set; }
```

### 5. **EmailAddress Validation** - Email
```csharp
[EmailAddress(ErrorMessage = "Email không hợp lệ")]
public string Email { get; set; }
```

### 6. **Url Validation** - URL
```csharp
[Url(ErrorMessage = "CV Link phải là URL hợp lệ")]
public string? CvLink { get; set; }
```

### 7. **Compare Validation** - So sánh 2 fields
```csharp
[Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
public string ConfirmPassword { get; set; }
```

---

## 📦 DTOs với Validation

### 🔹 **Jobs DTOs**

#### CreateJobRequest
```csharp
public class CreateJobRequest
{
    [Required(ErrorMessage = "Tiêu đề công việc là bắt buộc")]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; }

    [Required(ErrorMessage = "Mô tả công việc là bắt buộc")]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; }

    [Required(ErrorMessage = "Địa điểm làm việc là bắt buộc")]
    [StringLength(200)]
    public string Location { get; set; }

    [Range(0, 999999999)]
    public decimal? Salary { get; set; }

    [Required]
    [RegularExpression("^(Full-time|Part-time|Internship|Contract)$")]
    public string JobType { get; set; }

    [Required]
    public int? CreatedBy { get; set; }

    public DateTime? Deadline { get; set; }

    [Range(1, 1000)]
    public int? Quantity { get; set; }

    [StringLength(1000)]
    public string? Requirements { get; set; }

    [StringLength(1000)]
    public string? Benefits { get; set; }
}
```

**Validation Rules:**
- ✅ Title: 5-200 ký tự, bắt buộc
- ✅ Description: 10-2000 ký tự, bắt buộc
- ✅ Location: Max 200 ký tự, bắt buộc
- ✅ Salary: 0-999,999,999
- ✅ JobType: Chỉ nhận Full-time, Part-time, Internship, Contract
- ✅ Quantity: 1-1000

#### UpdateJobRequest
Tương tự CreateJobRequest nhưng thêm:
```csharp
[Required]
public int JobId { get; set; }

[RegularExpression("^(Active|Inactive|Closed)$")]
public string? Status { get; set; }
```

---

### 🔹 **Applications DTOs**

#### CreateApplicationRequest
```csharp
public class CreateApplicationRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int JobId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; }

    [Required]
    [RegularExpression(@"^(0|\+84)[0-9]{9,10}$")]
    public string Phone { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [RegularExpression("^(Year 1|Year 2|Year 3|Year 4|Graduate)$")]
    public string StudentYear { get; set; }

    [Required]
    [RegularExpression("^(Full-time|Part-time|Internship)$")]
    public string WorkType { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Url]
    [StringLength(500)]
    public string? CvLink { get; set; }
}
```

**Validation Rules:**
- ✅ FullName: 2-100 ký tự, bắt buộc
- ✅ Phone: Format Việt Nam (0123456789 hoặc +84123456789)
- ✅ Email: Email hợp lệ, max 100 ký tự
- ✅ StudentYear: Year 1, Year 2, Year 3, Year 4, Graduate
- ✅ WorkType: Full-time, Part-time, Internship
- ✅ CvLink: URL hợp lệ

#### UpdateApplicationStatusRequest
```csharp
public class UpdateApplicationStatusRequest
{
    [Required]
    [RegularExpression("^(Pending|Approved|Rejected|Interview|Cancelled)$")]
    public string Status { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}
```

**Validation Rules:**
- ✅ Status: Pending, Approved, Rejected, Interview, Cancelled
- ✅ Note: Max 500 ký tự

---

### 🔹 **Users DTOs**

#### CreateUserRequest
```csharp
public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$")]
    public string Password { get; set; }

    [Required]
    [RegularExpression("^(Admin|User|Manager)$")]
    public string Role { get; set; } = "User";

    [Required]
    [RegularExpression(@"^(0|\+84)[0-9]{9,10}$")]
    public string Phone { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Address { get; set; }
}
```

**Validation Rules:**
- ✅ FullName: 2-100 ký tự
- ✅ Email: Email hợp lệ
- ✅ Password: Min 6 ký tự, phải có chữ hoa, chữ thường, số
- ✅ Role: Admin, User, Manager
- ✅ Phone: Format Việt Nam
- ✅ Address: 5-200 ký tự

#### UpdateUserRequest
```csharp
public class UpdateUserRequest
{
    [StringLength(100, MinimumLength = 2)]
    public string? FullName { get; set; }

    [RegularExpression(@"^(0|\+84)[0-9]{9,10}$")]
    public string? Phone { get; set; }

    [StringLength(200, MinimumLength = 5)]
    public string? Address { get; set; }

    [RegularExpression("^(Admin|User|Manager)$")]
    public string? Role { get; set; }

    [RegularExpression("^(Active|Inactive|Suspended)$")]
    public string? Status { get; set; }
}
```

**Validation Rules:**
- ✅ Tất cả fields là optional
- ✅ Status: Active, Inactive, Suspended

#### ChangePasswordRequest
```csharp
public class ChangePasswordRequest
{
    [StringLength(100, MinimumLength = 6)]
    public string? OldPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$")]
    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}
```

**Validation Rules:**
- ✅ NewPassword: Min 6 ký tự, phải có chữ hoa, chữ thường, số
- ✅ ConfirmPassword: Phải khớp với NewPassword

---

## 🔧 Cách sử dụng trong Controller

### 1. Kiểm tra ModelState
```csharp
[HttpPost]
public async Task<ActionResult> CreateJob([FromBody] CreateJobRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new
        {
            Message = "Dữ liệu không hợp lệ",
            Errors = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
        });
    }
    
    // Xử lý logic...
}
```

### 2. Automatic Validation với [ApiController]
Với attribute `[ApiController]`, ASP.NET Core tự động validate và trả về 400 nếu ModelState invalid.

---

## ✅ Test Cases

### Test Create Job - Valid Data
```json
POST /api/Jobs
{
  "title": "Senior Developer",
  "description": "We are looking for experienced developer",
  "location": "Ha Noi",
  "salary": 25000000,
  "jobType": "Full-time",
  "createdBy": 1,
  "quantity": 5
}
```
✅ **Result**: 201 Created

### Test Create Job - Invalid Title (Too Short)
```json
POST /api/Jobs
{
  "title": "Dev",
  "description": "We are looking for experienced developer",
  "location": "Ha Noi",
  "jobType": "Full-time",
  "createdBy": 1
}
```
❌ **Result**: 400 Bad Request
```json
{
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Tiêu đề phải từ 5-200 ký tự"
  ]
}
```

### Test Create Job - Invalid JobType
```json
POST /api/Jobs
{
  "title": "Senior Developer",
  "description": "We are looking for experienced developer",
  "location": "Ha Noi",
  "jobType": "Remote",
  "createdBy": 1
}
```
❌ **Result**: 400 Bad Request
```json
{
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Loại công việc phải là: Full-time, Part-time, Internship, hoặc Contract"
  ]
}
```

### Test Create Application - Invalid Phone
```json
POST /api/Applications
{
  "jobId": 1,
  "fullName": "Nguyen Van A",
  "phone": "123",
  "email": "nguyenvana@gmail.com",
  "studentYear": "Year 3",
  "workType": "Part-time"
}
```
❌ **Result**: 400 Bad Request
```json
{
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Số điện thoại không hợp lệ (VD: 0123456789 hoặc +84123456789)"
  ]
}
```

### Test Create User - Invalid Password
```json
POST /api/Users
{
  "fullName": "Test User",
  "email": "test@example.com",
  "password": "123456",
  "role": "User",
  "phone": "0123456789",
  "address": "Ha Noi"
}
```
❌ **Result**: 400 Bad Request
```json
{
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số"
  ]
}
```

### Test Change Password - Mismatch Confirm
```json
PUT /api/Users/5/change-password
{
  "oldPassword": "OldPass123",
  "newPassword": "NewPass123",
  "confirmPassword": "NewPass456"
}
```
❌ **Result**: 400 Bad Request
```json
{
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "Mật khẩu xác nhận không khớp"
  ]
}
```

---

## 🎨 Regex Patterns sử dụng

### Phone Number (Việt Nam)
```regex
^(0|\+84)[0-9]{9,10}$
```
**Matches:**
- ✅ 0123456789
- ✅ +84123456789
- ✅ 0987654321
- ❌ 123456789
- ❌ 84123456789

### Password (Strong)
```regex
^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$
```
**Requirements:**
- ✅ Ít nhất 1 chữ thường (a-z)
- ✅ Ít nhất 1 chữ hoa (A-Z)
- ✅ Ít nhất 1 số (0-9)
- ✅ Tối thiểu 6 ký tự

**Valid Examples:**
- ✅ Pass123
- ✅ MyPassword1
- ✅ Admin@123
- ❌ password (không có chữ hoa và số)
- ❌ PASSWORD123 (không có chữ thường)
- ❌ Pass (quá ngắn)

---

## 💡 Best Practices

### 1. Luôn validate ở cả Client và Server
- Client: UX tốt hơn (feedback ngay lập tức)
- Server: Bảo mật (không tin tưởng client)

### 2. Error Messages rõ ràng
```csharp
[Required(ErrorMessage = "Email là bắt buộc")]  // ✅ Good
[Required]  // ❌ Generic message
```

### 3. Sử dụng DTOs thay vì Entities
```csharp
// ✅ Good
public async Task<ActionResult> CreateJob([FromBody] CreateJobRequest request)

// ❌ Bad
public async Task<ActionResult> CreateJob([FromBody] Job job)
```

### 4. Validate Business Rules trong Service Layer
```csharp
// Check duplicate email
if (await _context.Users.AnyAsync(u => u.Email == request.Email))
{
    return BadRequest(new { Message = "Email đã tồn tại" });
}
```

### 5. Custom Validation Attributes
Tạo custom attribute cho logic phức tạp:
```csharp
[AttributeUsage(AttributeTargets.Property)]
public class FutureDate : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value is DateTime date && date > DateTime.Now)
            return ValidationResult.Success;
            
        return new ValidationResult("Ngày phải là ngày trong tương lai");
    }
}

// Sử dụng
[FutureDate]
public DateTime? Deadline { get; set; }
```

---

## 📊 Summary

| Category | Validations |
|----------|-------------|
| **Jobs** | Title, Description, Location, Salary, JobType, Status, Requirements, Benefits |
| **Applications** | JobId, FullName, Phone, Email, StudentYear, WorkType, Status, CvLink |
| **Users** | FullName, Email, Password, Role, Phone, Address, Status |

**Total Validation Rules**: 30+

---

## 🔒 Security Notes

1. **Password Strength**: Regex đảm bảo password mạnh
2. **Input Length**: StringLength ngăn buffer overflow
3. **Email Validation**: Ngăn SQL injection qua email field
4. **Phone Format**: Chỉ chấp nhận format Việt Nam
5. **Enum Values**: RegularExpression chỉ cho phép giá trị định sẵn

---

Với hệ thống validation này, dữ liệu được ràng buộc chặt chẽ trước khi vào service layer, đảm bảo tính toàn vẹn và bảo mật! ✅
