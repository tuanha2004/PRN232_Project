using Microsoft.AspNetCore.Mvc;
using Project_PRN232.DTOs;
using Project_PRN232.Services;
using System.Diagnostics;

namespace Project_PRN232.Controllers
{
	public class HomeController : Controller
	{
	private readonly ILogger<HomeController> _logger;
	private readonly JobService _jobService;
	private readonly ApplicationService _applicationService;
	private readonly CheckinService _checkinService;

	public HomeController(ILogger<HomeController> logger, JobService jobService, ApplicationService applicationService, CheckinService checkinService)
	{
		_logger = logger;
		_jobService = jobService;
		_applicationService = applicationService;
		_checkinService = checkinService;
	}		public async Task<IActionResult> Index()
		{
			var jobs = await _jobService.GetAllJobsAsync();
			return View(jobs);
		}

	public async Task<IActionResult> JobDetail(int id)
	{
		var job = await _jobService.GetJobByIdAsync(id);
		if (job == null)
		{
			TempData["ErrorMessage"] = "Không tìm thấy công việc này!";
			return RedirectToAction("Index");
		}

		var userRole = HttpContext.Session.GetString("UserRole");
		if (userRole == "Student")
		{
			var applications = await _applicationService.GetMyApplicationsAsync();
			var approvedApp = applications?.FirstOrDefault(a => a.JobId == id && a.Status == "Approved");
			ViewBag.IsApproved = approvedApp != null;

			var currentCheckin = await _checkinService.GetCurrentCheckinAsync();
			ViewBag.CurrentCheckin = currentCheckin;
			ViewBag.IsCheckedIn = currentCheckin != null && currentCheckin.JobId == id;
		}
		else
		{
			ViewBag.IsApproved = false;
			ViewBag.IsCheckedIn = false;
		}

		return View(job);
	}		[HttpPost]
		public async Task<IActionResult> ApplyJob([FromBody] CreateApplicationRequest request)
		{
			try
			{
				if (request == null)
				{
					return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
				}

				var token = HttpContext.Session.GetString("JwtToken");
				if (string.IsNullOrEmpty(token))
				{
					return Json(new { success = false, message = "Bạn cần đăng nhập để ứng tuyển" });
				}

				var userRole = HttpContext.Session.GetString("UserRole");
				if (userRole != "Student" && userRole != "Admin")
				{
					return Json(new { success = false, message = "Chỉ sinh viên (Student) mới có thể ứng tuyển. Tài khoản của bạn là: " + userRole });
				}

				var result = await _applicationService.CreateApplicationAsync(request);
				
				return Json(new { success = result.Success, message = result.Message });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Lỗi: " + ex.Message });
			}
		}

		public async Task<IActionResult> MyApplications()
		{
			var applications = await _applicationService.GetMyApplicationsAsync();
			return View(applications);
		}

		public IActionResult TestApi()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
