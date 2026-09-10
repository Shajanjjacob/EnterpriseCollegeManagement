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
        public async Task<IActionResult> GetStudentById(int id)
        {
           
            return Ok();
        }

    }
}
