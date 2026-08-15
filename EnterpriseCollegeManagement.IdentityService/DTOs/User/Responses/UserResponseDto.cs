using Azure.Identity;

namespace EnterpriseCollegeManagement.IdentityService.DTOs.User.Responses
{
    public class UserResponseDto
    {

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? PreviousRole { get; set; }

        public string NewRole { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
