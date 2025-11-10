using Microsoft.AspNetCore.Mvc;
using Project_PRN232.DTOs;
using Project_PRN232.Services;

namespace Project_PRN232.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly AuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AdminService adminService, AuthService authService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _authService = authService;
            _logger = logger;
        }

        private bool CheckAdminAccess()
        {
            if (!_authService.IsLoggedIn())
            {
                return false;
            }

            var userRole = _authService.GetUserRole();
            return userRole == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!CheckAdminAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var statistics = await _adminService.GetUserStatisticsAsync();
            ViewBag.Statistics = statistics;

            return View();
        }

        public async Task<IActionResult> Users()
        {
            if (!CheckAdminAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var users = await _adminService.GetAllUsersAsync();
            return View(users ?? new List<UserDto>());
        }

        public IActionResult CreateUser()
        {
            if (!CheckAdminAccess())
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
        public async Task<IActionResult> CreateUser(CreateUserDto model)
        {
            if (!CheckAdminAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.UserEmail = _authService.GetUserEmail();
                ViewBag.FullName = _authService.GetFullName();
                return View(model);
            }

            var result = await _adminService.CreateUserAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Users));
            }
            else
            {

                ViewBag.UserEmail = _authService.GetUserEmail();
                ViewBag.FullName = _authService.GetFullName();

                var errorLines = result.Message.Split('\n');
                if (errorLines.Length > 1)
                {
                    ViewBag.ValidationErrors = errorLines;
                }
                
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> EditUser(int id)
        {
            if (!CheckAdminAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy user!";
                return RedirectToAction(nameof(Users));
            }

            ViewBag.UserId = user.UserId;
            ViewBag.Email = user.Email;
            ViewBag.CreatedAt = user.CreatedAt;
            ViewBag.UpdatedAt = user.UpdatedAt;

            var model = new UpdateUserDto
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                Status = user.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UpdateUserDto model)
        {
            if (!CheckAdminAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {

                ViewBag.UserEmail = _authService.GetUserEmail();
                ViewBag.FullName = _authService.GetFullName();
                
                var user = await _adminService.GetUserByIdAsync(id);
                if (user != null)
                {
                    ViewBag.UserId = user.UserId;
                    ViewBag.Email = user.Email;
                    ViewBag.CreatedAt = user.CreatedAt;
                    ViewBag.UpdatedAt = user.UpdatedAt;
                }
                
                return View(model);
            }

            var result = await _adminService.UpdateUserAsync(id, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Users));
            }
            else
            {

                ViewBag.UserEmail = _authService.GetUserEmail();
                ViewBag.FullName = _authService.GetFullName();
                
                var user = await _adminService.GetUserByIdAsync(id);
                if (user != null)
                {
                    ViewBag.UserId = user.UserId;
                    ViewBag.Email = user.Email;
                    ViewBag.CreatedAt = user.CreatedAt;
                    ViewBag.UpdatedAt = user.UpdatedAt;
                }

                var errorLines = result.Message.Split('\n');
                if (errorLines.Length > 1)
                {
                    ViewBag.ValidationErrors = errorLines;
                }
                
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!CheckAdminAccess())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            var result = await _adminService.DeleteUserAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (!CheckAdminAccess())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            var result = await _adminService.ToggleUserStatusAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> ResetPassword(int id)
        {
            if (!CheckAdminAccess())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();

            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy user!";
                return RedirectToAction(nameof(Users));
            }

            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirm(int id)
        {
            if (!CheckAdminAccess())
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
            }

            var result = await _adminService.ResetUserPasswordAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}

