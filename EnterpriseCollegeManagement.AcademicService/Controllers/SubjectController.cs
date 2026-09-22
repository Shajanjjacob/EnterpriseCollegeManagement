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
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly ILogger<SubjectController> _logger;

        public SubjectController(ISubjectService subjectService, ILogger<SubjectController> logger)
        {
            _logger = logger;
            _subjectService = subjectService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation("Create subject request received. SubjectName: {SubjectName}, DepartmentId: {DepartmentId}", request.Name, request.DepartmentId);

            var result = await _subjectService.CreateSubjectAsync(request, actorUserId);

            return CreatedAtAction(
                nameof(GetSubjectById),
                new { id = result.Id },
                result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation("Update subject request received. SubjectId: {SubjectId}", id);

            var result = await _subjectService.UpdateSubjectAsync(id, request, actorUserId);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Subject not found."
                });
            }

            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation("Delete subject request received. SubjectId: {SubjectId}", id);

            var result = await _subjectService.DeleteSubjectAsync(actorUserId, id);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Subject not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Subject deleted successfully."
            });
        }




        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            _logger.LogInformation("Get subject request received. SubjectId: {SubjectId}", id);

            var result = await _subjectService.GetSubjectByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Subject not found."
                });
            }

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAllSubjects()
        {
            _logger.LogInformation("Get all subjects request received.");

            var result = await _subjectService
                .GetAllSubjectsAsync();

            return Ok(result);
        }
    }
}

