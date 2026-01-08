using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Project_PRN232.DTOs;

namespace Project_PRN232.Services
{
    public class ProviderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProviderService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<JobDto>?> GetMyJobsAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/ProviderJobs";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jobs = JsonSerializer.Deserialize<List<JobDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return jobs;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<JobDto?> GetJobDetailsAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/ProviderJobs/{id}";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var job = JsonSerializer.Deserialize<JobDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return job;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message, JobDto? Job)> CreateJobAsync(CreateJobDto request)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/ProviderJobs";
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<JobDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Tạo job thành công", result?.Data);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<JobDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Tạo job thất bại", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateJobAsync(int id, UpdateJobDto request)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/ProviderJobs/{id}";
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<JobDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Cập nhật thành công");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<JobDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Cập nhật thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteJobAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/ProviderJobs/{id}";

                var response = await _httpClient.DeleteAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Xóa job thành công");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Xóa job thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<List<ApplicationDto>?> GetJobApplicationsAsync(int jobId)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/ProviderJobs/{jobId}/applications";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var applications = JsonSerializer.Deserialize<List<ApplicationDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return applications;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> UpdateApplicationStatusAsync(int applicationId, string status)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/ProviderJobs/applications/{applicationId}/status";
                var jsonContent = JsonSerializer.Serialize(new { Status = status });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Cập nhật status thành công");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Cập nhật status thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<ProviderStatisticsDto?> GetStatisticsAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/ProviderJobs/statistics";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<ProviderStatisticsDto>>(responseContent, new JsonSerializerOptions
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

        public async Task<(bool Success, string Message)> RemoveStudentFromJobAsync(int jobId, int studentId)
        {
            try
            {
                SetAuthorizationHeader();
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/ProviderJobs/{jobId}/assignments/{studentId}";

                var response = await _httpClient.DeleteAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, result?.Message ?? "Hủy phân công nhân viên thành công");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? "Hủy phân công nhân viên thất bại");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }
    }
}
