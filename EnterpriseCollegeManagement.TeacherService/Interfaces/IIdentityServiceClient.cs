using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;

namespace EnterpriseCollegeManagement.TeacherService.Interfaces
{
    public interface IIdentityServiceClient
    {
        Task<TeacherUserResponseDto?> GetUserByIdAsync(string userId);
    }
}
