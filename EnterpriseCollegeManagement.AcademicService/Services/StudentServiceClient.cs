using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using System.Net;

namespace EnterpriseCollegeManagement.AcademicService.Services
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
            _logger.LogInformation( "Requesting department information from StudentService. DepartmentId: {DepartmentId}",departmentId);

            var response = await _httpClient.GetAsync($"api/Department/{departmentId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Department not found in StudentService. DepartmentId: {DepartmentId}",departmentId);

                return null;
            }
            response.EnsureSuccessStatusCode();

            var departmentid =  await response.Content.ReadFromJsonAsync<DepartmentResponse>();  //json to object
            return departmentid;
        }

        public async Task<StudentResponseDto?> GetStudentByUserIdAsync(string userId)
        {
            _logger.LogInformation("Requesting student information from StudentService. UserId: {UserId}",userId);

            var response = await _httpClient.GetAsync($"api/Student/user/{userId}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning( "Student profile not found in StudentService. UserId: {UserId}", userId);
                return null;
            }
            response.EnsureSuccessStatusCode();

            var Student =await response.Content.ReadFromJsonAsync<StudentResponseDto>();
            return Student;
        }
    }
}
