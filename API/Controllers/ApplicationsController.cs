using API.DTOs.Applications;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập
    public class ApplicationsController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;

        public ApplicationsController(ProjectPrn232Context context)
        {
            _context = context;
        }

        // GET: api/Applications - CHỈ ADMIN xem tất cả đơn ứng tuyển
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

        // GET: api/Applications/my - User xem đơn ứng tuyển của chính mình
        [HttpGet("my")]
        [Authorize(Roles = "User,Admin")]
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
                    .Where(a => a.StudentId == user.UserId)
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: api/Applications/5
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

                // User chỉ xem được đơn của mình, Admin xem được tất cả
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

        // POST: api/Applications - User nộp đơn ứng tuyển
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
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

                // Kiểm tra job có tồn tại không
                var job = await _context.Jobs.FindAsync(request.JobId);
                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                // Kiểm tra user đã nộp đơn cho job này chưa
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

                return CreatedAtAction(nameof(GetApplication), 
                    new { id = application.ApplicationId }, 
                    application);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: api/Applications/5/status - CHỈ ADMIN cập nhật trạng thái đơn
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
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

                var application = await _context.Applications.FindAsync(id);
                
                if (application == null)
                {
                    return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển" });
                }

                application.Status = request.Status;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Cập nhật trạng thái thành công", Application = application });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: api/Applications/5 - User xóa đơn của mình, Admin xóa mọi đơn
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

                // Kiểm tra quyền
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
