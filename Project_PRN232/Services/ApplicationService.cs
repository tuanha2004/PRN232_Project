using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Project_PRN232.Models.DTOs;

namespace Project_PRN232.Services
{
    public class ApplicationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Success, string Message)> CreateApplicationAsync(CreateApplicationRequest request)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Applications";
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");

                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Bạn cần đăng nhập để ứng tuyển");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    try
                    {
                        var successResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var message = successResponse.TryGetProperty("message", out var msg)
                            ? msg.GetString() ?? "Đơn ứng tuyển đã được gửi thành công!"
                            : "Đơn ứng tuyển đã được gửi thành công!";
                        return (true, message);
                    }
                    catch
                    {
                        return (true, "Đơn ứng tuyển đã được gửi thành công!");
                    }
                }
                else
                {

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var errorMessage = errorResponse.TryGetProperty("message", out var msg)
                            ? msg.GetString() ?? "Gửi đơn thất bại"
                            : "Gửi đơn thất bại";
                        return (false, errorMessage);
                    }
                    catch
                    {

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return (false, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!");
                        }
                        return (false, $"Gửi đơn thất bại (Mã lỗi: {response.StatusCode})");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<List<ApplicationDto>> GetMyApplicationsAsync()
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Applications/my";
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");

                if (string.IsNullOrEmpty(token))
                {
                    return new List<ApplicationDto>();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var applications = JsonSerializer.Deserialize<List<ApplicationDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return applications ?? new List<ApplicationDto>();
                }

                return new List<ApplicationDto>();
            }
            catch (Exception)
            {
                return new List<ApplicationDto>();
            }
        }
    }
}
