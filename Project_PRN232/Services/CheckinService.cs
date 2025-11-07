using System.Text;
using System.Text.Json;
using Project_PRN232.Models.DTOs;

namespace Project_PRN232.Services
{
    public class CheckinService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private string? ApiBaseUrl => _configuration["ApiSettings:BaseUrl"];

        public CheckinService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        }

        // Lấy tất cả checkin records của sinh viên
        public async Task<List<CheckinRecordResponse>> GetMyCheckinRecordsAsync()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return new List<CheckinRecordResponse>();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{ApiBaseUrl}/api/CheckinRecords/my-records");
            if (!response.IsSuccessStatusCode)
                return new List<CheckinRecordResponse>();

            var content = await response.Content.ReadAsStringAsync();
            var records = JsonSerializer.Deserialize<List<CheckinRecordResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return records ?? new List<CheckinRecordResponse>();
        }

        // Lấy checkin hiện tại (chưa checkout)
        public async Task<CheckinRecordResponse?> GetCurrentCheckinAsync()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return null;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{ApiBaseUrl}/api/CheckinRecords/current");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var record = JsonSerializer.Deserialize<CheckinRecordResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return record;
        }

        // Check-in
        public async Task<(bool Success, string Message, CheckinRecordResponse? Record)> CheckinAsync(int jobId)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return (false, "Vui lòng đăng nhập", null);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CheckinRequest { JobId = jobId };
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{ApiBaseUrl}/api/CheckinRecords/checkin", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var record = JsonSerializer.Deserialize<CheckinRecordResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return (true, "Check-in thành công!", record);
            }

            try
            {
                var error = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (error != null && error.ContainsKey("message"))
                {
                    return (false, error["message"].ToString() ?? "Check-in thất bại", null);
                }
                
                return (false, responseContent, null);
            }
            catch
            {
                return (false, $"Check-in thất bại: {responseContent}", null);
            }
        }

        // Check-out
        public async Task<(bool Success, string Message, CheckinRecordResponse? Record)> CheckoutAsync(int checkinId)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return (false, "Vui lòng đăng nhập", null);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CheckoutRequest { CheckinId = checkinId };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{ApiBaseUrl}/api/CheckinRecords/checkout", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var record = JsonSerializer.Deserialize<CheckinRecordResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return (true, "Check-out thành công!", record);
            }

            try
            {
                var error = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return (false, error?["message"] ?? "Check-out thất bại", null);
            }
            catch
            {
                return (false, "Check-out thất bại", null);
            }
        }
    }
}
