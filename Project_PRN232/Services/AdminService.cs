using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Project_PRN232.Services
{
    public class AdminService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET: Lấy tất cả users (không bao gồm Admin)
        public async Task<List<UserDto>?> GetAllUsersAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Admin/users";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var users = JsonSerializer.Deserialize<List<UserDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return users;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // GET: Lấy thông tin chi tiết 1 user
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Admin/users/{id}";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var user = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return user;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // POST: Tạo user mới
        public async Task<(bool Success, string Message, UserDto? User)> CreateUserAsync(CreateUserDto request)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Admin/users";
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log để debug
                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Tạo user thành công", result?.Data);
                }
                else
                {
                    // Parse error response để lấy message chi tiết
                    try
                    {
                        // Kiểm tra xem có phải validation errors không
                        var errorDoc = JsonDocument.Parse(responseContent);
                        
                        if (errorDoc.RootElement.TryGetProperty("errors", out var errors))
                        {
                            // Validation errors từ ModelState
                            var errorMessages = new List<string>();
                            foreach (var error in errors.EnumerateObject())
                            {
                                foreach (var message in error.Value.EnumerateArray())
                                {
                                    errorMessages.Add($"{error.Name}: {message.GetString()}");
                                }
                            }
                            return (false, string.Join("\n", errorMessages), null);
                        }
                        else
                        {
                            // Thử parse ApiResponse
                            var errorResponse = JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return (false, errorResponse?.Message ?? "Tạo user thất bại", null);
                        }
                    }
                    catch
                    {
                        // Nếu không parse được, trả về raw content
                        return (false, $"Lỗi từ server: {responseContent}", null);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}", null);
            }
        }

        // PUT: Cập nhật user
        public async Task<(bool Success, string Message)> UpdateUserAsync(int id, UpdateUserDto request)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Admin/users/{id}";
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log để debug
                Console.WriteLine($"Update Status Code: {response.StatusCode}");
                Console.WriteLine($"Update Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Cập nhật thành công");
                }
                else
                {
                    // Parse error response để lấy message chi tiết
                    try
                    {
                        // Kiểm tra xem có phải validation errors không
                        var errorDoc = JsonDocument.Parse(responseContent);
                        
                        if (errorDoc.RootElement.TryGetProperty("errors", out var errors))
                        {
                            // Validation errors từ ModelState
                            var errorMessages = new List<string>();
                            foreach (var error in errors.EnumerateObject())
                            {
                                foreach (var message in error.Value.EnumerateArray())
                                {
                                    errorMessages.Add($"{error.Name}: {message.GetString()}");
                                }
                            }
                            return (false, string.Join("\n", errorMessages));
                        }
                        else
                        {
                            // Thử parse ApiResponse
                            var errorResponse = JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return (false, errorResponse?.Message ?? "Cập nhật thất bại");
                        }
                    }
                    catch
                    {
                        // Nếu không parse được, trả về raw content
                        return (false, $"Lỗi từ server: {responseContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // DELETE: Xóa user
        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Admin/users/{id}";

                var response = await _httpClient.DeleteAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Xóa user thành công");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Xóa user thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // PUT: Reset mật khẩu user về mặc định
        public async Task<(bool Success, string Message)> ResetUserPasswordAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Admin/users/{id}/reset-password";
                var content = new StringContent("", Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Reset mật khẩu thành công");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Reset mật khẩu thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // PUT: Toggle status user
        public async Task<(bool Success, string Message)> ToggleUserStatusAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Admin/users/{id}/toggle-status";
                var content = new StringContent("", Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Thay đổi trạng thái thành công");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Thay đổi trạng thái thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // GET: Thống kê users
        public async Task<UserStatisticsDto?> GetUserStatisticsAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Admin/users/statistics";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<UserStatisticsDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Data;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    // DTOs
    public class UserDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
    }

    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
