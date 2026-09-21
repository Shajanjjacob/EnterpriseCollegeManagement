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
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation("Create department request received.");

            var result = await _departmentService.CreateDepartmentAsync( request,actorUserId);

            return CreatedAtAction(
                nameof(GetDepartmentById),
                new { id = result.Id },
                result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> GetAllDepartments()
        {
            _logger.LogInformation("Get all departments request received.");

            var result = await _departmentService
                .GetAllDepartmentsAsync();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles = "Admin,Teacher,Student")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            _logger.LogInformation("Get department request received. DepartmentId: {DepartmentId}", id);

            var result = await _departmentService .GetDepartmentByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Department not found."
                });
            }

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment(int id,[FromBody] UpdateDepartmentRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation( "Update department request received. DepartmentId: {DepartmentId}",id);

            var result = await _departmentService.UpdateDepartmentAsync(request,actorUserId,id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Department not found."
                });
            }

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                return Unauthorized("User identity not found.");
            }

            _logger.LogInformation("Delete department request received. DepartmentId: {DepartmentId}",id);

            var result = await _departmentService.DeleteDepartmentAsync(id,actorUserId);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Department not found."
                });
            }

            return Ok(new
            {
                message = "Department deleted successfully."
            });
        }


    }
}
