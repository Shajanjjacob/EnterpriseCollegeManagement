using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.TeacherService.Services
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IIdentityServiceClient> _logger;

        public IdentityServiceClient(HttpClient httpClient, ILogger<IIdentityServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<TeacherUserResponseDto?> GetUserByIdAsync(string userId)
        {
            _logger.LogInformation("Requesting user information from IdentityService. UserId: {UserId}",userId);

            var response = await _httpClient.GetAsync($"api/User/{userId}");

            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User not found in IdentityService. UserId: {UserId}", userId);

                return null;
            }
            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<TeacherUserResponseDto>();

            return  user;
        }
    }
}
