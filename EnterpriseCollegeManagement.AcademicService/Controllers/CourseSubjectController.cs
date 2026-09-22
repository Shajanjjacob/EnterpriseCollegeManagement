using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCollegeManagement.AcademicService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseSubjectController : ControllerBase
    {
        private readonly ICourseSubjectService _courseSubjectService;
        private readonly ILogger<CourseSubjectController> _logger;

        public CourseSubjectController(ICourseSubjectService courseSubjectService, ILogger<CourseSubjectController> logger)
        {
            _courseSubjectService = courseSubjectService;
            _logger = logger;
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> AssignSubjectToCourse([FromBody] CreateCourseSubjectRequestDto request)
        {
            _logger.LogInformation("Assign subject to course request received. CourseId: {CourseId}, SubjectId: {SubjectId}",
                request.CourseId,
                request.SubjectId);

            var result = await _courseSubjectService.AssignSubjectToCourseAsync(request);

            return Ok(result);
        }

       
        [HttpGet("course/{courseId:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetSubjectsByCourseId(int courseId)
        {
            _logger.LogInformation("Get subjects by course request received. CourseId: {CourseId}",courseId);

            var result = await _courseSubjectService.GetSubjectsByCourseIdAsync(courseId);

            return Ok(result);
        }

       
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> RemoveSubjectFromCourse(int id)
        {
            _logger.LogInformation("Remove subject from course request received. CourseSubjectId: {CourseSubjectId}",id);

            var result = await _courseSubjectService.RemoveSubjectFromCourseAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Course-subject assignment not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Subject removed from course successfully."
            });
        }
    }
}
