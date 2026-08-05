namespace EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
