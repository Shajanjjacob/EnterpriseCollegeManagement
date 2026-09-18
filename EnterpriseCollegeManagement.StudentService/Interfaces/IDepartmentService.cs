using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.StudentService.Interfaces
{
    public interface IDepartmentService
    {
        Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentRequestDto request, string actorUserId);

        Task<DepartmentResponseDto?> UpdateDepartmentAsync(UpdateDepartmentRequestDto request, string actorUserId ,int id);

        Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id);

        Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync();

        Task<bool> DeleteDepartmentAsync(int id, string actorUserId);

    }
}
