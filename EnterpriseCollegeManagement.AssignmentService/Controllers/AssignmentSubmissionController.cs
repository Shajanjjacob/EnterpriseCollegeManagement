using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.AssignmentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentSubmissionController : ControllerBase
    {
        private readonly IAssignmentSubmissionService _assignmentSubmissionService;
        private readonly ILogger<AssignmentSubmissionController> _logger;
        private readonly IAssignmentReviewService _assignmentReviewService; //AI

        public AssignmentSubmissionController(IAssignmentSubmissionService assignmentSubmissionService, ILogger<AssignmentSubmissionController> logger,
            IAssignmentReviewService assignmentReviewService)
        {
            _assignmentSubmissionService = assignmentSubmissionService;
            _logger = logger;
            _assignmentReviewService = assignmentReviewService;
        }


       [HttpPost("submit")]
        [Authorize(Roles = "Student")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitAssignment([FromForm] SubmitAssignmentRequestDto request)
        {
            var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Student user ID not found in token."
                });
            }

            _logger.LogInformation("Assignment submission requested by student {StudentUserId} for assignment {AssignmentId}",studentUserId,request.AssignmentId);

            var result = await _assignmentSubmissionService.SubmitAssignmentAsync(request,studentUserId);

            return CreatedAtAction(
                nameof(SubmitAssignment),
                new { id = result.Id },
                new
                {
                    Success = true,
                    Message = "Assignment submitted successfully.",
                    Data = result
                });
        }


        [Authorize(Roles = "Student")]
        [HttpGet("my-submissions")]
        public async Task<IActionResult> GetMySubmissions()
        {
            var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                _logger.LogWarning("Student user ID not found in JWT claims.");

                return Unauthorized(new
                {
                    Success = false,
                    Message = "Student user ID not found in token."
                });
            }

            _logger.LogInformation("Fetching submissions for student. UserId: {UserId}", studentUserId);

            var result = await _assignmentSubmissionService.GetMySubmissionsAsync(studentUserId);

            _logger.LogInformation("Retrieved {Count} submissions for student. UserId: {UserId}", result.Count,studentUserId);

            return Ok(new
            {
                Success = true,
                Message = "Student submissions retrieved successfully.",
                Data = result
            });
        }

        [HttpGet("assignment/{assignmentId}/submissions")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetSubmissionsByAssignment(int assignmentId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("User ID not found in JWT claims.");

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User ID not found in token."
                });
            }

            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("User {UserId} requested submissions for assignment {AssignmentId}.",currentUserId,assignmentId);

            var result = await _assignmentSubmissionService.GetSubmissionsByAssignmentAsync(assignmentId,currentUserId,isAdmin);

            return Ok(new
            {
                Success = true,
                Message = "Assignment submissions retrieved successfully.",
                Data = result
            });
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetSubmissionById(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("User ID not found in JWT claims.");

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User ID not found in token."
                });
            }

            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("User {UserId} requested submission {SubmissionId}.",currentUserId, id);

            var result = await _assignmentSubmissionService.GetSubmissionByIdAsync(id, currentUserId, isAdmin);

            return Ok(new
            {
                Success = true,
                Message = "Submission retrieved successfully.",
                Data = result
            });
        }


        [HttpPut("{id:int}/grade")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GradeSubmission(int id,[FromBody] GradeSubmissionRequestDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("User ID not found in JWT claims while grading submission {SubmissionId}.", id);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User ID not found in token."
                });
            }

            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation( "Grade request received. SubmissionId: {SubmissionId}, UserId: {UserId}", id, currentUserId);

            var result = await _assignmentSubmissionService.GradeSubmissionAsync(id, request, currentUserId, isAdmin);

            return Ok(new
            {
                Success = true,
                Message = "Submission graded successfully.",
                Data = result
            });
        }


        [HttpPost("assignment/{assignmentId:int}/submission/{submissionId:int}/ai-review")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> ReviewAssignment(int assignmentId, int submissionId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("User ID not found in JWT claims during AI review. AssignmentId: {AssignmentId}, SubmissionId: {SubmissionId}", assignmentId,
                    submissionId);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User ID not found in token."
                });
            }

            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("AI assignment review requested. AssignmentId: {AssignmentId}, SubmissionId: {SubmissionId}, UserId: {UserId}", assignmentId,
                submissionId,
                currentUserId);

            var review = await _assignmentReviewService.ReviewAssignmentAsync(assignmentId, submissionId, currentUserId, isAdmin);

            _logger.LogInformation("AI assignment review completed. AssignmentId: {AssignmentId}, SubmissionId: {SubmissionId}", assignmentId,submissionId);

            return Ok(new
            {
                Success = true,
                Message = "AI assignment review completed successfully.",
                Data = review
            });
        }

    }
}
