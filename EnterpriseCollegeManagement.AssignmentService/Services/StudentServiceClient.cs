using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.AssignmentService.Services
{
    public class StudentServiceClient : IStudentServiceClient
    {
        public readonly ILogger<StudentServiceClient> _logger;
        private readonly HttpClient _httpClient;


        public StudentServiceClient(HttpClient httpClient, ILogger<StudentServiceClient> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<StudentResponseDto?> GetStudentByUserIdAsync(string userId)
        {
            _logger.LogInformation("Requesting student profile. UserId: {UserId}", userId);

            var response = await _httpClient.GetAsync($"api/Student/user/{userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Student profile not found. UserId: {UserId}",userId);

                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<StudentResponseDto>();
        }
    
    }
}
