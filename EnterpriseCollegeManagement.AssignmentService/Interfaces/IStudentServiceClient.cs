using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface IStudentServiceClient
    {
        Task<StudentResponseDto?> GetStudentByUserIdAsync(string userId);
    }
}
