using EnterpriseCollegeManagement.TeacherService.Interfaces;

namespace EnterpriseCollegeManagement.TeacherService.Services
{
    public class AuditServiceClient : IAuditServiceClient
    {
        private readonly ILogger<AuditServiceClient> _logger;
        private readonly HttpClient _httpClient;

        public AuditServiceClient(ILogger<AuditServiceClient> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task LogAuditAsync(string? userId, string action, string entityName, string? entityId = null, string? description = null, string? oldValues = null, string? newValues = null)
        {
            var request = new
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues
            };

            var response = await _httpClient.PostAsJsonAsync("api/Audit/create", request);  //posting data to audit table 
            //,at post time  JSON request body get deserialize it into CreateAuditLogRequestDto.
            response.EnsureSuccessStatusCode();
        }
    }
}
