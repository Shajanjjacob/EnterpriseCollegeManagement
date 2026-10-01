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
    public class QuizGenerationController : ControllerBase
    {
        private readonly IQuizGenerationService _quizGenerationService;
        private readonly ILogger<QuizGenerationController> _logger;

        public QuizGenerationController(IQuizGenerationService quizGenerationService,ILogger<QuizGenerationController> logger)
        {
            _quizGenerationService = quizGenerationService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequestDto request)
        {
            _logger.LogInformation("AI quiz generation request received. ExamId: {ExamId}",request.ExamId);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User identity not found."
                });
            }

            var result = await _quizGenerationService.GenerateQuizAsync(request,actorUserId);

            _logger.LogInformation("AI quiz generation completed successfully. ExamId: {ExamId}, UserId: {UserId}",request.ExamId,actorUserId);

            return Ok(result);
        }
    }
}
