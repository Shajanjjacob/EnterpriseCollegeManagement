namespace EnterpriseCollegeManagement.IdentityService.DTOs.Audit.Requests
{
    public class AuditHistoryFilterDto
    {
        public DateTime? Date { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
