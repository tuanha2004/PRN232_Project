# Hướng dẫn Test Chức năng Check-in / Check-out

## Bước 1: Approve Application
Trước khi sinh viên có thể check-in, cần có application được approved.

### Cách 1: Sử dụng SQL Script
```sql
-- Xem tất cả applications
SELECT 
    a.ApplicationID,
    a.StudentID,
    u.FullName as StudentName,
    a.JobID,
    j.Title as JobTitle,
    a.Status,
    a.AppliedAt
FROM Applications a
JOIN Users u ON a.StudentID = u.UserID
JOIN Jobs j ON a.JobID = j.JobID
ORDER BY a.AppliedAt DESC;

-- Approve một application (thay ApplicationID phù hợp)
UPDATE Applications SET Status = 'Approved' WHERE ApplicationID = 1;
```

### Cách 2: Tạo API Endpoint cho Admin/Provider
Cần thêm endpoint PUT `/api/Applications/{id}/status` để Admin/Provider có thể approve.

## Bước 2: Test Check-in
1. **Đăng nhập bằng tài khoản Student** đã được approve
2. Vào trang **Job Detail** của công việc đã được approve
3. Nhấn nút **"Check-in ngay"**
4. Hệ thống sẽ ghi lại thời gian check-in

## Bước 3: Xem Lịch sử Check-in
1. Click vào dropdown ở góc phải (tên user)
2. Chọn **"Check-in / Check-out"**
3. Xem lịch sử và thời gian làm việc real-time

## Bước 4: Test Check-out
1. Trong trang Check-in/Check-out
2. Nhấn nút **"Check-out"** 
3. Hệ thống sẽ tính số giờ đã làm việc

## Kiểm tra các trường hợp lỗi

### Lỗi: "Check-in thất bại"
**Nguyên nhân có thể:**
- Application chưa được approve (Status = "Pending" hoặc "Rejected")
- Chưa nộp đơn ứng tuyển cho job này
- Đã check-in rồi mà chưa check-out

**Giải pháp:**
- Kiểm tra Status của Application trong database
- Approve application bằng SQL hoặc API
- Check-out trước khi check-in lại

### Lỗi: "Bạn đã check-in rồi"
**Nguyên nhân:** 
- Có CheckinRecord với CheckoutTime = NULL

**Giải pháp:**
- Check-out trước
- Hoặc xóa record đó trong database (chỉ để test)

## API Endpoints

### Check-in
```
POST /api/CheckinRecords/checkin
Body: { "jobId": 1 }
Headers: Authorization: Bearer {token}
```

### Check-out
```
POST /api/CheckinRecords/checkout
Body: { "checkinId": 1 }
Headers: Authorization: Bearer {token}
```

### Lấy lịch sử
```
GET /api/CheckinRecords/my-records
Headers: Authorization: Bearer {token}
```

### Lấy checkin hiện tại
```
GET /api/CheckinRecords/current
Headers: Authorization: Bearer {token}
```
