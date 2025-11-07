using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenController : ControllerBase
	{
		private readonly ProjectPrn232Context _context;
		private readonly JwtService _jwtService;

		public AuthenController(ProjectPrn232Context context, JwtService jwtService)
		{
			_context = context;
			_jwtService = jwtService;
		}

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new LoginResponse
				{
					Success = false,
					Message = "Dữ liệu không hợp lệ"
				});
			}

			try
			{
				var user = await _context.Users
					.FirstOrDefaultAsync(u => u.Email == request.Email);

				if (user == null)
				{
					return Unauthorized(new LoginResponse
					{
						Success = false,
						Message = "Email hoặc mật khẩu không đúng"
					});
				}

		
				if (user.PasswordHash != request.Password) 
				{
					return Unauthorized(new LoginResponse
					{
						Success = false,
						Message = "Email hoặc mật khẩu không đúng"
					});
				}

				if (user.Status != "Active")
				{
					return Unauthorized(new LoginResponse
					{
						Success = false,
						Message = "Tài khoản đã bị khóa"
					});
				}

				
				var token = _jwtService.GenerateToken(user.Email!, user.Role ?? "User");

				return Ok(new LoginResponse
				{
					Success = true,
					Message = "Đăng nhập thành công",
					Token = token,
					User = new UserInfo
					{
						UserId = user.UserId,
						FullName = user.FullName ?? "",
						Email = user.Email ?? "",
						Role = user.Role ?? "User",
						Phone = user.Phone,
						Address = user.Address
					}
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new LoginResponse
				{
					Success = false,
					Message = $"Lỗi server: {ex.Message}"
				});
			}
		}

		[HttpPost("register")]
		[AllowAnonymous]
		public async Task<ActionResult> Register([FromBody] RegisterUserRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Dữ liệu không hợp lệ" });
			}

			try
			{
				// Kiểm tra email đã tồn tại chưa
				var existingUser = await _context.Users
					.FirstOrDefaultAsync(u => u.Email == request.Email);

				if (existingUser != null)
				{
					return BadRequest(new { message = "Email đã được đăng ký" });
				}

				// Tạo user mới
				var newUser = new User
				{
					Email = request.Email,
					FullName = request.FullName,
					PasswordHash = request.Password, // Plaintext password
					Phone = request.PhoneNumber,
					Role = request.Role ?? "Student", // Mặc định là Student
					Status = "Active",
					CreatedAt = DateTime.Now
				};

				_context.Users.Add(newUser);
				await _context.SaveChangesAsync();

				return Ok(new { message = "Đăng ký thành công! Vui lòng đăng nhập." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
			}
		}

		[HttpPost("forgot-password")]
		[AllowAnonymous]
		public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Email không hợp lệ" });
			}

			try
			{
				var user = await _context.Users
					.FirstOrDefaultAsync(u => u.Email == request.Email);

				if (user == null)
				{
					// Không tiết lộ email có tồn tại hay không (security best practice)
					return Ok(new { message = "Nếu email tồn tại, mã xác nhận đã được gửi đến email của bạn" });
				}

				// Tạo reset code (6 chữ số)
				var resetCode = new Random().Next(100000, 999999).ToString();
				
				// Lưu reset code vào database (giả sử có trường ResetPasswordCode và ResetPasswordExpiry)
				// Trong thực tế bạn cần thêm 2 field này vào User model
				// user.ResetPasswordCode = resetCode;
				// user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
				// await _context.SaveChangesAsync();

				// TODO: Gửi email với reset code (tạm thời trả về code để test)
				return Ok(new { 
					message = "Mã xác nhận: " + resetCode + " (Hạn 15 phút)",
					resetCode = resetCode // Chỉ để test, production phải gửi qua email
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
			}
		}

		[HttpPost("reset-password")]
		[AllowAnonymous]
		public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Dữ liệu không hợp lệ" });
			}

			try
			{
				var user = await _context.Users
					.FirstOrDefaultAsync(u => u.Email == request.Email);

				if (user == null)
				{
					return BadRequest(new { message = "Thông tin không hợp lệ" });
				}

				// TODO: Kiểm tra reset code và expiry time
				// if (user.ResetPasswordCode != request.ResetCode || 
				//     user.ResetPasswordExpiry < DateTime.Now)
				// {
				//     return BadRequest(new { message = "Mã xác nhận không đúng hoặc đã hết hạn" });
				// }

				// Cập nhật password mới
				user.PasswordHash = request.NewPassword;
				// user.ResetPasswordCode = null;
				// user.ResetPasswordExpiry = null;
				
				await _context.SaveChangesAsync();

				return Ok(new { message = "Đổi mật khẩu thành công! Vui lòng đăng nhập." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
			}
		}

		[HttpGet("validate")]
		[Authorize]
		public ActionResult ValidateToken()
		{
			var userEmail = User.Identity?.Name;
			var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

			return Ok(new
			{
				Success = true,
				Message = "Token hợp lệ",
				Email = userEmail,
				Role = userRole
			});
		}

		[HttpGet("admin-only")]
		[Authorize(Roles = "Admin")]
		public ActionResult AdminOnly()
		{
			return Ok(new { Message = "Chỉ Admin mới được truy cập" });
		}

		[HttpGet("user-or-admin")]
		[Authorize(Roles = "User,Admin")]
		public ActionResult UserOrAdmin()
		{
			return Ok(new { Message = "User hoặc Admin có thể truy cập" });
		}
	}
}
