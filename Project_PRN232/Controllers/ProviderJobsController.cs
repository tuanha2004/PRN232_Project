using Microsoft.AspNetCore.Mvc;
using Project_PRN232.Services;
using Project_PRN232.Models.DTOs;

namespace Project_PRN232.Controllers
{
    public class ProviderJobsController : Controller
    {
        private readonly ProviderService _providerService;
        private readonly AuthService _authService;
        private readonly ILogger<ProviderJobsController> _logger;

        public ProviderJobsController(ProviderService providerService, AuthService authService, ILogger<ProviderJobsController> logger)
        {
            _providerService = providerService;
            _authService = authService;
            _logger = logger;
        }

        private bool CheckProviderAccess()
        {
            if (!_authService.IsLoggedIn())
            {
                return false;
            }

            var userRole = _authService.GetUserRole();
            return userRole == "Provider";
        }

        public async Task<IActionResult> Index()
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var statistics = await _providerService.GetStatisticsAsync();
            ViewBag.Statistics = statistics;

            return View();
        }

        public async Task<IActionResult> Jobs()
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var jobs = await _providerService.GetMyJobsAsync();
            return View(jobs ?? new List<JobDto>());
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var job = await _providerService.GetJobDetailsAsync(id);
            if (job == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy job!";
                return RedirectToAction(nameof(Jobs));
            }

            return View(job);
        }

        public IActionResult Create()
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateJobDto model)
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _providerService.CreateJobAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Jobs));
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var job = await _providerService.GetJobDetailsAsync(id);
            if (job == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy job!";
                return RedirectToAction(nameof(Jobs));
            }

            // Map JobDto to UpdateJobDto for editing
            var updateModel = new UpdateJobDto
            {
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                Salary = job.Salary,
                StartDate = job.StartDate,
                EndDate = job.EndDate,
                Status = job.Status
            };

            // Store readonly fields in ViewBag
            ViewBag.JobId = job.JobId;
            ViewBag.CreatedAt = job.CreatedAt;
            ViewBag.UpdatedAt = job.UpdatedAt;

            return View(updateModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateJobDto model)
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Validation failed: " + string.Join(", ", errors);
                
                // Restore ViewBag for re-rendering
                var job = await _providerService.GetJobDetailsAsync(id);
                if (job != null)
                {
                    ViewBag.JobId = job.JobId;
                    ViewBag.CreatedAt = job.CreatedAt;
                    ViewBag.UpdatedAt = job.UpdatedAt;
                }
                
                return View(model);
            }

            var result = await _providerService.UpdateJobAsync(id, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Jobs));
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                
                // Restore ViewBag for re-rendering
                var job = await _providerService.GetJobDetailsAsync(id);
                if (job != null)
                {
                    ViewBag.JobId = job.JobId;
                    ViewBag.CreatedAt = job.CreatedAt;
                    ViewBag.UpdatedAt = job.UpdatedAt;
                }
                
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!CheckProviderAccess())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            var result = await _providerService.DeleteJobAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> Applications(int id)
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var job = await _providerService.GetJobDetailsAsync(id);
            if (job == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy job!";
                return RedirectToAction(nameof(Jobs));
            }

            ViewBag.Job = job;

            var applications = await _providerService.GetJobApplicationsAsync(id);
            return View(applications ?? new List<ApplicationDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int id)
        {
            if (!CheckProviderAccess())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            var result = await _providerService.UpdateApplicationStatusAsync(id, "Approved");
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int id)
        {
            if (!CheckProviderAccess())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            var result = await _providerService.UpdateApplicationStatusAsync(id, "Rejected");
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
