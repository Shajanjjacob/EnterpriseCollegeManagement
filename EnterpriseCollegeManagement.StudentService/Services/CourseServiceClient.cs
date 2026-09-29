using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.StudentService.Services
{
    public class CourseServiceClient : ICourseServiceClient
    {
        private readonly ILogger<CourseServiceClient> _logger;
        private readonly HttpClient _httpClient;

        public CourseServiceClient(ILogger<CourseServiceClient> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CourseResponseDto?> GetCourseByIdAsync(int courseId)
        {
            _logger.LogInformation("Requesting course information from AcademicService. CourseId: {CourseId}",courseId);

            var response  = await _httpClient.GetAsync($"api/Course/{courseId}");

            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Course not found in AcademicService. CourseId: {CourseId}",courseId);

                return null;
            }
            response.EnsureSuccessStatusCode();

            var course = await response.Content.ReadFromJsonAsync<CourseResponseDto>();
            return course;
        }
    }
}
