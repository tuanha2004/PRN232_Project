using System.Text.Json;

namespace Project_PRN232.Services
{
    public class AttendanceService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AttendanceService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7777");
        }

        private void SetAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET: api/Attendance (OData)
        public async Task<(bool Success, string Message, List<AttendanceRecordDto>? Data)> GetAllAttendanceRecordsAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync("/api/Attendance");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // API trả về list trực tiếp, không wrap trong ApiResponse
                    var records = JsonSerializer.Deserialize<List<AttendanceRecordDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (true, "Lấy danh sách attendance thành công", records);
                }

                return (false, $"Lỗi: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }

        // GET: api/Attendance/job/{jobId}
        public async Task<(bool Success, string Message, List<AttendanceRecordDto>? Data)> GetAttendanceByJobAsync(int jobId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"/api/Attendance/job/{jobId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AttendanceRecordDto>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (apiResponse?.Success ?? false, apiResponse?.Message ?? "", apiResponse?.Data);
                }

                return (false, $"Lỗi: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }

        // GET: api/Attendance/student/{studentId}
        public async Task<(bool Success, string Message, List<AttendanceRecordDto>? Data)> GetAttendanceByStudentAsync(int studentId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"/api/Attendance/student/{studentId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<AttendanceRecordDto>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (apiResponse?.Success ?? false, apiResponse?.Message ?? "", apiResponse?.Data);
                }

                return (false, $"Lỗi: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }

        // GET: api/Attendance/{id}
        public async Task<(bool Success, string Message, AttendanceDetailDto? Data)> GetAttendanceDetailsAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"/api/Attendance/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AttendanceDetailDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (apiResponse?.Success ?? false, apiResponse?.Message ?? "", apiResponse?.Data);
                }

                return (false, $"Lỗi: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }

        // GET: api/Attendance/statistics
        public async Task<(bool Success, string Message, AttendanceStatisticsDto? Data)> GetStatisticsAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync("/api/Attendance/statistics");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AttendanceStatisticsDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (apiResponse?.Success ?? false, apiResponse?.Message ?? "", apiResponse?.Data);
                }

                return (false, $"Lỗi: {response.StatusCode}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }

        // GET: api/Attendance/summary/daily
        public async Task<(bool Success, string Message, List<DailySummaryDto>? Data)> GetDailySummaryAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync("/api/Attendance/summary/daily");
                var content = await response.Content.ReadAsStringAsync();

                // Log để debug
                Console.WriteLine($"DailySummary Status: {response.StatusCode}");
                Console.WriteLine($"DailySummary Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<DailySummaryDto>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (apiResponse?.Success ?? false, apiResponse?.Message ?? "", apiResponse?.Data);
                }

                // Parse error response
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<List<DailySummaryDto>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorResponse?.Message ?? $"Lỗi {response.StatusCode}", null);
                }
                catch
                {
                    return (false, $"Lỗi {response.StatusCode}: {content}", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }
    }

    // DTOs
    public class AttendanceRecordDto
    {
        public int CheckinId { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public int? JobId { get; set; }
        public string? JobTitle { get; set; }
        public DateTime? CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public double? WorkDuration { get; set; }
        public string? Status { get; set; }
    }

    public class AttendanceDetailDto
    {
        public int CheckinId { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentPhone { get; set; }
        public int? JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobLocation { get; set; }
        public DateTime? CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public double? WorkDuration { get; set; }
        public string? Status { get; set; }
    }

    public class AttendanceStatisticsDto
    {
        public int TotalCheckins { get; set; }
        public int CompletedCheckins { get; set; }
        public int InProgressCheckins { get; set; }
        public double TotalWorkHours { get; set; }
        public int MonthlyCheckins { get; set; }
        public double AverageWorkHoursPerCheckin { get; set; }
        public List<TopStudentDto>? TopStudents { get; set; }
    }

    public class TopStudentDto
    {
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public int TotalCheckins { get; set; }
        public int CompletedCheckins { get; set; }
        public double TotalWorkHours { get; set; }
    }

    public class DailySummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalCheckins { get; set; }
        public int CompletedCheckins { get; set; }
        public int UniqueStudents { get; set; }
        public double TotalWorkHours { get; set; }
    }
}
