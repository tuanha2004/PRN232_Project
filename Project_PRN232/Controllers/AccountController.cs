using Microsoft.AspNetCore.Mvc;
using Project_PRN232.Models.DTOs;
using Project_PRN232.Services;

namespace Project_PRN232.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì redirect về trang tương ứng
            if (_authService.IsLoggedIn())
            {
                var userRole = _authService.GetUserRole();
                
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (userRole == "Provider")
                {
                    return RedirectToAction("Index", "ProviderJobs");
                }
                
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                
                // Redirect dựa trên role
                var userRole = _authService.GetUserRole();
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (userRole == "Provider")
                {
                    return RedirectToAction("Index", "ProviderJobs");
                }
                
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["SuccessMessage"] = "Đã đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        // Logout via GET (for redirect after password change)
        [HttpGet]
        public IActionResult LogoutRedirect()
        {
            _authService.Logout();
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại với mật khẩu mới.";
            return RedirectToAction("Login");
        }

        // Trang chỉ dành cho Admin
        [HttpGet]
        public IActionResult AdminDashboard()
        {
            if (!_authService.IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var userRole = _authService.GetUserRole();
            if (userRole != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();
            return View();
        }

        // Trang Profile (yêu cầu đăng nhập)
        [HttpGet]
        public IActionResult Profile()
        {
            if (!_authService.IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserEmail = _authService.GetUserEmail();
            ViewBag.FullName = _authService.GetFullName();
            ViewBag.UserRole = _authService.GetUserRole();
            ViewBag.UserPhone = HttpContext.Session.GetString("UserPhone");
            ViewBag.UserAddress = HttpContext.Session.GetString("UserAddress");
            return View();
        }

        // Update Profile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string phone, string address)
        {
            if (!_authService.IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var result = await _authService.UpdateProfileAsync(fullName, phone, address);

            if (result.Success)
            {
                // Cập nhật lại session
                HttpContext.Session.SetString("FullName", fullName);
                HttpContext.Session.SetString("UserPhone", phone ?? "");
                HttpContext.Session.SetString("UserAddress", address ?? "");

                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Profile");
        }

        // Change Password
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!_authService.IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var result = await _authService.ChangePasswordAsync(currentPassword, newPassword);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                // Đăng xuất sau khi đổi mật khẩu thành công
                return RedirectToAction("LogoutRedirect");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Profile");
            }
        }

        // Sign Up
        [HttpGet]
        public IActionResult Register()
        {
            // Nếu đã đăng nhập rồi thì redirect về trang chủ
            if (_authService.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }

        // Forgot Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (_authService.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ForgotPasswordAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                // Chuyển sang trang ResetPassword và tự động điền email
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }

        // Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string email = "")
        {
            if (_authService.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            // Nếu không có email, yêu cầu quay lại trang ForgotPassword
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Vui lòng yêu cầu đặt lại mật khẩu trước!";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordRequest
            {
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ResetPasswordAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
        }
    }
}
