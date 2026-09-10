namespace EnterpriseCollegeManagement.StudentService.DTOs.Responses
{
    public class IdentityUserResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Role {  get; set; } = string.Empty;
    }
}
