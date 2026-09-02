namespace EnterpriseCollegeManagement.IdentityService.DTOs.User.Responses
{
    public class UserListResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? Role { get; set; }
    }
}
