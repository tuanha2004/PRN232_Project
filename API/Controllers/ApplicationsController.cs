using API.DTOs.Applications;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;
        private readonly IEmailService _emailService;

        public ApplicationsController(ProjectPrn232Context context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Application>>> GetAllApplications()
        {
            try
            {
                var applications = await _context.Applications
                    .Include(a => a.Job)
                    .Include(a => a.Student)
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<Application>>> GetMyApplications()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return NotFound(new { Message = "Không tìm thấy user" });
                }

                var applications = await _context.Applications
                    .Include(a => a.Job)
                        .ThenInclude(j => j!.Provider)
                    .Where(a => a.StudentId == user.UserId)
                    .Select(a => new
                    {
                        a.ApplicationId,
                        a.StudentId,
                        a.JobId,
                        a.AppliedAt,
                        a.Status,
                        a.Phone,
                        a.StudentYear,
                        a.WorkType,
                        a.Notes,
                        Job = a.Job != null ? new
                        {
                            a.Job.JobId,
                            a.Job.Title,
                            a.Job.Description,
                            a.Job.Location,
                            a.Job.Salary,
                            a.Job.StartDate,
                            a.Job.EndDate,
                            a.Job.Status,
                            a.Job.CreatedAt,
                            ProviderName = a.Job.Provider != null ? a.Job.Provider.FullName : null
                        } : null
                    })
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Application>> GetApplication(int id)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.Job)
                    .Include(a => a.Student)
                    .FirstOrDefaultAsync(a => a.ApplicationId == id);

                if (application == null)
                {
                    return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển" });
                }

                var userEmail = User.Identity?.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && application.StudentId != currentUser?.UserId)
                {
                    return Forbid();
                }

                return Ok(application);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Student,Admin")]
        public async Task<ActionResult<Application>> CreateApplication([FromBody] CreateApplicationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return NotFound(new { Message = "Không tìm thấy user" });
                }

                var job = await _context.Jobs
                    .Include(j => j.Provider)
                    .FirstOrDefaultAsync(j => j.JobId == request.JobId);
                    
                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                var existingApplication = await _context.Applications
                    .FirstOrDefaultAsync(a => a.StudentId == user.UserId && a.JobId == request.JobId);
                
                if (existingApplication != null)
                {
                    return BadRequest(new { Message = "Bạn đã nộp đơn cho công việc này rồi" });
                }

                var application = new Application
                {
                    JobId = request.JobId,
                    StudentId = user.UserId, // Tự động lấy từ token
                    Phone = request.Phone,
                    StudentYear = request.StudentYear,
                    WorkType = request.WorkType,
                    Notes = request.Notes,
                    AppliedAt = DateTime.Now,
                    Status = "Pending"
                };

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                if (job.Provider != null && !string.IsNullOrEmpty(job.Provider.Email))
                {
                    await _emailService.SendNewApplicationEmailAsync(
                        job.Provider.Email,
                        job.Provider.FullName ?? "Nhà tuyển dụng",
                        job.Title ?? "Công việc",
                        user.FullName ?? "Ứng viên",
                        request.Phone,
                        request.StudentYear,
                        request.WorkType
                    );
                }

                return Ok(new 
                { 
                    Message = "Đơn ứng tuyển đã được gửi thành công!",
                    ApplicationId = application.ApplicationId,
                    Status = application.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateApplicationStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var application = await _context.Applications
                    .Include(a => a.Student)
                    .Include(a => a.Job)
                        .ThenInclude(j => j!.Provider)
                    .FirstOrDefaultAsync(a => a.ApplicationId == id);
                
                if (application == null)
                {
                    return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển" });
                }

                var oldStatus = application.Status;
                application.Status = request.Status;
                await _context.SaveChangesAsync();

                if (oldStatus != request.Status && (request.Status == "Accepted" || request.Status == "Approved" || request.Status == "Rejected"))
                {
                    if (application.Student != null && !string.IsNullOrEmpty(application.Student.Email))
                    {
                        var emailStatus = request.Status == "Approved" ? "Accepted" : request.Status;
                        
                        await _emailService.SendApplicationStatusEmailAsync(
                            application.Student.Email,
                            application.Student.FullName ?? "Bạn",
                            application.Job?.Title ?? "Công việc",
                            emailStatus,
                            application.Job?.Provider?.FullName ?? "Provider"
                        );
                    }
                }

                return Ok(new { Message = "Cập nhật trạng thái thành công", Application = application });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            try
            {
                var application = await _context.Applications.FindAsync(id);
                
                if (application == null)
                {
                    return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển" });
                }

                var userEmail = User.Identity?.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && application.StudentId != currentUser?.UserId)
                {
                    return Forbid();
                }

                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Xóa đơn ứng tuyển thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
