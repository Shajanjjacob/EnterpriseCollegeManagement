using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.StudentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }


        [HttpPost("create-profile")]

        public async Task<IActionResult> CreateStudentProfile([FromBody] CreateStudentProfileRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (string.IsNullOrEmpty(actorUserId))
            {
                _logger.LogWarning("Unable to identify authenticated Admin user.");

                return Unauthorized();
            }

            _logger.LogInformation("Create student profile request started. AdminUserId: {AdminUserId}, TargetUserId: {TargetUserId}",
                actorUserId,
                request.UserId);

            var result = await _studentService.CreateStudentAsync(request, actorUserId);

            _logger.LogInformation("Create student profile request completed. StudentId: {StudentId}",
               result.Id);

            return CreatedAtAction(
                nameof(GetStudentById),
                new { id = result.Id },
                result);

        }
        [HttpGet("{id:int}")]
       public  async Task<IActionResult> GetStudentById(int id)
        {
            _logger.LogInformation("Get student request started. StudentId: {StudentId}",id);

            var result = await _studentService.GetStudentByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning(
                    "Student profile not found. StudentId: {StudentId}",
                    id);

                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }

            _logger.LogInformation("Get student request completed. StudentId: {StudentId}", id);

            return Ok(result);
        }

    }
}
