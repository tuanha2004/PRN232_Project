# Hướng Dẫn Cấu Hình Email cho Forgot Password

## 📧 Cấu hình Gmail

### Bước 1: Tạo App Password cho Gmail

1. Truy cập: https://myaccount.google.com/security
2. Bật **2-Step Verification** (Xác thực 2 bước) nếu chưa bật
3. Tìm và click vào **App passwords** (Mật khẩu ứng dụng)
4. Chọn:
   - App: **Mail**
   - Device: **Windows Computer** hoặc **Other (Custom name)**
5. Click **Generate** và lưu lại **16-ký tự mật khẩu ứng dụng**

### Bước 2: Cập nhật appsettings.json

Mở file: `API/appsettings.json`

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "Project PRN232",
    "SenderEmail": "your-email@gmail.com",        // ← Thay bằng Gmail của bạn
    "Password": "xxxx xxxx xxxx xxxx"              // ← Thay bằng App Password 16 ký tự
  }
}
```

**Ví dụ:**
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "Project PRN232",
    "SenderEmail": "hatuan0504@gmail.com",
    "Password": "abcd efgh ijkl mnop"
  }
}
```

### Bước 3: Cập nhật appsettings.Development.json (tùy chọn)

Nếu bạn muốn dùng email khác cho môi trường development:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "Project PRN232 - DEV",
    "SenderEmail": "dev-email@gmail.com",
    "Password": "dev-app-password"
  }
}
```

---

## 🔧 Các SMTP Server khác

### Outlook/Hotmail
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@outlook.com",
    "Password": "your-password"
  }
}
```

### Yahoo Mail
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mail.yahoo.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@yahoo.com",
    "Password": "your-app-password"
  }
}
```

---

## ✅ Kiểm tra

1. **Build lại project:**
   ```bash
   dotnet build
   ```

2. **Chạy API:**
   ```bash
   cd API
   dotnet run
   ```

3. **Test Forgot Password:**
   - Vào trang Forgot Password
   - Nhập email đã đăng ký
   - Kiểm tra hộp thư (cả Inbox và Spam)

---

## 🐛 Xử lý lỗi thường gặp

### Lỗi: "Authentication failed"
- ✅ Kiểm tra lại App Password (16 ký tự, không có khoảng trắng)
- ✅ Đảm bảo đã bật 2-Step Verification
- ✅ Tạo lại App Password mới

### Lỗi: "SMTP connection failed"
- ✅ Kiểm tra kết nối internet
- ✅ Firewall/Antivirus có thể chặn port 587
- ✅ Thử đổi SmtpPort sang 465 và bật SSL

### Email vào Spam
- ✅ Đánh dấu "Not Spam" trong Gmail
- ✅ Thêm SenderEmail vào danh bạ
- ✅ Trong production, nên dùng dịch vụ email chuyên nghiệp (SendGrid, AWS SES, MailGun)

---

## 🎯 Cách hoạt động

1. User nhập email trong form Forgot Password
2. API tạo mã xác thực 6 số ngẫu nhiên
3. **EmailService gửi email chứa mã xác thực**
4. Mã cũng được lưu vào Session và hiển thị trên màn hình (dự phòng)
5. User nhập mã và đặt mật khẩu mới
6. Sau khi reset thành công, mã bị xóa khỏi Session

---

## 📝 Lưu ý bảo mật

⚠️ **KHÔNG COMMIT** file `appsettings.json` chứa mật khẩu thật lên Git!

Thêm vào `.gitignore`:
```
**/appsettings.Development.json
**/appsettings.Production.json
```

Hoặc dùng **User Secrets** (khuyến nghị):
```bash
cd API
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
```

---

## 📧 Template Email

Email được gửi sẽ có giao diện đẹp với:
- 🔐 Icon khóa
- 📦 Box màu vàng hiển thị mã 6 số
- ⏰ Cảnh báo hết hạn 15 phút
- ⚠️ Lưu ý bảo mật

Bạn có thể tùy chỉnh template trong file: `API/Services/EmailService.cs` → method `GetEmailTemplate()`
