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

            return user.UserId;
        }


        [HttpGet]
        public async Task<IActionResult> GetAttendanceRecords()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

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


        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetAttendanceByJob(int jobId)
        {
            var providerId = await GetCurrentProviderIdAsync();
            if (providerId == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid provider token" });
            }

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

            var totalWorkHours = allRecords
                .Where(c => c.CheckinTime.HasValue && c.CheckoutTime.HasValue)
                .Sum(c => (c.CheckoutTime!.Value - c.CheckinTime!.Value).TotalHours);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlyCheckins = allRecords
                .Count(c => c.CheckinTime.HasValue 
                    && c.CheckinTime.Value.Month == currentMonth 
                    && c.CheckinTime.Value.Year == currentYear);

            var topStudentsData = await _context.CheckinRecords
                .Include(c => c.Student)
                .Include(c => c.Job)
                .Where(c => c.Job != null && c.Job.ProviderId == providerId && c.StudentId != null)
                .ToListAsync(); 

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
                .ToList(); 

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
                .ToListAsync(); 

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
                .ToList(); 

            return Ok(new
            {
                Success = true,
                Message = "Lấy tổng hợp attendance theo ngày thành công",
                Data = dailySummary
            });
        }
    }
}
