using Microsoft.AspNetCore.Mvc;
using Project_PRN232.Services;

namespace Project_PRN232.Controllers
{
    public class CheckinController : Controller
    {
        private readonly CheckinService _checkinService;
        private readonly AuthService _authService;

        public CheckinController(CheckinService checkinService, AuthService authService)
        {
            _checkinService = checkinService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            if (!_authService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Student")
            {
                TempData["ErrorMessage"] = "Chỉ sinh viên mới có thể truy cập trang này";
                return RedirectToAction("Index", "Home");
            }

            var records = await _checkinService.GetMyCheckinRecordsAsync();
            var currentCheckin = await _checkinService.GetCurrentCheckinAsync();

            ViewBag.CurrentCheckin = currentCheckin;
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");

            return View(records);
        }

        [HttpPost]
        public async Task<IActionResult> Checkin([FromBody] CheckinRequestModel request)
        {
            if (!_authService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (request == null || request.JobId <= 0)
            {
                return Json(new { success = false, message = $"JobId không hợp lệ: {request?.JobId ?? 0}" });
            }

            var result = await _checkinService.CheckinAsync(request.JobId);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestModel request)
        {
            if (!_authService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (request == null || request.CheckinId <= 0)
            {
                return Json(new { success = false, message = $"CheckinId không hợp lệ: {request?.CheckinId ?? 0}" });
            }

            var result = await _checkinService.CheckoutAsync(request.CheckinId);
            return Json(new { success = result.Success, message = result.Message });
        }
    }

    public class CheckinRequestModel
    {
        public int JobId { get; set; }
    }

    public class CheckoutRequestModel
    {
        public int CheckinId { get; set; }
    }
}
