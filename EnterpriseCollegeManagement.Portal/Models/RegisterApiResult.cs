namespace EnterpriseCollegeManagement.Portal.Models
{
    public class RegisterApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }
}
