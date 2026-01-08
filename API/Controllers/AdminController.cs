using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using API.Models;
using API.DTOs.Users;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ProjectPrn232Context _context;

        public AdminController(ProjectPrn232Context context)
        {
            _context = context;
        }

        //[HttpGet("users")]
        //[EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
        //public async Task<ActionResult<IEnumerable<User>>> GetAllNonAdminUsers()
        //{
        //    try
        //    {
        //        var users = _context.Users
        //            .Where(u => u.Role != "Admin")
        //            .AsQueryable();

        //        return Ok(users);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            Success = false,
        //            Message = $"Lỗi server: {ex.Message}"
        //        });
        //    }
        //}

        [HttpGet("users/{id}")]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select | AllowedQueryOptions.Expand)]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
                var user = await _context.Users
                    .Where(u => u.UserId == id && u.Role != "Admin")
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy user hoặc user là Admin"
                    });
                }

                return Ok(user);
        }

        [HttpPost("users")]
        public async Task<ActionResult<object>> CreateUser([FromBody] CreateUserRequest request)
        {

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                if (request.Role?.ToLower() == "admin")
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không được phép tạo tài khoản Admin"
                    });
                }

                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Email đã tồn tại trong hệ thống"
                    });
                }

                var user = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = request.Password,
                    Role = string.IsNullOrEmpty(request.Role) ? "User" : request.Role,
                    Phone = request.Phone,
                    Address = request.Address,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "Active"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new
                {
                    Success = true,
                    Message = "Tạo user thành công",
                    Data = new
                    {
                        user.UserId,
                        user.FullName,
                        user.Email,
                        user.Role,
                        user.Phone,
                        user.Address,
                        user.Status,
                        user.CreatedAt
                    }
                });
            
           
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy user"
                    });
                }

                if (user.Role?.ToLower() == "admin")
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không được phép cập nhật tài khoản Admin"
                    });
                }

                if (request.Role?.ToLower() == "admin")
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không được phép đổi role thành Admin"
                    });
                }

                user.FullName = request.FullName ?? user.FullName;
                user.Phone = request.Phone ?? user.Phone;
                user.Address = request.Address ?? user.Address;
                user.Role = request.Role ?? user.Role;
                user.Status = request.Status ?? user.Status;
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Cập nhật user thành công",
                    Data = new
                    {
                        user.UserId,
                        user.FullName,
                        user.Email,
                        user.Role,
                        user.Phone,
                        user.Address,
                        user.Status,
                        user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy user"
                    });
                }

                if (user.Role?.ToLower() == "admin")
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không được phép xóa tài khoản Admin"
                    });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Xóa user thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpPut("users/{id}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy user"
                    });
                }

                if (user.Role?.ToLower() == "admin")
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không được phép reset mật khẩu Admin"
                    });
                }

                user.PasswordHash = "123456789";
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Reset mật khẩu thành công. Mật khẩu mới: 123456789"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpPut("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Không tìm thấy user"
                    });
                }

                if (user.Role?.ToLower() == "admin")
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Không được phép thay đổi trạng thái Admin"
                    });
                }

                user.Status = user.Status == "Active" ? "Inactive" : "Active";
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã {(user.Status == "Active" ? "kích hoạt" : "vô hiệu hóa")} tài khoản",
                    Data = new
                    {
                        user.UserId,
                        user.FullName,
                        user.Email,
                        user.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpGet("users/statistics")]
        public async Task<ActionResult<object>> GetUserStatistics()
        {
            try
            {
                var totalUsers = await _context.Users.Where(u => u.Role != "Admin").CountAsync();
                var activeUsers = await _context.Users.Where(u => u.Role != "Admin" && u.Status == "Active").CountAsync();
                var inactiveUsers = await _context.Users.Where(u => u.Role != "Admin" && u.Status == "Inactive").CountAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Lấy thống kê thành công",
                    Data = new
                    {
                        TotalUsers = totalUsers,
                        ActiveUsers = activeUsers,
                        InactiveUsers = inactiveUsers
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpGet("users/search")]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
        public ActionResult<IQueryable<User>> SearchUsers()
        {
            try
            {
                var users = _context.Users
                    .Where(u => u.Role != "Admin")
                    .AsQueryable();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}
