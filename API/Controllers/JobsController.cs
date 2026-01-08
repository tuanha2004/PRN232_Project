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
    }
}
