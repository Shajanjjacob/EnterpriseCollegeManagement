using EnterpriseCollegeManagement.IdentityService.DTOs.User.Requests;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPut("assign-role")]

        public async Task<IActionResult> RoleASsign( [FromBody] AssignRoleRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                _logger.LogWarning( "Unable to identify authenticated Admin user.");

                return Unauthorized();
            }
            _logger.LogInformation(
               "Admin requested role assignment. ActorUserId: {ActorUserId}, TargetUserId: {TargetUserId}, Role: {Role}",
               actorUserId,
               request.UserId,
               request.Role);

            var result = await _userService.AssignRoleAsync( actorUserId,request);

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetUsers()
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(actorUserId))
            {
                _logger.LogWarning( "Unable to identify authenticated Admin user.");
                return Unauthorized();
            }
            _logger.LogInformation( "Get users request started. AdminUserId: {AdminUserId}",actorUserId);



            var result = await _userService.GetUsersAsync(actorUserId);
            _logger.LogInformation(  "Get users request completed. AdminUserId: {AdminUserId}, UserCount: {UserCount}", actorUserId, result.Count);


            return Ok(result);
        }





    }
}
