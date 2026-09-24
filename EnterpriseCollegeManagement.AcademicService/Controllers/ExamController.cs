using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.AcademicService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly ILogger<ExamController> _logger;

        public ExamController(IExamService examService, ILogger<ExamController> logger)
        {
            _examService = examService;
            _logger = logger;
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamRequestDto request)
        {
            _logger.LogInformation("Create exam request received. CourseSubjectId: {CourseSubjectId}", request.CourseSubjectId);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                _logger.LogWarning("Create exam failed because user ID was not found in token.");

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _examService.CreateExamAsync(
                request,
                actorUserId);

            return CreatedAtAction(
                nameof(GetExamById),
                new { id = result.Id },
                result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetAllExams()
        {
            _logger.LogInformation("Get all exams request received.");

            var result = await _examService.GetAllExamsAsync();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetExamById(int id)
        {
            _logger.LogInformation( "Get exam by ID request received. ExamId: {ExamId}",id);

            var result = await _examService.GetExamByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Exam not found."
                });
            }

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateExam( int id, [FromBody] CreateExamRequestDto request)
        {
            _logger.LogInformation("Update exam request received. ExamId: {ExamId}",id);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                _logger.LogWarning( "Update exam failed because user ID was not found in token. ExamId: {ExamId}", id);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _examService.UpdateExamAsync(id,request,actorUserId);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            _logger.LogInformation( "Delete exam request received. ExamId: {ExamId}",id);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                _logger.LogWarning( "Delete exam failed because user ID was not found in token. ExamId: {ExamId}", id);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            await _examService.DeleteExamAsync( id, actorUserId);

            return Ok(new
            {
                Success = true,
                Message = "Exam deleted successfully."
            });
        }
    }
}
