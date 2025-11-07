using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using API.Models;
using API.DTOs.Jobs;
using API.DTOs.Applications;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Provider")] // CHỈ PROVIDER mới được truy cập
    public class ProviderJobsController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;

        public ProviderJobsController(ProjectPrn232Context context)
        {
            _context = context;
        }

        // Helper: Lấy ProviderId từ token
        private async Task<int?> GetCurrentProviderIdAsync()
        {
            var userEmail = User.Identity?.Name;
            var provider = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail && u.Role == "Provider");
            return provider?.UserId;
        }

        // GET: api/ProviderJobs - Lấy tất cả jobs của provider đang đăng nhập
        [HttpGet(Name = "GetMyProviderJobs")]
        public async Task<ActionResult> GetMyJobs()
        {
            try
            {
                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                var jobs = await _context.Jobs
                    .Where(j => j.ProviderId == providerId)
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
                        j.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/ProviderJobs/{id} - Lấy chi tiết 1 job với danh sách students
        [HttpGet("{id}", Name = "GetProviderJobDetails")]
        public async Task<ActionResult> GetJobDetails(int id)
        {
            try
            {
                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                var job = await _context.Jobs
                    .Where(j => j.JobId == id && j.ProviderId == providerId)
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
                        Applications = j.Applications.Select(a => new
                        {
                            a.ApplicationId,
                            a.StudentId,
                            StudentName = a.Student != null ? a.Student.FullName : null,
                            StudentEmail = a.Student != null ? a.Student.Email : null,
                            a.Status,
                            a.AppliedAt
                        }).ToList(),
                        Assignments = j.JobAssignments.Select(ja => new
                        {
                            ja.AssignmentId,
                            ja.StudentId,
                            StudentName = ja.Student != null ? ja.Student.FullName : null,
                            StudentEmail = ja.Student != null ? ja.Student.Email : null,
                            ja.AssignedAt,
                            ja.Status
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (job == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy job hoặc bạn không có quyền truy cập"
                    });
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // POST: api/ProviderJobs - Tạo job mới
        [HttpPost(Name = "CreateProviderJob")]
        public async Task<ActionResult<object>> CreateJob([FromBody] CreateJobRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                // Validate dates
                if (request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    if (request.EndDate < request.StartDate)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Ngày kết thúc phải sau ngày bắt đầu"
                        });
                    }
                }

                var job = new Job
                {
                    Title = request.Title,
                    Description = request.Description,
                    Location = request.Location,
                    Salary = request.Salary,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ProviderId = providerId.Value,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "Active" // Mặc định là Active
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Tạo job thành công",
                    Data = new
                    {
                        job.JobId,
                        job.Title,
                        job.Description,
                        job.Location,
                        job.Salary,
                        job.StartDate,
                        job.EndDate,
                        job.Status,
                        job.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // PUT: api/ProviderJobs/{id} - Cập nhật job
        [HttpPut("{id}", Name = "UpdateProviderJob")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.JobId == id && j.ProviderId == providerId);

                if (job == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy job hoặc bạn không có quyền chỉnh sửa"
                    });
                }

                // Cập nhật thông tin
                job.Title = request.Title ?? job.Title;
                job.Description = request.Description ?? job.Description;
                job.Location = request.Location ?? job.Location;
                job.Salary = request.Salary ?? job.Salary;
                job.StartDate = request.StartDate ?? job.StartDate;
                job.EndDate = request.EndDate ?? job.EndDate;
                job.Status = request.Status ?? job.Status;
                job.UpdatedAt = DateTime.Now;

                // Validate dates nếu có thay đổi
                if (job.StartDate.HasValue && job.EndDate.HasValue)
                {
                    if (job.EndDate < job.StartDate)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Ngày kết thúc phải sau ngày bắt đầu"
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Cập nhật job thành công",
                    Data = new
                    {
                        job.JobId,
                        job.Title,
                        job.Description,
                        job.Location,
                        job.Salary,
                        job.StartDate,
                        job.EndDate,
                        job.Status,
                        job.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // DELETE: api/ProviderJobs/{id} - Xóa job
        [HttpDelete("{id}", Name = "DeleteProviderJob")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                var job = await _context.Jobs
                    .Include(j => j.Applications)
                    .Include(j => j.JobAssignments)
                    .FirstOrDefaultAsync(j => j.JobId == id && j.ProviderId == providerId);

                if (job == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy job hoặc bạn không có quyền xóa"
                    });
                }

                // Kiểm tra xem có students đang làm việc không
                if (job.JobAssignments.Any())
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không thể xóa job đang có students được phân công"
                    });
                }

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Xóa job thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/ProviderJobs/{jobId}/applications - Lấy danh sách applications của 1 job
        [HttpGet("{jobId}/applications", Name = "GetProviderJobApplications")]
        public async Task<ActionResult> GetJobApplications(int jobId)
        {
            try
            {
                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                // Kiểm tra job có thuộc về provider không
                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.JobId == jobId && j.ProviderId == providerId);

                if (job == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy job hoặc bạn không có quyền truy cập"
                    });
                }

                var applications = await _context.Applications
                    .Where(a => a.JobId == jobId)
                    .Select(a => new
                    {
                        a.ApplicationId,
                        a.JobId,
                        a.StudentId,
                        StudentName = a.Student != null ? a.Student.FullName : null,
                        StudentEmail = a.Student != null ? a.Student.Email : null,
                        StudentPhone = a.Student != null ? a.Student.Phone : null,
                        a.Status,
                        a.AppliedAt,
                        a.WorkType,
                        a.StudentYear,
                        a.Phone,
                        a.Notes
                    })
                    .ToListAsync();

                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // PUT: api/ProviderJobs/applications/{applicationId}/status - Accept/Reject application
        [HttpPut("applications/{applicationId}/status", Name = "UpdateProviderApplicationStatus")]
        public async Task<IActionResult> UpdateApplicationStatus(
            int applicationId, 
            [FromBody] UpdateApplicationStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                var application = await _context.Applications
                    .Include(a => a.Job)
                    .Include(a => a.Student)
                    .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

                if (application == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy application"
                    });
                }

                // Kiểm tra job có thuộc về provider không
                if (application.Job?.ProviderId != providerId)
                {
                    return Forbid();
                }

                // Cập nhật status
                var oldStatus = application.Status;
                application.Status = request.Status;

                // Nếu approve thì tạo JobAssignment
                if (request.Status == "Approved" && oldStatus != "Approved")
                {
                    // Kiểm tra xem đã có assignment chưa
                    var existingAssignment = await _context.JobAssignments
                        .FirstOrDefaultAsync(ja => ja.StudentId == application.StudentId 
                                                && ja.JobId == application.JobId);

                    if (existingAssignment == null)
                    {
                        var assignment = new JobAssignment
                        {
                            StudentId = application.StudentId,
                            JobId = application.JobId,
                            AssignedAt = DateTime.Now,
                            Status = "Active"
                        };
                        _context.JobAssignments.Add(assignment);
                    }
                }

                // Nếu reject thì xóa JobAssignment (nếu có)
                if (request.Status == "Rejected" && oldStatus == "Approved")
                {
                    var assignment = await _context.JobAssignments
                        .FirstOrDefaultAsync(ja => ja.StudentId == application.StudentId 
                                                && ja.JobId == application.JobId);
                    if (assignment != null)
                    {
                        _context.JobAssignments.Remove(assignment);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã {(request.Status == "Approved" ? "chấp nhận" : request.Status == "Rejected" ? "từ chối" : "cập nhật")} application",
                    Data = new
                    {
                        application.ApplicationId,
                        application.StudentId,
                        StudentName = application.Student?.FullName,
                        application.Status,
                        JobTitle = application.Job?.Title
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // GET: api/ProviderJobs/statistics - Thống kê jobs của provider
        [HttpGet("statistics", Name = "GetProviderStatistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            try
            {
                var providerId = await GetCurrentProviderIdAsync();
                if (providerId == null)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin Provider"
                    });
                }

                var totalJobs = await _context.Jobs.CountAsync(j => j.ProviderId == providerId);
                var activeJobs = await _context.Jobs.CountAsync(j => j.ProviderId == providerId && j.Status == "Active");
                var closedJobs = await _context.Jobs.CountAsync(j => j.ProviderId == providerId && j.Status == "Closed");
                
                var totalApplications = await _context.Applications
                    .Where(a => a.Job!.ProviderId == providerId)
                    .CountAsync();
                
                var pendingApplications = await _context.Applications
                    .Where(a => a.Job!.ProviderId == providerId && a.Status == "Pending")
                    .CountAsync();

                var totalAssignedStudents = await _context.JobAssignments
                    .Where(ja => ja.Job!.ProviderId == providerId)
                    .CountAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Lấy thống kê thành công",
                    Data = new
                    {
                        TotalJobs = totalJobs,
                        ActiveJobs = activeJobs,
                        ClosedJobs = closedJobs,
                        TotalApplications = totalApplications,
                        PendingApplications = pendingApplications,
                        TotalAssignedStudents = totalAssignedStudents
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}
