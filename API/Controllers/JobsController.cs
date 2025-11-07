using API.DTOs.Jobs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;

        public JobsController(ProjectPrn232Context context)
        {
            _context = context;
        }

        // Helper method: Kiểm tra và cập nhật status job tự động
        private async Task UpdateJobStatusBasedOnDate(Job job)
        {
            if (job.EndDate.HasValue && job.Status == "Open")
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (job.EndDate.Value < today)
                {
                    job.Status = "Closed";
                    job.UpdatedAt = DateTime.Now;
                }
            }
        }

        // Helper method: Cập nhật tất cả jobs đã quá hạn
        private async Task UpdateAllExpiredJobs()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var expiredJobs = await _context.Jobs
                .Where(j => j.Status == "Open" && j.EndDate.HasValue && j.EndDate.Value < today)
                .ToListAsync();

            foreach (var job in expiredJobs)
            {
                job.Status = "Closed";
                job.UpdatedAt = DateTime.Now;
            }

            if (expiredJobs.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        // GET: api/Jobs - Public endpoint, không cần đăng nhập
        [HttpGet(Name = "GetAllJobs")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            try
            {
                // Tự động cập nhật các job đã hết hạn
                await UpdateAllExpiredJobs();

                var jobs = await _context.Jobs
                    .Include(j => j.Provider)
                    .Where(j => j.Status == "Open" || j.Status == "Closed") // Chỉ hiển thị Open và Closed
                    .Select(j => new
                    {
                        j.JobId,
                        j.Title,
                        j.Description,
                        j.Location,
                        j.Salary,
                        j.StartDate,
                        j.EndDate,
                        j.Status,
                        j.CreatedAt,
                        j.UpdatedAt,
                        ProviderName = j.Provider != null ? j.Provider.FullName : null,
                        ProviderEmail = j.Provider != null ? j.Provider.Email : null,
                        CompanyName = j.Provider != null ? j.Provider.FullName : null // Sử dụng FullName làm CompanyName
                    })
                    .ToListAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: api/Jobs/5 - Public endpoint, không cần đăng nhập
        [HttpGet("{id}", Name = "GetJobById")]
        [AllowAnonymous]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Provider)
                    .Where(j => j.JobId == id)
                    .FirstOrDefaultAsync();

                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                // Kiểm tra và cập nhật status nếu cần
                await UpdateJobStatusBasedOnDate(job);
                if (_context.Entry(job).State == EntityState.Modified)
                {
                    await _context.SaveChangesAsync();
                }

                var result = new
                {
                    job.JobId,
                    job.Title,
                    job.Description,
                    job.Location,
                    job.Salary,
                    job.StartDate,
                    job.EndDate,
                    job.Status,
                    job.CreatedAt,
                    job.UpdatedAt,
                    ProviderName = job.Provider != null ? job.Provider.FullName : null,
                    ProviderEmail = job.Provider != null ? job.Provider.Email : null,
                    CompanyName = job.Provider != null ? job.Provider.FullName : null // Sử dụng FullName làm CompanyName
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: api/Jobs - CHỈ ADMIN có thể tạo công việc mới
        [HttpPost(Name = "CreateJobByAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Job>> CreateJob([FromBody] CreateJobRequest request)
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

                // Map DTO to Entity
                var job = new Job
                {
                    Title = request.Title,
                    Description = request.Description,
                    Location = request.Location,
                    Salary = request.Salary,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ProviderId = request.ProviderId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "Open" // Mặc định là Open khi tạo mới
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetJobById", new { id = job.JobId }, job);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: api/Jobs/5 - CHỈ ADMIN hoặc PROVIDER (chủ job) có thể cập nhật
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobRequest request)
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

                var job = await _context.Jobs.FindAsync(id);

                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                // Kiểm tra quyền: Provider chỉ có thể update job của mình
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (userRole == "Provider")
                {
                    var provider = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                    if (provider == null || job.ProviderId != provider.UserId)
                    {
                        return Forbid(); // Provider chỉ có thể update job của họ
                    }
                }

                // Cập nhật các fields
                job.Title = request.Title ?? job.Title;
                job.Description = request.Description ?? job.Description;
                job.Location = request.Location ?? job.Location;
                job.Salary = request.Salary ?? job.Salary;
                job.StartDate = request.StartDate ?? job.StartDate;
                job.EndDate = request.EndDate ?? job.EndDate;
                job.Status = request.Status ?? job.Status;
                job.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Cập nhật thành công", Job = job });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: api/Jobs/5/close - Provider đóng job của họ
        [HttpPut("{id}/close")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<IActionResult> CloseJob(int id)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                
                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                // Kiểm tra quyền: Provider chỉ có thể đóng job của mình
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (userRole == "Provider")
                {
                    var provider = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                    if (provider == null || job.ProviderId != provider.UserId)
                    {
                        return Forbid();
                    }
                }

                if (job.Status == "Closed")
                {
                    return BadRequest(new { Message = "Công việc này đã được đóng" });
                }

                job.Status = "Closed";
                job.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Đã đóng công việc thành công", Job = job });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: api/Jobs/5/reopen - Provider mở lại job của họ (nếu chưa quá hạn)
        [HttpPut("{id}/reopen")]
        [Authorize(Roles = "Admin,Provider")]
        public async Task<IActionResult> ReopenJob(int id)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                
                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                // Kiểm tra quyền
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (userRole == "Provider")
                {
                    var provider = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                    if (provider == null || job.ProviderId != provider.UserId)
                    {
                        return Forbid();
                    }
                }

                if (job.Status == "Open")
                {
                    return BadRequest(new { Message = "Công việc này đang mở" });
                }

                // Kiểm tra xem job đã quá hạn chưa
                if (job.EndDate.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    if (job.EndDate.Value < today)
                    {
                        return BadRequest(new { Message = "Không thể mở lại công việc đã quá hạn. Vui lòng cập nhật ngày kết thúc." });
                    }
                }

                job.Status = "Open";
                job.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Đã mở lại công việc thành công", Job = job });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: api/Jobs/5 - CHỈ ADMIN có thể xóa công việc
        [HttpDelete("{id}", Name = "DeleteJobByAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                // Soft delete: Chỉ đổi status thành Inactive
                job.Status = "Inactive";
                job.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Xóa công việc thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
