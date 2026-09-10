namespace EnterpriseCollegeManagement.IdentityService.DTOs.User.Responses
{
    public class GetUserResponseDto
    {
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
