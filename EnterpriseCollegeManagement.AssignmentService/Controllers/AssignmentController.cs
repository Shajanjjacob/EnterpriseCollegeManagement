using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.Exceptions;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.AssignmentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly ILogger<AssignmentController> _logger;
        private readonly IAssignmentService _assignmentService;
        public AssignmentController(ILogger<AssignmentController> logger, IAssignmentService  assignmentService)
        {
            _logger = logger;
            _assignmentService = assignmentService;
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequestDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var isAdmin = User.IsInRole("Admin");

            var result = await _assignmentService.CreateAssignmentAsync(request, currentUserId, isAdmin);

            return Ok(new
            {
                Success = true,
                Message = "Assignment created successfully.",
                Data = result
            });
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateAssignment(int id,[FromBody] UpdateAssignmentRequestDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var isAdmin = User.IsInRole("Admin");

            var result = await _assignmentService.UpdateAssignmentAsync(id, request,currentUserId,isAdmin);

            return Ok(new
            {
                Success = true,
                Message = "Assignment updated successfully.",
                Data = result
            });
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            _logger.LogInformation(
                "Get assignment request received. AssignmentId: {AssignmentId}", id);

            var result = await _assignmentService.GetAssignmentByIdAsync(id);

            _logger.LogInformation(
                "Assignment retrieved successfully. AssignmentId: {AssignmentId}", id);

            return Ok(new
            {
                Success = true,
                Message = "Assignment retrieved successfully.",
                Data = result
            });
        }
        [HttpGet("my-assignments")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(currentUserId == null)
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var isAdmin = User.IsInRole("Admin");
            _logger.LogInformation("Get my assignments request received. UserId: {UserId}, IsAdmin: {IsAdmin}", currentUserId, isAdmin);

            var result = await _assignmentService.GetMyAssignmentsAsync(currentUserId, isAdmin);

            return Ok(new
            {
                Success = true,
                Message = "Assignments retrieved successfully.",
                Data = result
            });
        }

        [HttpPatch("{id:int}/publish")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> PublishAssignment(int id,[FromBody] PublishAssignmentRequestDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Publish assignment request received. AssignmentId: {AssignmentId}, UserId: {UserId}", id, currentUserId);

            var result = await _assignmentService.PublishAssignmentAsync( id, request, currentUserId, isAdmin);

            return Ok(new
            {
                Success = true,
                Message = request.IsPublished
                    ? "Assignment published successfully."
                    : "Assignment unpublished successfully.",
                Data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Delete assignment request received. AssignmentId: {AssignmentId}, UserId: {UserId}",id, currentUserId);

            var result = await _assignmentService.DeleteAssignmentAsync(id, currentUserId, isAdmin);

            return Ok(new
            {
                Success = result,
                Message = "Assignment deleted successfully."
            });
        }
    }
}
