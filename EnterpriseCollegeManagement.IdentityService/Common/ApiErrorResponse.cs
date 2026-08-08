namespace EnterpriseCollegeManagement.IdentityService.Common
{
    public class ApiErrorResponse
    {
        public bool Success { get; set; }
        public  int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
