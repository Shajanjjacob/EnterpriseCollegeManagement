namespace EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests
{
    public class LogoutRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
