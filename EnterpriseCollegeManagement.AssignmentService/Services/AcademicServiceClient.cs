using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.AssignmentService.Services
{
    public class AcademicServiceClient : IAcademicServiceClient
    {
        private readonly ILogger<AcademicServiceClient> _logger;
        private readonly HttpClient _httpClient;

        public AcademicServiceClient(ILogger<AcademicServiceClient> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CourseSubjectServiceResponseDto?> CourseSubjectExistsAsync(int courseSubjectId)
        {
            _logger.LogInformation("Requesting CourseSubject details from AcademicService. CourseSubjectId: {CourseSubjectId}",courseSubjectId);
            var response = await _httpClient.GetAsync($"api/CourseSubject/{courseSubjectId}");

            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("CourseSubject not found in AcademicService. CourseSubjectId: {CourseSubjectId}",courseSubjectId);

                return null;

            }
            response.EnsureSuccessStatusCode();

          var courseSubject =  await response.Content.ReadFromJsonAsync<CourseSubjectServiceResponseDto>();
            return courseSubject;
        }
    }
}
