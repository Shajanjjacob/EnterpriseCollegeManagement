namespace EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
        public string? RefreshToken { get; set; }

        public string? Role { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
