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

        [HttpGet(Name = "GetAllJobs")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            try
            {

                await UpdateAllExpiredJobs();

                var jobs = await _context.Jobs
                    .Include(j => j.Provider)
                    .Where(j => j.Status == "Open" || j.Status == "Closed")
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
                        CompanyName = j.Provider != null ? j.Provider.FullName : null
                    })
                    .ToListAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

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
                    CompanyName = job.Provider != null ? job.Provider.FullName : null
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

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
                    Status = "Open"
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
