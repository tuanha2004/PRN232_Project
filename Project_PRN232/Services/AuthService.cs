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

                    if (loginResponse != null && loginResponse.Success && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        _httpContextAccessor.HttpContext?.Session.SetString("JwtToken", loginResponse.Token);
                        _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", loginResponse.User?.Email ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("UserRole", loginResponse.User?.Role ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("FullName", loginResponse.User?.FullName ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("UserPhone", loginResponse.User?.Phone ?? "");
                        _httpContextAccessor.HttpContext?.Session.SetString("UserAddress", loginResponse.User?.Address ?? "");
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
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Authen/register";
                
                var jsonContent = JsonSerializer.Serialize(new
                {
                    email = request.Email,
                    fullName = request.FullName,
                    password = request.Password,
                    phoneNumber = request.PhoneNumber,
                    role = "Student"
                }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (errorResponse != null && errorResponse.ContainsKey("message"))
                        {
                            return (false, errorResponse["message"].ToString() ?? "Đăng ký thất bại");
                        }
                    }
                    catch { }
                    
                    return (false, "Đăng ký thất bại");
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
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Authen/forgot-password";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (result != null && result.ContainsKey("message"))
                        {
                            return (true, result["message"].ToString() ?? "Mã xác nhận đã được gửi");
                        }
                    }
                    catch { }
                    
                    return (true, "Mã xác nhận đã được gửi đến email của bạn");
                }
                else
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (errorResponse != null && errorResponse.ContainsKey("message"))
                        {
                            return (false, errorResponse["message"].ToString() ?? "Không tìm thấy email này");
                        }
                    }
                    catch { }
                    
                    return (false, "Không tìm thấy email này");
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
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Authen/reset-password";
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.");
                }
                else
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (errorResponse != null && errorResponse.ContainsKey("message"))
                        {
                            return (false, errorResponse["message"].ToString() ?? "Đặt lại mật khẩu thất bại");
                        }
                    }
                    catch { }
                    
                    return (false, "Đặt lại mật khẩu thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(string fullName, string phone, string address)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
                var userEmail = _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userEmail))
                {
                    return (false, "Phiên đăng nhập đã hết hạn");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var getUserUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Users/me";
                var getUserResponse = await _httpClient.GetAsync(getUserUrl);
                
                if (!getUserResponse.IsSuccessStatusCode)
                {
                    return (false, "Không thể xác thực người dùng");
                }

                var userContent = await getUserResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<JsonElement>(userContent);
                var userId = userInfo.GetProperty("userId").GetInt32();

                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Users/{userId}";
                
                var requestData = new
                {
                    fullName = fullName,
                    phone = phone,
                    address = address
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    _httpContextAccessor.HttpContext?.Session.SetString("FullName", fullName);
                    _httpContextAccessor.HttpContext?.Session.SetString("UserPhone", phone);
                    _httpContextAccessor.HttpContext?.Session.SetString("UserAddress", address);

                    return (true, "Cập nhật thông tin thành công!");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.TryGetProperty("message", out var msg)
                        ? msg.GetString() ?? "Cập nhật thông tin thất bại"
                        : "Cập nhật thông tin thất bại";
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
                var userEmail = _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userEmail))
                {
                    return (false, "Phiên đăng nhập đã hết hạn");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var getUserUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Users/me";
                var getUserResponse = await _httpClient.GetAsync(getUserUrl);
                
                if (!getUserResponse.IsSuccessStatusCode)
                {
                    return (false, "Không thể xác thực người dùng");
                }

                var userContent = await getUserResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<JsonElement>(userContent);
                var userId = userInfo.GetProperty("userId").GetInt32();

                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Users/{userId}/change-password";
                
                var requestData = new
                {
                    OldPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.TryGetProperty("message", out var msg)
                        ? msg.GetString() ?? "Đổi mật khẩu thất bại"
                        : "Đổi mật khẩu thất bại";
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }
    }
}
