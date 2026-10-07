using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.Exceptions;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.TeacherService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherCourseSubjectController : ControllerBase
    {
        private readonly ILogger<TeacherCourseSubjectController> _logger;
        private readonly ITeacherCourseSubjectService _teacherCourseSubjectService;

        public TeacherCourseSubjectController(ILogger<TeacherCourseSubjectController> logger, ITeacherCourseSubjectService teacherCourseSubjectService)
        {
            _logger = logger;
            _teacherCourseSubjectService = teacherCourseSubjectService;
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignCourseSubject([FromBody] AssignTeacherCourseSubjectRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var result = await _teacherCourseSubjectService.AssignCourseSubjectAsync(request, actorUserId);

            return Ok(new
            {
                Success = true,
                Message = "CourseSubject assigned to teacher successfully.",
                Data = result
            });
        }

        [HttpGet("teacher/{teacherId:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAssignedCourseSubjects(int teacherId)
        {
            _logger.LogInformation("Get assigned CourseSubjects request received. TeacherId: {TeacherId}",teacherId);

            var result = await _teacherCourseSubjectService.GetAssignedCourseSubjectsAsync(teacherId);

            return Ok(new
            {
                Success = true,
                Message = "Assigned CourseSubjects retrieved successfully.",
                Data = result
            });
        }

        [HttpDelete("teacher/{teacherId:int}/course-subject/{courseSubjectId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveCourseSubject(int teacherId, int courseSubjectId)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            await _teacherCourseSubjectService.RemoveCourseSubjectAsync(teacherId,courseSubjectId,actorUserId);

            return Ok(new
            {
                Success = true,
                Message = "CourseSubject assignment removed successfully."
            });
        }
    }
}

