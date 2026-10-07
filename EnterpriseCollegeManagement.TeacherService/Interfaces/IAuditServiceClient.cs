namespace EnterpriseCollegeManagement.TeacherService.Interfaces
{
    public interface IAuditServiceClient
    {
        Task LogAuditAsync(string? userId,string action,string entityName, string? entityId = null, string? description = null,string? oldValues = null,
        string? newValues = null);
    }
}
