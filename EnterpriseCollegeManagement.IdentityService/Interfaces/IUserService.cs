using EnterpriseCollegeManagement.IdentityService.DTOs.User.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.User.Responses;

namespace EnterpriseCollegeManagement.IdentityService.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> AssignRoleAsync(string actorUserId, AssignRoleRequestDto request);
        Task<List<UserListResponseDto>> GetUsersAsync(string actorUserId);
    }
}
