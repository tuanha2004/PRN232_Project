# 📋 Tài liệu API - Role-Based Access Control

## 🔐 Phân quyền theo Role

Hệ thống có 2 roles chính:
- **Admin**: Toàn quyền quản trị hệ thống
- **User**: Người dùng thông thường

---

## 📌 1. Authentication API (`/api/Authen`)

### 🔓 Public Endpoints (Không cần đăng nhập)

#### Login
```http
POST /api/Authen/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "admin123"
}
```

**Response:**
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

### 🔒 Protected Endpoints (Cần JWT Token)

#### Validate Token
```http
GET /api/Authen/validate
Authorization: Bearer YOUR_JWT_TOKEN
```

#### Admin Only Endpoint
```http
GET /api/Authen/admin-only
Authorization: Bearer YOUR_JWT_TOKEN
```
✅ **Admin**: OK  
❌ **User**: 403 Forbidden

#### User or Admin Endpoint
```http
GET /api/Authen/user-or-admin
Authorization: Bearer YOUR_JWT_TOKEN
```
✅ **Admin**: OK  
✅ **User**: OK

---

## 👥 2. Users API (`/api/Users`)

### GET `/api/Users` - Lấy danh sách tất cả users
**Quyền:** 🔴 **Chỉ Admin**

```http
GET /api/Users
Authorization: Bearer ADMIN_TOKEN
```

### GET `/api/Users/{id}` - Xem thông tin user
**Quyền:** 
- 🟢 **User**: Chỉ xem thông tin của chính mình
- 🔴 **Admin**: Xem thông tin user bất kỳ

```http
GET /api/Users/5
Authorization: Bearer YOUR_TOKEN
```

### GET `/api/Users/me` - Lấy thông tin user đang đăng nhập
**Quyền:** 🟡 **User & Admin**

```http
GET /api/Users/me
Authorization: Bearer YOUR_TOKEN
```

### POST `/api/Users` - Tạo user mới
**Quyền:** 🔴 **Chỉ Admin**

```http
POST /api/Users
Authorization: Bearer ADMIN_TOKEN
Content-Type: application/json

{
  "fullName": "New User",
  "email": "newuser@example.com",
  "passwordHash": "password123",
  "role": "User",
  "phone": "0123456789",
  "address": "Ha Noi"
}
```

### PUT `/api/Users/{id}` - Cập nhật thông tin user
**Quyền:**
- 🟢 **User**: Chỉ cập nhật thông tin của mình (không đổi được Role/Status)
- 🔴 **Admin**: Cập nhật thông tin user bất kỳ (bao gồm Role/Status)

```http
PUT /api/Users/5
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "fullName": "Updated Name",
  "phone": "0987654321",
  "address": "Ho Chi Minh",
  "role": "Admin",      // Chỉ Admin mới đổi được
  "status": "Inactive"  // Chỉ Admin mới đổi được
}
```

### PUT `/api/Users/{id}/change-password` - Đổi mật khẩu
**Quyền:**
- 🟢 **User**: Đổi mật khẩu của mình (cần nhập mật khẩu cũ)
- 🔴 **Admin**: Đổi mật khẩu user bất kỳ (không cần mật khẩu cũ)

```http
PUT /api/Users/5/change-password
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "oldPassword": "old123",  // User bắt buộc, Admin không cần
  "newPassword": "new123"
}
```

### DELETE `/api/Users/{id}` - Xóa user
**Quyền:** 🔴 **Chỉ Admin**

```http
DELETE /api/Users/5
Authorization: Bearer ADMIN_TOKEN
```

---

## 💼 3. Jobs API (`/api/Jobs`)

### GET `/api/Jobs` - Lấy danh sách công việc
**Quyền:** 🟡 **User & Admin** (Ai đã login đều xem được)

```http
GET /api/Jobs
Authorization: Bearer YOUR_TOKEN
```

### GET `/api/Jobs/{id}` - Xem chi tiết công việc
**Quyền:** 🟡 **User & Admin**

```http
GET /api/Jobs/5
Authorization: Bearer YOUR_TOKEN
```

### POST `/api/Jobs` - Tạo công việc mới
**Quyền:** 🔴 **Chỉ Admin**

```http
POST /api/Jobs
Authorization: Bearer ADMIN_TOKEN
Content-Type: application/json

{
  "title": "Web Developer",
  "description": "Develop web applications",
  "location": "Ha Noi",
  "salary": 15000000,
  "jobType": "Full-time",
  "createdBy": 1
}
```

### PUT `/api/Jobs/{id}` - Cập nhật công việc
**Quyền:** 🔴 **Chỉ Admin**

```http
PUT /api/Jobs/5
Authorization: Bearer ADMIN_TOKEN
Content-Type: application/json

{
  "jobId": 5,
  "title": "Senior Web Developer",
  "description": "Updated description",
  "location": "Ho Chi Minh",
  "salary": 20000000,
  "jobType": "Full-time"
}
```

### DELETE `/api/Jobs/{id}` - Xóa công việc
**Quyền:** 🔴 **Chỉ Admin**

```http
DELETE /api/Jobs/5
Authorization: Bearer ADMIN_TOKEN
```

---

## 📝 4. Applications API (`/api/Applications`)

### GET `/api/Applications` - Lấy TẤT CẢ đơn ứng tuyển
**Quyền:** 🔴 **Chỉ Admin**

```http
GET /api/Applications
Authorization: Bearer ADMIN_TOKEN
```

### GET `/api/Applications/my` - Lấy đơn ứng tuyển của mình
**Quyền:** 🟡 **User & Admin**

```http
GET /api/Applications/my
Authorization: Bearer YOUR_TOKEN
```

### GET `/api/Applications/{id}` - Xem chi tiết đơn ứng tuyển
**Quyền:**
- 🟢 **User**: Chỉ xem đơn của mình
- 🔴 **Admin**: Xem mọi đơn

```http
GET /api/Applications/5
Authorization: Bearer YOUR_TOKEN
```

### POST `/api/Applications` - Nộp đơn ứng tuyển
**Quyền:** 🟡 **User & Admin**

```http
POST /api/Applications
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

{
  "jobId": 1,
  "fullName": "Nguyen Van A",
  "phone": "0123456789",
  "email": "nguyenvana@gmail.com",
  "studentYear": "Year 3",
  "workType": "Part-time"
}
```
> **Lưu ý:** `userId` tự động lấy từ token, không cần gửi

### PUT `/api/Applications/{id}/status` - Cập nhật trạng thái đơn
**Quyền:** 🔴 **Chỉ Admin**

```http
PUT /api/Applications/5/status
Authorization: Bearer ADMIN_TOKEN
Content-Type: application/json

{
  "status": "Approved"
}
```

Các trạng thái: `Pending`, `Approved`, `Rejected`

### DELETE `/api/Applications/{id}` - Xóa đơn ứng tuyển
**Quyền:**
- 🟢 **User**: Chỉ xóa đơn của mình
- 🔴 **Admin**: Xóa mọi đơn

```http
DELETE /api/Applications/5
Authorization: Bearer YOUR_TOKEN
```

---

## 🔑 Cách sử dụng JWT Token

### 1. Login để lấy token
```http
POST /api/Authen/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "admin123"
}
```

### 2. Copy token từ response
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AZXhhbXBsZS5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTczMDMxOTYwMCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MjU4IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo1MjU4In0.xyz123..."
}
```

### 3. Thêm vào Header khi gọi API
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📊 Bảng tóm tắt phân quyền

| Endpoint | Public | User | Admin |
|----------|--------|------|-------|
| **Authentication** |
| POST /api/Authen/login | ✅ | ✅ | ✅ |
| GET /api/Authen/validate | ❌ | ✅ | ✅ |
| GET /api/Authen/admin-only | ❌ | ❌ | ✅ |
| GET /api/Authen/user-or-admin | ❌ | ✅ | ✅ |
| **Users** |
| GET /api/Users | ❌ | ❌ | ✅ |
| GET /api/Users/{id} | ❌ | ✅ (own) | ✅ (all) |
| GET /api/Users/me | ❌ | ✅ | ✅ |
| POST /api/Users | ❌ | ❌ | ✅ |
| PUT /api/Users/{id} | ❌ | ✅ (own, limited) | ✅ (all) |
| PUT /api/Users/{id}/change-password | ❌ | ✅ (own) | ✅ (all) |
| DELETE /api/Users/{id} | ❌ | ❌ | ✅ |
| **Jobs** |
| GET /api/Jobs | ❌ | ✅ | ✅ |
| GET /api/Jobs/{id} | ❌ | ✅ | ✅ |
| POST /api/Jobs | ❌ | ❌ | ✅ |
| PUT /api/Jobs/{id} | ❌ | ❌ | ✅ |
| DELETE /api/Jobs/{id} | ❌ | ❌ | ✅ |
| **Applications** |
| GET /api/Applications | ❌ | ❌ | ✅ |
| GET /api/Applications/my | ❌ | ✅ | ✅ |
| GET /api/Applications/{id} | ❌ | ✅ (own) | ✅ (all) |
| POST /api/Applications | ❌ | ✅ | ✅ |
| PUT /api/Applications/{id}/status | ❌ | ❌ | ✅ |
| DELETE /api/Applications/{id} | ❌ | ✅ (own) | ✅ (all) |

---

## 🎯 Test Cases

### Scenario 1: User thường
```bash
# 1. Login as User
POST /api/Authen/login
{ "email": "user@example.com", "password": "user123" }

# 2. Xem danh sách Jobs (OK)
GET /api/Jobs

# 3. Nộp đơn ứng tuyển (OK)
POST /api/Applications

# 4. Xem đơn của mình (OK)
GET /api/Applications/my

# 5. Thử tạo Job mới (FAIL - 403 Forbidden)
POST /api/Jobs

# 6. Thử xem tất cả Applications (FAIL - 403 Forbidden)
GET /api/Applications
```

### Scenario 2: Admin
```bash
# 1. Login as Admin
POST /api/Authen/login
{ "email": "admin@example.com", "password": "admin123" }

# 2. Xem tất cả users (OK)
GET /api/Users

# 3. Tạo Job mới (OK)
POST /api/Jobs

# 4. Xem tất cả đơn ứng tuyển (OK)
GET /api/Applications

# 5. Duyệt đơn (OK)
PUT /api/Applications/5/status
{ "status": "Approved" }

# 6. Xóa user (OK)
DELETE /api/Users/10
```

---

## 🔒 Security Best Practices

1. **Token Lifetime**: Token hết hạn sau 1 giờ
2. **HTTPS**: Luôn sử dụng HTTPS trong production
3. **Password**: Nên hash password bằng BCrypt/Argon2
4. **CORS**: Chỉ cho phép origin tin cậy
5. **Rate Limiting**: Giới hạn số request để tránh brute force
6. **Logging**: Log tất cả các hành động quan trọng

---

## 📞 Support

Nếu gặp lỗi 401/403, kiểm tra:
- Token có hợp lệ không
- Token có hết hạn không
- Role có đủ quyền không
- Header Authorization có đúng format không
