using EnterpriseCollegeManagement.IdentityService.DTOs.Audit.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Audit.Responses;

namespace EnterpriseCollegeManagement.IdentityService.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            string? userId,
            string action,
            string entityname,
            string? entityId = null,
            string? description = null,
            string? oldValues = null,
            string? newValues = null

            );

        Task<PagedResultDto<AuditLogResponseDto>> GetUserHistoryAsync(string userId, AuditHistoryFilterDto filter);

       
    }
}
