using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Project_PRN232.Models.DTOs;

namespace Project_PRN232.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Authen/login";
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Lưu token vào session
                    if (loginResponse != null && loginResponse.Success && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        _httpContextAccessor.HttpContext?.Session.SetString("JwtToken", loginResponse.Token);
                        _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", loginResponse.User?.Email ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("UserRole", loginResponse.User?.Role ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("FullName", loginResponse.User?.FullName ?? "");
                    }

                    return loginResponse ?? new LoginResponse { Success = false, Message = "Lỗi không xác định" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse ?? new LoginResponse { Success = false, Message = "Đăng nhập thất bại" };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        public string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        }

        public bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(GetToken());
        }

        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");
        }

        public string? GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
        }

        public string? GetFullName()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("FullName");
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Users/register";
                var jsonContent = JsonSerializer.Serialize(new
                {
                    email = request.Email,
                    fullName = request.FullName,
                    password = request.Password,
                    phoneNumber = request.PhoneNumber,
                    role = "User" // Mặc định role là User
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đăng ký thành công! Vui lòng đăng nhập.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.TryGetProperty("message", out var msg) 
                        ? msg.GetString() ?? "Đăng ký thất bại" 
                        : "Đăng ký thất bại";
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Users/forgot-password";
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.TryGetProperty("message", out var msg) 
                        ? msg.GetString() ?? "Không tìm thấy email này" 
                        : "Không tìm thấy email này";
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Users/reset-password";
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.TryGetProperty("message", out var msg) 
                        ? msg.GetString() ?? "Đặt lại mật khẩu thất bại" 
                        : "Đặt lại mật khẩu thất bại";
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }
    }
}
