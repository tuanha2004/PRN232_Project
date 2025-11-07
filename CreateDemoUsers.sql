-- Script tạo user demo cho hệ thống
-- Chạy script này trên database Project_Prn232

USE Project_Prn232;
GO

-- Xóa dữ liệu demo cũ nếu có
DELETE FROM Users WHERE Email IN ('admin@example.com', 'user@example.com');
GO

-- Thêm user Admin
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, Address, CreatedAt, UpdatedAt, Status)
VALUES 
('Admin User', 'admin@example.com', 'admin123', 'Admin', '0123456789', 'Ha Noi', GETDATE(), GETDATE(), 'Active');

-- Thêm user thường
INSERT INTO Users (FullName, Email, PasswordHash, Role, Phone, Address, CreatedAt, UpdatedAt, Status)
VALUES 
('Normal User', 'user@example.com', 'user123', 'User', '0987654321', 'Ho Chi Minh', GETDATE(), GETDATE(), 'Active');

GO

-- Kiểm tra kết quả
SELECT UserId, FullName, Email, Role, Status FROM Users 
WHERE Email IN ('admin@example.com', 'user@example.com');
GO
