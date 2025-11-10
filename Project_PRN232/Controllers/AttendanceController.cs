using Microsoft.AspNetCore.Mvc;
using Project_PRN232.DTOs;
using Project_PRN232.Services;

namespace Project_PRN232.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AttendanceService _attendanceService;
        private readonly AuthService _authService;

        public AttendanceController(AttendanceService attendanceService, AuthService authService)
        {
            _attendanceService = attendanceService;
            _authService = authService;
        }

        private bool CheckProviderAccess()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "Provider")
            {
                return false;
            }
            return true;
        }

        public async Task<IActionResult> Index()
        {
            if (!CheckProviderAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var (success, message, data) = await _attendanceService.GetStatisticsAsync();
            if (!success)
            {
                TempData["ErrorMessage"] = message;
            }

            ViewBag.Statistics = data;
            return View();
        }

        public async Task<IActionResult> Records()
        {
            if (!CheckProviderAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var (success, message, data) = await _attendanceService.GetAllAttendanceRecordsAsync();
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return View(new List<AttendanceRecordDto>());
            }

            return View(data ?? new List<AttendanceRecordDto>());
        }

        public async Task<IActionResult> ByJob(int jobId)
        {
            if (!CheckProviderAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var (success, message, data) = await _attendanceService.GetAttendanceByJobAsync(jobId);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return View(new List<AttendanceRecordDto>());
            }

            ViewBag.JobId = jobId;
            return View(data ?? new List<AttendanceRecordDto>());
        }

        public async Task<IActionResult> ByStudent(int studentId)
        {
            if (!CheckProviderAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var (success, message, data) = await _attendanceService.GetAttendanceByStudentAsync(studentId);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return View(new List<AttendanceRecordDto>());
            }

            ViewBag.StudentId = studentId;
            return View(data ?? new List<AttendanceRecordDto>());
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!CheckProviderAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var (success, message, data) = await _attendanceService.GetAttendanceDetailsAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Records");
            }

            return View(data);
        }

        public async Task<IActionResult> DailySummary()
        {
            if (!CheckProviderAccess())
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập với tài khoản Provider để truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine($"Token exists: {!string.IsNullOrEmpty(token)}");
            Console.WriteLine($"User role: {role}");

            var (success, message, data) = await _attendanceService.GetDailySummaryAsync();
            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return View(new List<DailySummaryDto>());
            }

            return View(data ?? new List<DailySummaryDto>());
        }
    }
}
