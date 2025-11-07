-- =============================================
-- Script Update Status từ "Active" sang "Open"
-- Database: Project_Prn232
-- =============================================

USE [Project_Prn232]
GO

PRINT '========== BẮT ĐẦU UPDATE STATUS =========='
PRINT ''

-- Kiểm tra trước khi update
PRINT 'Số jobs có Status = ''Active'': ' + CAST((SELECT COUNT(*) FROM [dbo].[Jobs] WHERE [Status] = 'Active') AS VARCHAR)
PRINT ''

-- Update tất cả jobs có Status = 'Active' thành 'Open'
UPDATE [dbo].[Jobs]
SET [Status] = 'Open',
    [UpdatedAt] = GETDATE()
WHERE [Status] = 'Active'

PRINT 'Đã update: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' jobs'
PRINT ''

-- Tự động đóng các jobs đã quá hạn
UPDATE [dbo].[Jobs]
SET [Status] = 'Closed',
    [UpdatedAt] = GETDATE()
WHERE [Status] = 'Open' 
  AND [EndDate] IS NOT NULL 
  AND [EndDate] < CAST(GETDATE() AS DATE)

PRINT 'Đã tự động đóng: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' jobs quá hạn'
PRINT ''

-- Kiểm tra sau khi update
PRINT '========== KẾT QUẢ SAU UPDATE =========='
PRINT 'Jobs có Status = ''Open'': ' + CAST((SELECT COUNT(*) FROM [dbo].[Jobs] WHERE [Status] = 'Open') AS VARCHAR)
PRINT 'Jobs có Status = ''Closed'': ' + CAST((SELECT COUNT(*) FROM [dbo].[Jobs] WHERE [Status] = 'Closed') AS VARCHAR)
PRINT 'Jobs có Status = ''Active'': ' + CAST((SELECT COUNT(*) FROM [dbo].[Jobs] WHERE [Status] = 'Active') AS VARCHAR)
PRINT ''

-- Hiển thị chi tiết các jobs
SELECT 
    [JobId],
    [Title],
    [Status],
    [StartDate],
    [EndDate],
    CASE 
        WHEN [EndDate] IS NULL THEN 'Không có EndDate'
        WHEN [EndDate] < CAST(GETDATE() AS DATE) THEN 'Đã quá hạn'
        ELSE 'Còn hạn'
    END AS [JobStatus],
    [UpdatedAt]
FROM [dbo].[Jobs]
ORDER BY [Status], [JobId]

PRINT ''
PRINT '========== HOÀN TẤT =========='
GO
