# 🚀 Hướng dẫn chạy NHANH

## ⚡ Quick Start (3 bước)

### 1️⃣ Tạo User Demo trong Database
Mở SQL Server và chạy file `CreateDemoUsers.sql`

Hoặc chạy lệnh SQL này:
```sql
USE Project_Prn232;
GO

INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, Address, CreatedAt, UpdatedAt, Status)
VALUES 
('Admin User', 'admin@example.com', 'admin123', 'Admin', '0123456789', 'Ha Noi', GETDATE(), GETDATE(), 'Active'),
('Normal User', 'user@example.com', 'user123', 'User', '0987654321', 'Ho Chi Minh', GETDATE(), GETDATE(), 'Active');
GO
```

### 2️⃣ Chạy API (Terminal 1)
```powershell
cd API
dotnet run
```
✅ API chạy tại: **http://localhost:5258**

### 3️⃣ Chạy MVC (Terminal 2)  
```powershell
cd Project_PRN232
dotnet run
```
✅ MVC chạy tại: **http://localhost:xxxx** (xem output để biết port)

---

## 🔑 Tài khoản Demo

### Admin
- **Email**: admin@example.com
- **Password**: admin123
- **Quyền**: Full access + Admin Dashboard

### User
- **Email**: user@example.com  
- **Password**: user123
- **Quyền**: User thông thường

---

## 📝 Test API bằng Swagger

1. Truy cập: http://localhost:5258/swagger
2. Thử endpoint `POST /api/Authen/login`:
```json
{
  "email": "admin@example.com",
  "password": "admin123"
}
```
3. Copy token từ response
4. Click "Authorize" và paste token
5. Test các endpoint khác

---

## 🎯 Các trang quan trọng

| Trang | URL | Quyền |
|-------|-----|-------|
| Trang chủ | /Home/Index | Public |
| Đăng nhập | /Account/Login | Public |
| Profile | /Account/Profile | User + Admin |
| Admin Dashboard | /Account/AdminDashboard | Admin only |

---

## ⚠️ Nếu gặp lỗi

### API không chạy được
- Kiểm tra connection string trong `API/appsettings.json`
- Đảm bảo SQL Server đã chạy
- Port 5258 không bị chiếm

### MVC không kết nối API
- Kiểm tra API đã chạy chưa
- Kiểm tra `ApiSettings:BaseUrl` trong `Project_PRN232/appsettings.json`

### Login không thành công
- Chạy lại script tạo user demo
- Kiểm tra database có 2 user chưa
- Kiểm tra JWT settings trong appsettings.json

---

## 📚 Đọc thêm

Xem file **README_JWT.md** để hiểu chi tiết về:
- Kiến trúc hệ thống
- Flow authentication
- API endpoints
- Security best practices
