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
    public class StudentAnswerController : ControllerBase
    {
        private readonly IStudentAnswerService _studentAnswerService;
        private readonly ILogger<StudentAnswerController> _logger;

        public StudentAnswerController(IStudentAnswerService studentAnswerService, ILogger<StudentAnswerController> logger)
        {
            _logger = logger;
            _studentAnswerService = studentAnswerService;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SaveAnswer([FromBody] CreateStudentAnswerRequestDto request)
        {
            _logger.LogInformation("Save student answer request received. AttendanceId: {AttendanceId}, QuestionId: {QuestionId}",
                request.ExamAttendanceId,
                request.QuestionId);

            var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                _logger.LogWarning("Save student answer failed because student user ID was not found in token.");

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _studentAnswerService.SaveAnswerAsync(request,studentUserId);

            return Ok(result);
        }
    }
}
