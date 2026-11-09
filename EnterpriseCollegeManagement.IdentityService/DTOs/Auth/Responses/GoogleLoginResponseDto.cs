namespace EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses
{
    public class GoogleLoginResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiration { get; set; }

        public string? Role { get; set; }
    }
}
