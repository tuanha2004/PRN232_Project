using API.DTOs.Users;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập
    public class UsersController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;

        public UsersController(ProjectPrn232Context context)
        {
            _context = context;
        }

        // GET: api/Users - CHỈ ADMIN xem tất cả users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.UserId,
                        u.FullName,
                        u.Email,
                        u.Role,
                        u.Phone,
                        u.Address,
                        u.Status,
                        u.CreatedAt,
                        // Không trả về PasswordHash
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: api/Users/5 - Admin xem user bất kỳ, User xem chính mình
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

                // User chỉ xem được thông tin của mình, Admin xem được tất cả
                if (userRole != "Admin" && currentUser?.UserId != id)
                {
                    return Forbid();
                }

                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { Message = "Không tìm thấy user" });
                }

                return Ok(new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.Phone,
                    user.Address,
                    user.Status,
                    user.CreatedAt,
                    user.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: api/Users/me - Lấy thông tin user đang đăng nhập
        [HttpGet("me")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return NotFound(new { Message = "Không tìm thấy user" });
                }

                return Ok(new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.Phone,
                    user.Address,
                    user.Status,
                    user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: api/Users - CHỈ ADMIN tạo user mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Kiểm tra email đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { Message = "Email đã tồn tại" });
                }

                var user = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = request.Password, // TODO: Hash password trong production
                    Role = request.Role,
                    Phone = request.Phone,
                    Address = request.Address,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "Active"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.Phone,
                    user.Address,
                    user.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: api/Users/5 - Admin cập nhật user bất kỳ, User cập nhật chính mình
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var userEmail = User.Identity?.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

                // User chỉ cập nhật được thông tin của mình, Admin cập nhật được tất cả
                if (userRole != "Admin" && currentUser?.UserId != id)
                {
                    return Forbid();
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = "Không tìm thấy user" });
                }

                // Cập nhật thông tin
                user.FullName = request.FullName ?? user.FullName;
                user.Phone = request.Phone ?? user.Phone;
                user.Address = request.Address ?? user.Address;
                user.UpdatedAt = DateTime.Now;

                // Chỉ Admin mới được đổi Role và Status
                if (userRole == "Admin")
                {
                    user.Role = request.Role ?? user.Role;
                    user.Status = request.Status ?? user.Status;
                }

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Cập nhật thành công", User = new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.Phone,
                    user.Address,
                    user.Status
                }});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: api/Users/5 - CHỈ ADMIN xóa user
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = "Không tìm thấy user" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Xóa user thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }

        // PUT: api/Users/5/change-password - Đổi mật khẩu
        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var userEmail = User.Identity?.Name;
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

                // User chỉ đổi được mật khẩu của mình, Admin đổi được tất cả
                if (userRole != "Admin" && currentUser?.UserId != id)
                {
                    return Forbid();
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = "Không tìm thấy user" });
                }

                // Kiểm tra mật khẩu cũ (nếu không phải Admin)
                if (userRole != "Admin")
                {
                    if (string.IsNullOrEmpty(request.OldPassword))
                    {
                        return BadRequest(new { Message = "Mật khẩu cũ là bắt buộc" });
                    }

                    if (user.PasswordHash != request.OldPassword)
                    {
                        return BadRequest(new { Message = "Mật khẩu cũ không đúng" });
                    }
                }

                user.PasswordHash = request.NewPassword; // TODO: Hash password trong production
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
