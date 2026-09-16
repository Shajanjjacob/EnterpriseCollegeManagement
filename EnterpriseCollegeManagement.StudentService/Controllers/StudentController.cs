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
    //[Authorize(Roles = "Admin")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;
        private readonly IWebHostEnvironment _environment;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger, IWebHostEnvironment environment)
        {
            _studentService = studentService;
            _logger = logger;
            _environment = environment;
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            _logger.LogInformation("Get student request started. StudentId: {StudentId}", id);

            var result = await _studentService.GetStudentByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Student profile not found. StudentId: {StudentId}", id);

                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }

            _logger.LogInformation("Get student request completed. StudentId: {StudentId}", id);

            return Ok(result);
        }


        [HttpPost("me/profile-photo")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadProfilePhoto([FromForm] UploadProfilePhotoRequestDto request)
        {
            _logger.LogInformation("UploadProfilePhoto endpoint reached.");



            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unable to identify authenticated Student user.");

                return Unauthorized();
            }

            if (request.Photo == null || request.Photo.Length == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Profile photo is required."
                });
            }

            var allowedExtensions = new[]
                    {
                ".jpg",
                ".jpeg",
                ".png"
            };

            var extension = Path.GetExtension(request.Photo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Only JPG, JPEG and PNG images are allowed."
                });
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (request.Photo.Length > maxFileSize)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Profile photo must be less than 5 MB."
                });
            }


            //USER PROFILE BASED 
            var existingStudent = await _studentService.GetMyProfileAsync(userId);

            if (existingStudent == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }


            var oldPhotoUrl = existingStudent.ProfilePhotoUrl;


            var fileName = $"{Guid.NewGuid()}{extension}";

            var folder = Path.Combine(_environment.WebRootPath, "uploads", "students");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filePath = Path.Combine(folder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Photo.CopyToAsync(stream);
            }

            var profilePhotoUrl = $"/uploads/students/{fileName}";

            try
            {
                // Update database
                var result = await _studentService.UploadProfilePhotoAsync(userId, profilePhotoUrl);

                //delete old phote after change done only 
                if (!string.IsNullOrEmpty(oldPhotoUrl))
                {
                    var oldFileName = Path.GetFileName(oldPhotoUrl);

                    var oldFilePath = Path.Combine(folder, oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);

                        _logger.LogInformation("Old profile photo deleted. UserId: {UserId}", userId);
                    }
                }

                return Ok(result);
            }
            catch
            {
                //update fails remove new phote
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                throw;
            }

        }

        [HttpGet("me")]
        [Authorize(Roles = "Student")]

        public async Task<IActionResult> GetMyProfileAsync(string userId)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unable to identify authenticated Student user.");

                return Unauthorized();
            }

            var result = await _studentService.GetMyProfileAsync(userId);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }

            return Ok(result);
        }


        [HttpPut("me")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStudentProfileRequestDto request)
        {
            _logger.LogInformation("Update my profile request started.");


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unable to identify authenticated Student user.");

                return Unauthorized();
            }

            var result = await _studentService.UpdateMyProfileAsync(userId, request);

            if (result == null)
            {
                _logger.LogWarning("Student profile not found. UserId: {UserId}", userId);

                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }

            _logger.LogInformation("Update my profile request completed. UserId: {UserId}", userId);

            return Ok(result);
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] AdminUpdateStudentRequestDto request)
        {
            _logger.LogInformation("Admin student update request started. StudentId: {StudentId}", id);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                _logger.LogWarning("Unable to identify authenticated Admin user.");

                return Unauthorized();
            }

            var result = await _studentService.UpdateStudentAsync(id, request, actorUserId);

            if (result == null)
            {
                _logger.LogWarning("Student profile not found. StudentId: {StudentId}", id);

                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }

            _logger.LogInformation("Admin student update request completed. StudentId: {StudentId}, AdminUserId: {AdminUserId}", id, actorUserId);

            return Ok(result);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllStudents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Get all students request started. PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

            if (pageNumber <= 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Page number must be greater than 0."
                });
            }

            if (pageSize <= 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Page size must be greater than 0."
                });
            }
            var result = await _studentService.GetAllStudentsAsync(pageNumber, pageSize);

            _logger.LogInformation("Get all students request completed. PageNumber: {PageNumber}, PageSize: {PageSize}, TotalCount: {TotalCount}", pageNumber,
                pageSize,
                result.TotalCount);

            return Ok(result);
        }


        [HttpGet("search")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> SearchStudents([FromQuery] StudentSearchRequestDto request)
        {
            _logger.LogInformation("Get all students request started.");

            if (request.PageNumber <= 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Page number must be greater than 0."
                });
            }

            if (request.PageSize <= 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Page size must be greater than 0."
                });
            }

            if (request.PageSize > 100)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Page size cannot exceed 100."
                });
            }

            var result = await _studentService.SearchStudentsAsync(request);

            _logger.LogInformation("Get all students request completed. TotalCount: {TotalCount}",result.TotalCount);

            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            _logger.LogInformation( "Delete student request started. StudentId: {StudentId}", id);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                _logger.LogWarning( "Unable to identify authenticated Admin user.");

                return Unauthorized();
            }

            var result = await _studentService.DeleteStudentAsync(id,actorUserId);

            if (!result)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }

            _logger.LogInformation("Delete student request completed. StudentId: {StudentId}",id);

            return Ok(new
            {
                Success = true,
                Message = "Student profile deleted successfully."
            });
        }
    }
}
