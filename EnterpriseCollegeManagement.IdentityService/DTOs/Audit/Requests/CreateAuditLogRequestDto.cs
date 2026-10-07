namespace EnterpriseCollegeManagement.IdentityService.DTOs.Audit.Requests
{
    public class CreateAuditLogRequestDto
    {
        public string? UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        public string? Description { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }
    }
}
