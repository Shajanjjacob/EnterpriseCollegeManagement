using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.AssignmentService.Services
{
    public class TeacherCourseSubjectService : ITeacherServiceClient
    {
        public readonly ILogger<TeacherCourseSubjectService> _logger;
        private readonly HttpClient _httpClient;

        public TeacherCourseSubjectService(ILogger<TeacherCourseSubjectService> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<int?> GetTeacherIdByUserIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/Teacher/user/{Uri.EscapeDataString(userId)}/teacher-id");

        if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Teacher profile not found. UserId: {UserId}", userId);

                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<int?>();
        }

        public async Task<bool> IsTeacherAssignedToCourseSubjectAsync(int teacherId, int courseSubjectId)
        {
            _logger.LogInformation("Checking CourseSubject assignment in TeacherService. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}", teacherId, courseSubjectId);


            var response = await _httpClient.GetAsync($"api/TeacherCourseSubject/teacher/{teacherId}/course-subject/{courseSubjectId}/assigned");
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Teacher or CourseSubject assignment not found. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}",
                      teacherId, courseSubjectId);

                return false;
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TeacherCourseSubjectAssignmentResponseDto>();
            _logger.LogInformation("CourseSubject assignment check completed. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}, IsAssigned: {IsAssigned}",
            teacherId, courseSubjectId, result);

            return result.IsAssigned;

        }
    }
}
