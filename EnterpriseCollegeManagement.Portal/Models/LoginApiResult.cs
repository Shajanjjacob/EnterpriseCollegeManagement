namespace EnterpriseCollegeManagement.Portal.Models
{
    public class LoginApiResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public string? Token { get; set; }

        public DateTime? Expiration { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime? RefreshTokenExpiration { get; set; }

        public string? Role { get; set; }
    }
}
