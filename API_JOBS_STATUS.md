# API Jobs - Quản lý trạng thái công việc

## 📋 Tổng quan

Hệ thống tự động quản lý trạng thái công việc dựa trên EndDate và cho phép Provider tự đóng/mở lại jobs.

## 🔄 Logic tự động

### 1. **Tự động đóng jobs khi quá hạn**
- Mỗi khi gọi API `GET /api/Jobs` hoặc `GET /api/Jobs/{id}`, hệ thống sẽ:
  - Kiểm tra tất cả jobs có `Status = "Active"`
  - So sánh `EndDate` với ngày hiện tại
  - Nếu `EndDate < Today` → Tự động chuyển `Status = "Closed"`

### 2. **Provider có thể đóng job thủ công**
- Provider có thể đóng job bất cứ lúc nào (không cần đợi đến EndDate)
- Lý do: Đã tuyển đủ người, hoặc không muốn tuyển thêm

## 🔐 Quyền truy cập

| Endpoint | Admin | Provider | User/Public |
|----------|-------|----------|-------------|
| GET Jobs | ✅ | ✅ | ✅ (Không cần login) |
| GET Job Detail | ✅ | ✅ | ✅ (Không cần login) |
| POST Create | ✅ | ❌ | ❌ |
| PUT Update | ✅ | ✅ (Chỉ job của họ) | ❌ |
| PUT Close | ✅ | ✅ (Chỉ job của họ) | ❌ |
| PUT Reopen | ✅ | ✅ (Chỉ job của họ) | ❌ |
| DELETE | ✅ | ❌ | ❌ |

## 📡 API Endpoints

### 1. GET /api/Jobs
Lấy danh sách tất cả jobs (public, không cần đăng nhập)

**Request:**
```http
GET /api/Jobs
```

**Response:**
```json
[
  {
    "jobId": 1,
    "title": "Mobile App Tester",
    "description": "Test Android/iOS apps",
    "location": "Remote",
    "salary": 50000.00,
    "startDate": "2025-11-01",
    "endDate": "2025-12-01",
    "status": "Active",
    "createdAt": "2025-10-28T21:19:55",
    "updatedAt": "2025-10-28T21:19:55",
    "providerName": "Công ty ABC Technology",
    "providerEmail": "provider1@abc.com"
  }
]
```

**Status values:**
- `"Active"` - Đang tuyển (chưa quá hạn, chưa bị đóng)
- `"Closed"` - Đã đóng (quá hạn hoặc Provider đóng thủ công)
- `"Inactive"` - Đã xóa (bởi Admin)

---

### 2. GET /api/Jobs/{id}
Xem chi tiết một job (public)

**Request:**
```http
GET /api/Jobs/1
```

**Response:** Tương tự GET /api/Jobs

---

### 3. POST /api/Jobs
Tạo job mới (CHỈ ADMIN)

**Authorization:** `Bearer {token}` - Role: Admin

**Request:**
```json
{
  "title": "Software Developer",
  "description": "Develop web applications",
  "location": "Hanoi",
  "salary": 80000,
  "startDate": "2025-12-01",
  "endDate": "2026-02-01",
  "providerId": 3
}
```

**Response:**
```json
{
  "jobId": 11,
  "title": "Software Developer",
  "status": "Active",
  "createdAt": "2025-11-07T10:00:00",
  ...
}
```

---

### 4. PUT /api/Jobs/{id}
Cập nhật job (Admin hoặc Provider của job đó)

**Authorization:** `Bearer {token}` - Role: Admin, Provider

**Request:**
```json
{
  "title": "Senior Software Developer",
  "description": "Updated description",
  "salary": 90000,
  "endDate": "2026-03-01",
  "status": "Active"
}
```

**Response:**
```json
{
  "message": "Cập nhật thành công",
  "job": { ... }
}
```

**Lưu ý:**
- Provider chỉ có thể update job của họ (kiểm tra ProviderId)
- Admin có thể update bất kỳ job nào

---

### 5. PUT /api/Jobs/{id}/close
Đóng job (Provider đóng thủ công)

**Authorization:** `Bearer {token}` - Role: Admin, Provider

**Request:**
```http
PUT /api/Jobs/1/close
```

**Response:**
```json
{
  "message": "Đã đóng công việc thành công",
  "job": {
    "jobId": 1,
    "status": "Closed",
    "updatedAt": "2025-11-07T14:30:00",
    ...
  }
}
```

**Use cases:**
- Đã tuyển đủ người
- Không muốn tuyển thêm
- Tạm dừng tuyển dụng

**Error responses:**
- `400` - Job đã được đóng rồi
- `403` - Provider cố đóng job không phải của họ
- `404` - Job không tồn tại

---

### 6. PUT /api/Jobs/{id}/reopen
Mở lại job đã đóng (nếu chưa quá hạn)

**Authorization:** `Bearer {token}` - Role: Admin, Provider

**Request:**
```http
PUT /api/Jobs/1/reopen
```

**Response:**
```json
{
  "message": "Đã mở lại công việc thành công",
  "job": {
    "jobId": 1,
    "status": "Active",
    "updatedAt": "2025-11-07T15:00:00",
    ...
  }
}
```

**Conditions:**
- Job phải đang ở trạng thái `"Closed"`
- EndDate phải >= ngày hiện tại (chưa quá hạn)

**Error responses:**
- `400` - Job đang Active hoặc đã quá hạn
- `403` - Provider cố mở job không phải của họ
- `404` - Job không tồn tại

---

### 7. DELETE /api/Jobs/{id}
Xóa job (CHỈ ADMIN) - Soft delete

**Authorization:** `Bearer {token}` - Role: Admin

**Request:**
```http
DELETE /api/Jobs/1
```

**Response:**
```json
{
  "message": "Xóa công việc thành công"
}
```

**Lưu ý:** Chỉ set `Status = "Inactive"`, không xóa thật khỏi database

---

## 🔄 Workflow ví dụ

### Scenario 1: Job tự động đóng khi hết hạn
```
1. Provider tạo job: EndDate = "2025-12-01", Status = "Active"
2. Hôm nay là 2025-12-02
3. User gọi GET /api/Jobs
   → Hệ thống tự động: Status = "Closed"
4. Job hiển thị với badge đỏ "Đã đóng"
```

### Scenario 2: Provider đóng job thủ công
```
1. Provider login
2. Gọi PUT /api/Jobs/1/close
3. Job Status = "Closed" ngay lập tức
4. Lý do: Đã tuyển đủ người
```

### Scenario 3: Provider mở lại job
```
1. Job đã đóng: Status = "Closed", EndDate = "2025-12-15"
2. Hôm nay: 2025-12-01 (chưa quá hạn)
3. Provider gọi PUT /api/Jobs/1/reopen
4. Job Status = "Active" → Có thể nhận ứng tuyển
```

### Scenario 4: Không thể mở lại job quá hạn
```
1. Job: Status = "Closed", EndDate = "2025-11-01"
2. Hôm nay: 2025-11-07 (đã quá hạn)
3. Provider gọi PUT /api/Jobs/1/reopen
4. Error 400: "Không thể mở lại công việc đã quá hạn"
5. Provider phải update EndDate trước
```

---

## 🎨 Frontend Display

### Badge Status:
- **Active** (Xanh lá): `<span class="badge bg-success">Đang tuyển</span>`
- **Closed** (Đỏ): `<span class="badge bg-danger">Đã đóng</span>`
- **Inactive** (Xám): `<span class="badge bg-secondary">Đã xóa</span>`

### Filter mặc định:
- Trang Index chỉ hiển thị jobs `Status = "Active"` khi load
- User có thể filter để xem cả `"Closed"` jobs

---

## ⚠️ Lưu ý quan trọng

1. **Automatic Status Update:**
   - Chỉ cập nhật khi có request đến API
   - Không chạy background job
   - Performance: O(n) với n = số jobs Active

2. **Provider Permissions:**
   - Chỉ quản lý jobs của họ (check ProviderId)
   - Admin có quyền full

3. **EndDate vs Manual Close:**
   - Quá EndDate → Auto close (không thể reopen)
   - Manual close → Có thể reopen (nếu chưa quá EndDate)

4. **Database:**
   - Status types: "Active", "Closed", "Inactive"
   - Không có "Open" (đã đổi thành "Active")

---

## 🧪 Test Cases

### Test 1: Auto close expired jobs
```bash
# Tạo job với EndDate = yesterday
POST /api/Jobs
{
  "endDate": "2025-11-06", # Yesterday
  ...
}

# Get jobs → Status should be "Closed"
GET /api/Jobs
```

### Test 2: Provider close job manually
```bash
# Login as Provider
POST /api/Authen/login

# Close their job
PUT /api/Jobs/1/close

# Verify status
GET /api/Jobs/1
# Expected: status = "Closed"
```

### Test 3: Reopen valid job
```bash
# Job: Closed, EndDate = tomorrow
PUT /api/Jobs/1/reopen
# Expected: status = "Active"
```

### Test 4: Cannot reopen expired job
```bash
# Job: Closed, EndDate = yesterday
PUT /api/Jobs/1/reopen
# Expected: 400 Error
```

---

**Ngày cập nhật:** 2025-11-07
