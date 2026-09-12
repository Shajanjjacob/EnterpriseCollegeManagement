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

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            _logger.LogInformation("Get student request started. StudentId: {StudentId}", id);

            var result = await _studentService.GetStudentByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning( "Student profile not found. StudentId: {StudentId}", id);

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
        public async Task<IActionResult> UploadProfilePhoto( [FromForm] UploadProfilePhotoRequestDto request)
        {
            _logger.LogInformation("UploadProfilePhoto endpoint reached.");



            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning( "Unable to identify authenticated Student user.");

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

            var extension = Path.GetExtension(request.Photo.FileName) .ToLowerInvariant();

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

            if(existingStudent == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Student profile not found."
                });
            }


            var oldPhotoUrl = existingStudent.ProfilePhotoUrl;


            var fileName = $"{Guid.NewGuid()}{extension}";

            var folder = Path.Combine( _environment.WebRootPath, "uploads", "students");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filePath = Path.Combine(folder, fileName);

            await using (var stream =new FileStream(filePath, FileMode.Create))
            {
                await request.Photo.CopyToAsync(stream);
            }

            var profilePhotoUrl = $"/uploads/students/{fileName}";

            try
            {
                // Update database
                var result = await _studentService.UploadProfilePhotoAsync( userId,profilePhotoUrl);

                //delete old phote after change done only 
                if (!string.IsNullOrEmpty(oldPhotoUrl))
                {
                    var oldFileName =Path.GetFileName(oldPhotoUrl);

                    var oldFilePath = Path.Combine( folder,oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);

                        _logger.LogInformation( "Old profile photo deleted. UserId: {UserId}", userId);
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
                _logger.LogWarning( "Unable to identify authenticated Student user.");

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


    }
}
