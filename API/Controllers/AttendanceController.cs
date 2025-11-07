using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using API.Models;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Provider")]
    public class AttendanceController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;

        public AttendanceController(ProjectPrn232Context context)
        {
            _context = context;
        }

        // Helper method to get current provider ID from JWT token
        private async Task<int?> GetCurrentProviderIdAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user?.Role != "Provider")
            {
                return null;
            }

            // Provider ID chính là User ID vì Provider là User với Role = "Provider"
            return user.UserId;
        }

        /// <summary>
        /// GET: api/Attendance
        /// Lấy tất cả checkin records của các students đang làm việc cho provider
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttendanceRecords()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            // Lấy checkin records của các jobs thuộc provider này
            var records = await _context.CheckinRecords
                .Include(c => c.Student)
                .Include(c => c.Job)
                .Where(c => c.Job != null && c.Job.ProviderId == userId)
                .OrderByDescending(c => c.CheckinTime)
                .Select(c => new
                {
                    c.CheckinId,
                    c.StudentId,
                    StudentName = c.Student != null ? c.Student.FullName : null,
                    StudentEmail = c.Student != null ? c.Student.Email : null,
                    c.JobId,
                    JobTitle = c.Job != null ? c.Job.Title : null,
                    c.CheckinTime,
                    c.CheckoutTime,
                    WorkDuration = c.CheckoutTime.HasValue && c.CheckinTime.HasValue
                        ? Math.Round((c.CheckoutTime.Value - c.CheckinTime.Value).TotalHours, 2)
                        : (double?)null,
                    Status = c.CheckoutTime.HasValue ? "Completed" : "In Progress"
                })
                .ToListAsync();

            return Ok(records);
        }

        /// <summary>
        /// GET: api/Attendance/job/{jobId}
        /// Lấy attendance records của một job cụ thể
        /// </summary>
        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetAttendanceByJob(int jobId)
        {
            var providerId = await GetCurrentProviderIdAsync();
            if (providerId == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid provider token" });
            }

            // Kiểm tra job có thuộc provider này không
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
            {
                return NotFound(new { Success = false, Message = "Job không tồn tại" });
            }

            if (job.ProviderId != providerId)
            {
                return Forbid();
            }

            var records = await _context.CheckinRecords
                .Include(c => c.Student)
                .Include(c => c.Job)
                .Where(c => c.JobId == jobId)
                .OrderByDescending(c => c.CheckinTime)
                .Select(c => new
                {
                    c.CheckinId,
                    c.StudentId,
                    StudentName = c.Student != null ? c.Student.FullName : null,
                    StudentEmail = c.Student != null ? c.Student.Email : null,
                    c.JobId,
                    JobTitle = c.Job != null ? c.Job.Title : null,
                    c.CheckinTime,
                    c.CheckoutTime,
                    WorkDuration = c.CheckoutTime.HasValue && c.CheckinTime.HasValue
                        ? (c.CheckoutTime.Value - c.CheckinTime.Value).TotalHours
                        : (double?)null,
                    Status = c.CheckoutTime.HasValue ? "Completed" : "In Progress"
                })
                .ToListAsync();

            return Ok(new
            {
                Success = true,
                Message = "Lấy attendance records thành công",
                Data = records
            });
        }

        /// <summary>
        /// GET: api/Attendance/student/{studentId}
        /// Lấy attendance records của một student cụ thể (chỉ trong jobs của provider)
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttendanceByStudent(int studentId)
        {
            var providerId = await GetCurrentProviderIdAsync();
            if (providerId == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid provider token" });
            }

            var records = await _context.CheckinRecords
                .Include(c => c.Student)
                .Include(c => c.Job)
                .Where(c => c.StudentId == studentId 
                    && c.Job != null 
                    && c.Job.ProviderId == providerId)
                .OrderByDescending(c => c.CheckinTime)
                .Select(c => new
                {
                    c.CheckinId,
                    c.StudentId,
                    StudentName = c.Student != null ? c.Student.FullName : null,
                    c.JobId,
                    JobTitle = c.Job != null ? c.Job.Title : null,
                    c.CheckinTime,
                    c.CheckoutTime,
                    WorkDuration = c.CheckoutTime.HasValue && c.CheckinTime.HasValue
                        ? (c.CheckoutTime.Value - c.CheckinTime.Value).TotalHours
                        : (double?)null,
                    Status = c.CheckoutTime.HasValue ? "Completed" : "In Progress"
                })
                .ToListAsync();

            return Ok(new
            {
                Success = true,
                Message = "Lấy attendance records thành công",
                Data = records
            });
        }

        /// <summary>
        /// GET: api/Attendance/{id}
        /// Lấy chi tiết một checkin record
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendanceRecord(int id)
        {
            var providerId = await GetCurrentProviderIdAsync();
            if (providerId == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid provider token" });
            }

            var record = await _context.CheckinRecords
                .Include(c => c.Student)
                .Include(c => c.Job)
                .FirstOrDefaultAsync(c => c.CheckinId == id);

            if (record == null)
            {
                return NotFound(new { Success = false, Message = "Checkin record không tồn tại" });
            }

            // Kiểm tra job có thuộc provider này không
            if (record.Job == null || record.Job.ProviderId != providerId)
            {
                return Forbid();
            }

            var result = new
            {
                record.CheckinId,
                record.StudentId,
                StudentName = record.Student?.FullName,
                StudentEmail = record.Student?.Email,
                StudentPhone = record.Student?.Phone,
                record.JobId,
                JobTitle = record.Job?.Title,
                JobLocation = record.Job?.Location,
                record.CheckinTime,
                record.CheckoutTime,
                WorkDuration = record.CheckoutTime.HasValue && record.CheckinTime.HasValue
                    ? (record.CheckoutTime.Value - record.CheckinTime.Value).TotalHours
                    : (double?)null,
                Status = record.CheckoutTime.HasValue ? "Completed" : "In Progress"
            };

            return Ok(new
            {
                Success = true,
                Message = "Lấy chi tiết attendance record thành công",
                Data = result
            });
        }

        /// <summary>
        /// GET: api/Attendance/statistics
        /// Lấy thống kê attendance của provider
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAttendanceStatistics()
        {
            var providerId = await GetCurrentProviderIdAsync();
            if (providerId == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid provider token" });
            }

            var allRecords = await _context.CheckinRecords
                .Include(c => c.Job)
                .Where(c => c.Job != null && c.Job.ProviderId == providerId)
                .ToListAsync();

            var totalCheckins = allRecords.Count;
            var completedCheckins = allRecords.Count(c => c.CheckoutTime.HasValue);
            var inProgressCheckins = totalCheckins - completedCheckins;

            // Tính tổng giờ làm việc
            var totalWorkHours = allRecords
                .Where(c => c.CheckinTime.HasValue && c.CheckoutTime.HasValue)
                .Sum(c => (c.CheckoutTime!.Value - c.CheckinTime!.Value).TotalHours);

            // Lấy checkins trong tháng này
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlyCheckins = allRecords
                .Count(c => c.CheckinTime.HasValue 
                    && c.CheckinTime.Value.Month == currentMonth 
                    && c.CheckinTime.Value.Year == currentYear);

            // Top students theo số lượng checkin
            var topStudentsData = await _context.CheckinRecords
                .Include(c => c.Student)
                .Include(c => c.Job)
                .Where(c => c.Job != null && c.Job.ProviderId == providerId && c.StudentId != null)
                .ToListAsync(); // ← Load vào memory trước

            var topStudents = topStudentsData
                .GroupBy(c => new { c.StudentId, StudentName = c.Student?.FullName })
                .Select(g => new
                {
                    StudentId = g.Key.StudentId,
                    StudentName = g.Key.StudentName,
                    TotalCheckins = g.Count(),
                    CompletedCheckins = g.Count(c => c.CheckoutTime.HasValue),
                    TotalWorkHours = Math.Round(
                        g.Where(c => c.CheckinTime.HasValue && c.CheckoutTime.HasValue)
                         .Sum(c => (c.CheckoutTime!.Value - c.CheckinTime!.Value).TotalHours),
                        2)
                })
                .OrderByDescending(s => s.TotalCheckins)
                .Take(10)
                .ToList(); // ← Tính toán trong memory

            var statistics = new
            {
                TotalCheckins = totalCheckins,
                CompletedCheckins = completedCheckins,
                InProgressCheckins = inProgressCheckins,
                TotalWorkHours = Math.Round(totalWorkHours, 2),
                MonthlyCheckins = monthlyCheckins,
                AverageWorkHoursPerCheckin = completedCheckins > 0 
                    ? Math.Round(totalWorkHours / completedCheckins, 2) 
                    : 0,
                TopStudents = topStudents
            };

            return Ok(new
            {
                Success = true,
                Message = "Lấy thống kê attendance thành công",
                Data = statistics
            });
        }

        /// <summary>
        /// GET: api/Attendance/summary/daily
        /// Lấy tổng hợp attendance theo ngày (7 ngày gần nhất)
        /// </summary>
        [HttpGet("summary/daily")]
        public async Task<IActionResult> GetDailySummary()
        {
            var providerId = await GetCurrentProviderIdAsync();
            if (providerId == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid provider token" });
            }

            var sevenDaysAgo = DateTime.Now.AddDays(-7).Date;

            var recordsData = await _context.CheckinRecords
                .Include(c => c.Job)
                .Where(c => c.Job != null 
                    && c.Job.ProviderId == providerId
                    && c.CheckinTime.HasValue
                    && c.CheckinTime.Value >= sevenDaysAgo)
                .ToListAsync(); // ← Load vào memory trước

            var dailySummary = recordsData
                .GroupBy(c => c.CheckinTime!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalCheckins = g.Count(),
                    CompletedCheckins = g.Count(c => c.CheckoutTime.HasValue),
                    UniqueStudents = g.Select(c => c.StudentId).Distinct().Count(),
                    TotalWorkHours = Math.Round(
                        g.Where(c => c.CheckinTime.HasValue && c.CheckoutTime.HasValue)
                         .Sum(c => (c.CheckoutTime!.Value - c.CheckinTime!.Value).TotalHours),
                        2)
                })
                .OrderByDescending(s => s.Date)
                .ToList(); // ← Tính toán trong memory

            return Ok(new
            {
                Success = true,
                Message = "Lấy tổng hợp attendance theo ngày thành công",
                Data = dailySummary
            });
        }
    }
}
