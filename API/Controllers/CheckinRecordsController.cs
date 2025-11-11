using API.DTOs.CheckinRecords;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckinRecordsController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;

        public CheckinRecordsController(ProjectPrn232Context context)
        {
            _context = context;
        }

        [HttpGet("my-records")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<CheckinRecordResponse>>> GetMyCheckinRecords()
        {
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            var records = await _context.CheckinRecords
                .Include(c => c.Job)
                .Include(c => c.Student)
                .Where(c => c.StudentId == user.UserId)
                .OrderByDescending(c => c.CheckinTime)
                .Select(c => new CheckinRecordResponse
                {
                    CheckinId = c.CheckinId,
                    StudentId = c.StudentId ?? 0,
                    StudentName = c.Student!.FullName ?? "",
                    JobId = c.JobId ?? 0,
                    JobTitle = c.Job!.Title ?? "",
                    CheckinTime = c.CheckinTime,
                    CheckoutTime = c.CheckoutTime,
                    Status = c.CheckoutTime == null ? "Checked In" : "Checked Out",
                    WorkedHours = c.CheckoutTime != null && c.CheckinTime != null 
                        ? Math.Round((c.CheckoutTime.Value - c.CheckinTime.Value).TotalHours, 2) 
                        : (double?)null
                })
                .ToListAsync();

            return Ok(records);
        }

        [HttpGet("current")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<CheckinRecordResponse>>> GetCurrentCheckin()
        {
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            var currentCheckins = await _context.CheckinRecords
                .Include(c => c.Job)
                .Include(c => c.Student)
                .Where(c => c.StudentId == user.UserId && c.CheckoutTime == null)
                .OrderByDescending(c => c.CheckinTime)
                .Select(c => new CheckinRecordResponse
                {
                    CheckinId = c.CheckinId,
                    StudentId = c.StudentId ?? 0,
                    StudentName = c.Student!.FullName ?? "",
                    JobId = c.JobId ?? 0,
                    JobTitle = c.Job!.Title ?? "",
                    CheckinTime = c.CheckinTime,
                    CheckoutTime = c.CheckoutTime,
                    Status = "Checked In",
                    WorkedHours = null
                })
                .ToListAsync();

            return Ok(currentCheckins);
        }

        [HttpPost("checkin")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<CheckinRecordResponse>> Checkin([FromBody] CheckinRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            if (request.JobId <= 0)
            {
                return BadRequest(new { message = "Công việc không hợp lệ" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            var job = await _context.Jobs.FindAsync(request.JobId);
            if (job == null)
            {
                return NotFound(new { message = "Công việc không tồn tại" });
            }

            var today = DateTime.Today;

            var checkinToday = await _context.CheckinRecords
                .Where(c => c.StudentId == user.UserId 
                    && c.JobId == request.JobId 
                    && c.CheckinTime.HasValue
                    && c.CheckinTime.Value.Date == today)
                .FirstOrDefaultAsync();

            if (checkinToday != null)
            {
                return BadRequest(new { message = "Bạn đã check-in cho công việc này hôm nay rồi. Mỗi ngày chỉ được check-in 1 lần cho mỗi công việc." });
            }

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.StudentId == user.UserId 
                    && a.JobId == request.JobId 
                    && a.Status == "Approved");

            if (application == null)
            {

                var anyApplication = await _context.Applications
                    .FirstOrDefaultAsync(a => a.StudentId == user.UserId && a.JobId == request.JobId);
                
                if (anyApplication == null)
                {
                    return BadRequest(new { message = "Bạn chưa nộp đơn ứng tuyển cho công việc này" });
                }
                else
                {
                    return BadRequest(new { message = $"Đơn ứng tuyển của bạn đang ở trạng thái: {anyApplication.Status}. Chỉ những đơn được duyệt mới có thể check-in." });
                }
            }

            var checkinRecord = new CheckinRecord
            {
                StudentId = user.UserId,
                JobId = request.JobId,
                CheckinTime = DateTime.Now,
                CheckoutTime = null
            };

            _context.CheckinRecords.Add(checkinRecord);
            await _context.SaveChangesAsync();

            var response = new CheckinRecordResponse
            {
                CheckinId = checkinRecord.CheckinId,
                StudentId = user.UserId,
                StudentName = user.FullName ?? "",
                JobId = request.JobId,
                JobTitle = job.Title ?? "",
                CheckinTime = checkinRecord.CheckinTime,
                CheckoutTime = null,
                Status = "Checked In",
                WorkedHours = null
            };

            return CreatedAtAction(nameof(GetCurrentCheckin), response);
        }

        [HttpPost("checkout")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<CheckinRecordResponse>> Checkout([FromBody] CheckoutRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại" });
            }

            var checkinRecord = await _context.CheckinRecords
                .Include(c => c.Job)
                .Include(c => c.Student)
                .FirstOrDefaultAsync(c => c.CheckinId == request.CheckinId && c.StudentId == user.UserId);

            if (checkinRecord == null)
            {
                return NotFound(new { message = "Không tìm thấy bản ghi check-in" });
            }

            if (checkinRecord.CheckoutTime != null)
            {
                return BadRequest(new { message = "Bạn đã check-out rồi" });
            }

            checkinRecord.CheckoutTime = DateTime.Now;
            await _context.SaveChangesAsync();

            var workedHours = checkinRecord.CheckoutTime != null && checkinRecord.CheckinTime != null
                ? Math.Round((checkinRecord.CheckoutTime.Value - checkinRecord.CheckinTime.Value).TotalHours, 2)
                : (double?)null;

            var response = new CheckinRecordResponse
            {
                CheckinId = checkinRecord.CheckinId,
                StudentId = checkinRecord.StudentId ?? 0,
                StudentName = checkinRecord.Student!.FullName ?? "",
                JobId = checkinRecord.JobId ?? 0,
                JobTitle = checkinRecord.Job!.Title ?? "",
                CheckinTime = checkinRecord.CheckinTime,
                CheckoutTime = checkinRecord.CheckoutTime,
                Status = "Checked Out",
                WorkedHours = workedHours
            };

            return Ok(response);
        }

        [HttpGet("job/{jobId}/records")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<ActionResult<IEnumerable<CheckinRecordResponse>>> GetJobCheckinRecords(int jobId)
        {
            var records = await _context.CheckinRecords
                .Include(c => c.Job)
                .Include(c => c.Student)
                .Where(c => c.JobId == jobId)
                .OrderByDescending(c => c.CheckinTime)
                .Select(c => new CheckinRecordResponse
                {
                    CheckinId = c.CheckinId,
                    StudentId = c.StudentId ?? 0,
                    StudentName = c.Student!.FullName ?? "",
                    JobId = c.JobId ?? 0,
                    JobTitle = c.Job!.Title ?? "",
                    CheckinTime = c.CheckinTime,
                    CheckoutTime = c.CheckoutTime,
                    Status = c.CheckoutTime == null ? "Checked In" : "Checked Out",
                    WorkedHours = c.CheckoutTime != null && c.CheckinTime != null
                        ? Math.Round((c.CheckoutTime.Value - c.CheckinTime.Value).TotalHours, 2)
                        : (double?)null
                })
                .ToListAsync();

            return Ok(records);
        }
    }
}
