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
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly ILogger<QuestionController> _logger;

        public QuestionController(IQuestionService questionService, ILogger<QuestionController> logger)
        {
            _questionService = questionService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequestDto request)
        {
            _logger.LogInformation( "Create question request received. ExamId: {ExamId}",request.ExamId);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(string.IsNullOrWhiteSpace(actorUserId))
            {
                _logger.LogWarning("Create question failed because user ID was not found in token.");

                return Unauthorized(new
                {
                    succes = false,
                    message = "User identity not found."

             
                });
            }


            var result = await _questionService.CreateQuestionAsync(request,actorUserId);

            return Ok(result);

        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetQuestionById(int id)
        {
            _logger.LogInformation("Get question by ID request received. QuestionId: {QuestionId}", id);

            var result = await _questionService.GetQuestionByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Question not found."
                });
            }

            return Ok(result);
        }

        [HttpGet("exam/{examId:int}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetQuestionsByExamId(int examId)
        {
            _logger.LogInformation("Get questions by exam request received. ExamId: {ExamId}",examId);

            var result = await _questionService.GetQuestionsByExamIdAsync(examId);

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateQuestion(
            int id,
            [FromBody] CreateQuestionRequestDto request)
        {
            _logger.LogInformation("Update question request received. QuestionId: {QuestionId}",id);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                _logger.LogWarning("Update question failed because user ID was not found in token. QuestionId: {QuestionId}",id);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            var result = await _questionService.UpdateQuestionAsync(id,request,actorUserId);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            _logger.LogInformation("Delete question request received. QuestionId: {QuestionId}",id);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                _logger.LogWarning("Delete question failed because user ID was not found in token. QuestionId: {QuestionId}",id);

                return Unauthorized(new
                {
                    Success = false,
                    Message = "User identity not found."
                });
            }

            await _questionService.DeleteQuestionAsync(id,actorUserId);

            return Ok(new
            {
                Success = true,
                Message = "Question deleted successfully."
            });
        }

    }
}
