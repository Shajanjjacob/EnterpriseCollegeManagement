using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;

namespace EnterpriseCollegeManagement.TeacherService.Interfaces
{
    public interface IStudentServiceClient
    {
        Task<StudentResponseDto?> GetStudentByUserIdAsync(string userId);

        Task<DepartmentResponse?> GetDepartmentByIdAsync(int departmentId);
    }
}
