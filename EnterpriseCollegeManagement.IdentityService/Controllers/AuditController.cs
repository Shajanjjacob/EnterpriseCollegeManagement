using EnterpriseCollegeManagement.IdentityService.DTOs.Audit.Requests;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCollegeManagement.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserHistory(
     string userId,
     [FromQuery] AuditHistoryFilterDto filter)
        {
            var result = await _auditService.GetUserHistoryAsync(userId, filter);

            return Ok(result);
        }
    }
}
