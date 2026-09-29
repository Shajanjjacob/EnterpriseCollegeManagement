using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IStudentServiceClient
    {

        Task<DepartmentResponse?> GetDepartmentByIdAsync(int departmentId);

        Task<StudentResponseDto?> GetStudentByUserIdAsync(string userId);
    }
}
