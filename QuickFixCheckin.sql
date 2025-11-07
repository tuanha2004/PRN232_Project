-- Quick check và approve application cho JobID 12

-- 1. Kiểm tra Job có tồn tại không
SELECT JobID, Title, Status, StartDate, EndDate 
FROM Jobs 
WHERE JobID = 12;

-- 2. Kiểm tra user alice@example.com
SELECT UserID, Email, FullName, Role 
FROM Users 
WHERE Email = 'alice@example.com';

-- 3. Kiểm tra application của alice cho job 12
SELECT 
    a.ApplicationID,
    a.StudentID,
    a.JobID,
    a.Status,
    a.AppliedAt
FROM Applications a
WHERE a.StudentID = (SELECT UserID FROM Users WHERE Email = 'alice@example.com')
  AND a.JobID = 12;

-- 4. Nếu chưa có application, tạo và approve ngay
-- Uncomment 2 dòng dưới nếu cần
/*
INSERT INTO Applications (StudentID, JobID, Phone, StudentYear, WorkType, Status, AppliedAt)
SELECT UserID, 12, '0123456789', 'Year 3', 'Part-time', 'Approved', GETDATE()
FROM Users WHERE Email = 'alice@example.com';
*/

-- 5. Hoặc approve application hiện có
-- Uncomment dòng dưới để approve
/*
UPDATE Applications 
SET Status = 'Approved' 
WHERE StudentID = (SELECT UserID FROM Users WHERE Email = 'alice@example.com')
  AND JobID = 12;
*/

-- 6. Xác nhận lại
SELECT 
    a.ApplicationID,
    u.Email,
    j.Title as JobTitle,
    a.Status
FROM Applications a
JOIN Users u ON a.StudentID = u.UserID
JOIN Jobs j ON a.JobID = j.JobID
WHERE u.Email = 'alice@example.com' AND j.JobID = 12;
