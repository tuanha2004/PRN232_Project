using System.Text.Json;
using Project_PRN232.DTOs;

namespace Project_PRN232.Services
{
    public class JobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public JobService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<JobDto>> GetAllJobsAsync()
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Jobs";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jobs = JsonSerializer.Deserialize<List<JobDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return jobs ?? new List<JobDto>();
                }

                return new List<JobDto>();
            }
            catch (Exception)
            {
                return new List<JobDto>();
            }
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            try
            {
                var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/api/Jobs/{id}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var job = JsonSerializer.Deserialize<JobDto>(content, new JsonSerializerOptions
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
    }
}
