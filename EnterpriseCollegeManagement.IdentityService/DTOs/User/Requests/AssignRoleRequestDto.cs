using Microsoft.AspNetCore.SignalR;

namespace EnterpriseCollegeManagement.IdentityService.DTOs.User.Requests
{
    public class AssignRoleRequestDto
    {
        public string UserId { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
