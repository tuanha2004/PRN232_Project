-- Script để approve application cho test check-in
-- Thay đổi các giá trị phù hợp với database của bạn

-- Xem tất cả applications
SELECT 
    a.ApplicationID,
    a.StudentID,
    u.FullName as StudentName,
    a.JobID,
    j.JobTitle,
    a.Status,
    a.AppliedAt
FROM Applications a
JOIN Users u ON a.StudentID = u.UserID
JOIN Jobs j ON a.JobID = j.JobID
ORDER BY a.AppliedAt DESC;

-- Approve application đầu tiên (hoặc chọn ApplicationID cụ thể)
-- Uncomment dòng dưới và thay đổi ApplicationID
-- UPDATE Applications SET Status = 'Approved' WHERE ApplicationID = 1;

-- Kiểm tra lại
-- SELECT * FROM Applications WHERE ApplicationID = 1;
