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
						Role = user.Role ?? "User"
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
