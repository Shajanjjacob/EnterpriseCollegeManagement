using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.AcademicService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamAttendanceController : ControllerBase
    {
        private readonly IExamAttendanceService _examAttendanceService;
        private readonly ILogger<ExamAttendanceController> _logger;

        public ExamAttendanceController(IExamAttendanceService examAttendanceService, ILogger<ExamAttendanceController> logger)
        {
            _examAttendanceService = examAttendanceService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StartExam([FromBody] CreateExamAttendanceRequestDto request)
        {
            _logger.LogInformation("Start exam request received. ExamId: {ExamId}", request.ExamId);

            var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                _logger.LogWarning("Start exam failed because student user ID was not found in token.");

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _examAttendanceService.StartExamAsync(request,studentUserId);

            return Ok(result);
        }

        [HttpGet("{attendanceId:int}/questions")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetExamQuestions(int attendanceId)
        {
            _logger.LogInformation("Get exam questions request received. AttendanceId: {AttendanceId}",attendanceId);

            var studentUserId =  User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                _logger.LogWarning("Get exam questions failed because student user ID was not found in token. AttendanceId: {AttendanceId}",attendanceId);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _examAttendanceService.GetExamQuestionsAsync(attendanceId,studentUserId);

            return Ok(result);
        }

        [HttpPost("{attendanceId:int}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitExam(int attendanceId)
        {
            _logger.LogInformation("Submit exam request received. AttendanceId: {AttendanceId}",attendanceId);

            var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                _logger.LogWarning("Submit exam failed because student user ID was not found in token. AttendanceId: {AttendanceId}",
                    attendanceId);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _examAttendanceService.SubmitExamAsync(studentUserId, attendanceId);

            _logger.LogInformation("Exam submitted successfully. AttendanceId: {AttendanceId}",attendanceId);

            return Ok(result);
        }


        [HttpPost("{attendanceId:int}/publish-result")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> PublishResult(int attendanceId)
        {
            _logger.LogInformation("Publish result request received. AttendanceId: {AttendanceId}",attendanceId);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                _logger.LogWarning("Publish result failed because user ID was not found in token. AttendanceId: {AttendanceId}", attendanceId);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _examAttendanceService.PublishResultAsync(attendanceId,actorUserId);

            return Ok(result);
        }

        [HttpGet("exam/{examId}/result")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentResult(int examId)
        {
            _logger.LogInformation("Get student result request received. ExamId: {ExamId}", examId);

            var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                _logger.LogWarning("Get student result failed because user ID was not found in token. ExamId: {ExamId}",examId);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _examAttendanceService.GetStudentResultAsync(examId,studentUserId);

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("exam/{examId}/submissions")]
        public async Task<IActionResult> GetExamSubmissions(int examId)
        {
            _logger.LogInformation(
                "Exam submissions request received. ExamId: {ExamId}",
                examId);

            var result = await _examAttendanceService.GetExamSubmissionsAsync(examId);

            return Ok(result);
        }

    }
}
