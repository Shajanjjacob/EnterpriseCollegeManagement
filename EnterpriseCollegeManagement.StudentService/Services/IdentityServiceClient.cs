using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Interfaces;

namespace EnterpriseCollegeManagement.StudentService.Services
{
    public class IdentityServiceClient : IIdentityServiceClient
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<IdentityServiceClient> _logger;

        public IdentityServiceClient(HttpClient httpClient, ILogger<IdentityServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }



        public async Task<IdentityUserResponseDto?> GetUserAsync(string userId)
        {
            _logger.LogInformation("Requesting user information from IdentityService. UserId: {UserId}",userId);

            var response = await _httpClient.GetAsync($"api/user/{userId}");

            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User not found in IdentityService. UserId: {UserId}",userId);

                return null;
            }
            response.EnsureSuccessStatusCode(); //return status code 

            var user = await response.Content.ReadFromJsonAsync<IdentityUserResponseDto>(); // convert json response into c# object of student service 

            return user;
        }
    }
}
