using Microsoft.AspNetCore.Mvc;
using Project_PRN232.Models;
using Project_PRN232.Services;
using System.Diagnostics;

namespace Project_PRN232.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly JobService _jobService;

		public HomeController(ILogger<HomeController> logger, JobService jobService)
		{
			_logger = logger;
			_jobService = jobService;
		}

		public async Task<IActionResult> Index()
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
			return View(job);
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
