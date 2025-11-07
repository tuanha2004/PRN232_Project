using Microsoft.AspNetCore.Mvc;
using Project_PRN232.Services;

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

        // Kiểm tra quyền Provider
        private bool CheckProviderAccess()
        {
            if (!_authService.IsLoggedIn())
            {
                return false;
            }

            var userRole = _authService.GetUserRole();
            return userRole == "Provider";
        }

        // GET: ProviderJobs/Index - Dashboard
        public async Task<IActionResult> Index()
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            // Lấy thống kê
            var statistics = await _providerService.GetStatisticsAsync();
            ViewBag.Statistics = statistics;

            return View();
        }

        // GET: ProviderJobs/Jobs - Danh sách jobs
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

        // GET: ProviderJobs/Details/5 - Chi tiết job
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

        // GET: ProviderJobs/Create
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

        // POST: ProviderJobs/Create
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

        // GET: ProviderJobs/Edit/5
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

            return View(job);
        }

        // POST: ProviderJobs/Edit/5
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
                return View(model);
            }
        }

        // POST: ProviderJobs/Delete/5
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

        // GET: ProviderJobs/Applications/5 - Xem applications của job
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

        // POST: ProviderJobs/ApproveApplication/5
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

        // POST: ProviderJobs/RejectApplication/5
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
