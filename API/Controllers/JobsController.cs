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

        // GET: api/Jobs - Public endpoint, không cần đăng nhập
        [HttpGet(Name = "GetAllJobs")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Include(j => j.Provider)
                    .Where(j => j.Status == "Active") // Chỉ hiện job active
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
                        ProviderEmail = j.Provider != null ? j.Provider.Email : null
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
                        ProviderEmail = j.Provider != null ? j.Provider.Email : null
                    })
                    .FirstOrDefaultAsync();

                if (job == null)
                {
                    return NotFound(new { Message = "Không tìm thấy công việc" });
                }

                return Ok(job);
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
                    Status = "Active"
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

        // PUT: api/Jobs/5 - CHỈ ADMIN có thể cập nhật công việc
        [HttpPut("{id}", Name = "UpdateJobByAdmin")]
        [Authorize(Roles = "Admin")]
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
