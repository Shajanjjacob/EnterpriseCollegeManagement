using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.TeacherService.Services
{
    public class AcademicServiceClient : IAcademicServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AcademicServiceClient> _logger;

        public AcademicServiceClient(HttpClient httpClient, ILogger<AcademicServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CourseResponseDto?> GetCourseByIdAsync(int courseId)
        {
            _logger.LogInformation("Requesting course information from AcademicService. CourseId: {CourseId}", courseId);

            var response = await _httpClient.GetAsync($"api/Course/{courseId}");

            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Course not found in AcademicService. CourseId: {CourseId}",courseId);

                return null;
            }

            response.EnsureSuccessStatusCode();

            var course = await response.Content.ReadFromJsonAsync<CourseResponseDto?>();
            return course;
        }

        public async Task<SubjectResponseDto?> GetSubjectByIdAsync(int subjectId)
        {
            _logger.LogInformation("Requesting subject information from AcademicService. SubjectId: {SubjectId}", subjectId);

            var response = await _httpClient.GetAsync($"api/Subject/{subjectId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Subject not found in AcademicService. SubjectId: {SubjectId}",subjectId);
                return null;

            }

            response.EnsureSuccessStatusCode();

            var subject = await response.Content.ReadFromJsonAsync<SubjectResponseDto?>();
            return subject;
        }

        public async Task<List<CourseSubjectResponseDto>> GetSubjectsByCourseIdAsync(int courseId)
        {
            _logger.LogInformation("Requesting subjects for course from AcademicService. CourseId: {CourseId}",courseId);

            var response = await _httpClient.GetAsync($"api/CourseSubject/course/{courseId}");

            response.EnsureSuccessStatusCode();
            var courseSubjects = await response.Content.ReadFromJsonAsync<List<CourseSubjectResponseDto>>();

            return courseSubjects ?? new List<CourseSubjectResponseDto>();
        }
    }
}
