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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService courseService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public  async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }
            _logger.LogInformation( "Create course request received. CourseName: {CourseName}, DepartmentId: {DepartmentId}",
              request.Name,
              request.DepartmentId);

            var result = await _courseService.CreateCourseAsync(request, actorUserId);

            return CreatedAtAction( nameof(GetCourseById),new { id = result.Id },result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateCourse(int id,[FromBody] UpdateCourseRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation("Update course request received. CourseId: {CourseId}",id);

            var result = await _courseService.UpdateCourseAsync(request, actorUserId, id);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Course not found."
                });
            }

            return Ok(result);
        }



        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            _logger.LogInformation( "Get course request received. CourseId: {CourseId}",id);

            var result = await _courseService.GetCourseByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Course not found."
                });
            }

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation("Delete course request received. CourseId: {CourseId}",id);

            var result = await _courseService.DeleteCourseAsync(id, actorUserId);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Course not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Course deleted successfully."
            });
        }
    }
}
