using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.TeacherService.Services
{
    public class StudentServiceClient : IStudentServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StudentServiceClient> _logger;

        public StudentServiceClient(HttpClient httpClient, ILogger<StudentServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DepartmentResponse?> GetDepartmentByIdAsync(int departmentId)
        {
            _logger.LogInformation("Requesting department information from StudentService. DepartmentId: {DepartmentId}",departmentId);

            var response = await _httpClient.GetAsync($"api/Department/{departmentId}");

            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Department not found in StudentService. DepartmentId: {DepartmentId}",departmentId);

                return null;
            }

            response.EnsureSuccessStatusCode();

            var Department = await response.Content.ReadFromJsonAsync<DepartmentResponse>();
            return Department;
        }

        public async Task<StudentResponseDto?> GetStudentByUserIdAsync(string userId)
        {
            _logger.LogInformation("Requesting student information from StudentService. UserId: {UserId}", userId);

            var response = await _httpClient.GetAsync($"api/Student/user/{userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Student profile not found in StudentService. UserId: {UserId}",userId);

                return null;
            }

            response.EnsureSuccessStatusCode();

            var student = await response.Content.ReadFromJsonAsync<StudentResponseDto>();
            return student;
        }
    }
}
