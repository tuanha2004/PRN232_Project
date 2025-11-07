# Hướng dẫn chạy dự án JWT Authentication với Role-Based Access

## Tổng quan
Dự án này triển khai hệ thống đăng nhập sử dụng JWT (JSON Web Token) và Role-Based Access Control với:
- **API Backend**: ASP.NET Core Web API
- **MVC Frontend**: ASP.NET Core MVC
- **Authentication**: JWT Token
- **Authorization**: Role-based (Admin, User)

## Cấu trúc dự án

### API Project
- `Controllers/AuthenController.cs`: API endpoints cho authentication
  - `POST /api/Authen/login`: Đăng nhập
  - `GET /api/Authen/validate`: Kiểm tra token
  - `GET /api/Authen/admin-only`: Chỉ Admin
  - `GET /api/Authen/user-or-admin`: User và Admin
- `Services/JwtService.cs`: Tạo và xác thực JWT token
- `DTOs/`: Data Transfer Objects (LoginRequest, LoginResponse)
- `Models/User.cs`: Entity User

### MVC Project  
- `Controllers/AccountController.cs`: Controller xử lý login/logout
- `Services/AuthService.cs`: Gọi API và quản lý Session
- `Views/Account/`: Views cho Login, Profile, AdminDashboard
- `Views/Shared/_Layout.cshtml`: Layout với menu động theo trạng thái đăng nhập

## Cài đặt và chạy

### Bước 1: Chuẩn bị Database
1. Mở SQL Server Management Studio
2. Chạy script `CreateDemoUsers.sql` để tạo 2 user demo:
   - **Admin**: admin@example.com / admin123
   - **User**: user@example.com / user123

### Bước 2: Cấu hình
1. Kiểm tra connection string trong `API/appsettings.json`:
```json
"ConnectionStrings": {
  "MyCnn": "server=TUẤNANH-HÀ\\TUANHA;database=Project_Prn232;uid=sA;pwd=SA123456;TrustServerCertificate=True;"
}
```

2. Kiểm tra JWT settings trong `API/appsettings.json`:
```json
"Jwt": {
  "Key": "ThisIsASuperSecretKeyForJwt1234567890",
  "Issuer": "http://localhost:5258",
  "Audience": "http://localhost:5258"
}
```

3. Kiểm tra API URL trong `Project_PRN232/appsettings.json`:
```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5258"
}
```

### Bước 3: Chạy API
```bash
cd API
dotnet run
```
API sẽ chạy tại: http://localhost:5258

### Bước 4: Chạy MVC
```bash
cd Project_PRN232
dotnet run
```
MVC sẽ chạy tại: http://localhost:5xxx (port tự động)

## Sử dụng

### Đăng nhập
1. Truy cập: http://localhost:xxxx/Account/Login
2. Sử dụng một trong hai tài khoản demo:
   - Admin: admin@example.com / admin123
   - User: user@example.com / user123

### Tính năng theo Role

#### User thường
- Xem trang chủ
- Xem thông tin cá nhân (Profile)
- Không thể truy cập Admin Dashboard

#### Admin
- Tất cả tính năng của User
- Truy cập Admin Dashboard
- Menu Admin Dashboard hiển thị trên navbar

### Endpoints API để test

#### 1. Login (Public)
```http
POST http://localhost:5258/api/Authen/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "admin123"
}
```

Response:
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 1,
    "fullName": "Admin User",
    "email": "admin@example.com",
    "role": "Admin"
  }
}
```

#### 2. Validate Token (Requires Authentication)
```http
GET http://localhost:5258/api/Authen/validate
Authorization: Bearer YOUR_JWT_TOKEN
```

#### 3. Admin Only Endpoint
```http
GET http://localhost:5258/api/Authen/admin-only
Authorization: Bearer YOUR_JWT_TOKEN
```
Chỉ Admin mới truy cập được.

#### 4. User or Admin Endpoint
```http
GET http://localhost:5258/api/Authen/user-or-admin
Authorization: Bearer YOUR_JWT_TOKEN
```
Cả User và Admin đều truy cập được.

## Kiến trúc

### Flow đăng nhập
1. User nhập email/password trên MVC Form
2. MVC gọi API: `POST /api/Authen/login`
3. API validate thông tin trong database
4. API tạo JWT token với claims (email, role)
5. API trả về token + thông tin user
6. MVC lưu token vào Session
7. MVC redirect về trang chủ

### Flow xác thực
1. Client gửi request với token trong header
2. JWT Middleware validate token
3. Nếu valid, set User.Identity và Claims
4. Controller kiểm tra [Authorize] và Role
5. Xử lý request hoặc trả về 401/403

## Bảo mật

### JWT Token
- **Thuật toán**: HMAC-SHA256
- **Thời gian sống**: 1 giờ
- **Claims**: Name (email), Role
- **Lưu trữ**: Session (MVC), không lưu trong cookie/localStorage

### Password
⚠️ **Lưu ý**: Trong demo này password được lưu dạng plaintext để đơn giản.
Trong production, **BẮT BUỘC** phải hash password bằng:
- BCrypt
- PBKDF2
- Argon2

### Role-Based Authorization
```csharp
[Authorize] // Yêu cầu đăng nhập
[Authorize(Roles = "Admin")] // Chỉ Admin
[Authorize(Roles = "User,Admin")] // User hoặc Admin
```

## Các tính năng đã implement

✅ JWT Token Generation  
✅ Token Validation  
✅ Role-Based Access Control  
✅ Login API  
✅ Login MVC Form  
✅ Session Management  
✅ Dynamic Navbar (hiển thị theo login state)  
✅ Protected Routes  
✅ Admin Dashboard  
✅ User Profile  
✅ Logout Function  

## Mở rộng

### Thêm role mới
1. Thêm role vào database (cột `Role` trong bảng `Users`)
2. Sử dụng `[Authorize(Roles = "NewRole")]` trên controller/action

### Thêm claims khác
Trong `JwtService.GenerateToken()`:
```csharp
var claims = new[]
{
    new Claim(ClaimTypes.Name, username),
    new Claim(ClaimTypes.Role, role),
    new Claim("UserId", userId.ToString()), // Thêm claim mới
    new Claim("Department", department)
};
```

### Refresh Token
Hiện tại chưa implement refresh token. Để thêm:
1. Tạo bảng RefreshTokens
2. Generate refresh token khi login
3. Implement endpoint `/api/Authen/refresh`
4. Client dùng refresh token để lấy access token mới

## Troubleshooting

### Lỗi 401 Unauthorized
- Kiểm tra token có được gửi trong header không
- Kiểm tra token có hết hạn không
- Kiểm tra JWT settings (Key, Issuer, Audience) giống nhau

### Lỗi 403 Forbidden  
- User không có quyền truy cập endpoint này
- Kiểm tra Role trong database và [Authorize(Roles = "...")]

### API không kết nối được
- Kiểm tra API đã chạy chưa (http://localhost:5258)
- Kiểm tra `ApiSettings:BaseUrl` trong MVC appsettings.json
- Kiểm tra CORS đã được config trong API

## Liên hệ
Nếu có câu hỏi, vui lòng liên hệ qua email hoặc tạo issue.
