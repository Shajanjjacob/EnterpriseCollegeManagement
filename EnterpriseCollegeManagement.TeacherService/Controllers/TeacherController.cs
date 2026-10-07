using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.Exceptions;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using EnterpriseCollegeManagement.TeacherService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.TeacherService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(ITeacherService teacherService, ILogger<TeacherController> logger)
        {
            _logger = logger;
            _teacherService = teacherService;
        }

        [HttpPost("create-profile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherProfileRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var result = await _teacherService.CreateTeacherAsync(request,actorUserId);

            return Ok(new
            {
                Success = true,
                Message = "Teacher profile created successfully.",
                Data = result
            });
        }

        [HttpPut("me")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTeacherProfileRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var result = await _teacherService.UpdateMyProfileAsync(request,userId);

            return Ok(new
            {
                Success = true,
                Message = "Teacher profile updated successfully.",
                Data = result
            });
        }

        [HttpGet("me")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var result = await _teacherService.GetMyProfileAsync(userId);

            return Ok(new
            {
                Success = true,
                Message = "Teacher profile retrieved successfully.",
                Data = result
            });
        }

        [HttpPost("me/profile-photo")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UploadProfilePhoto([FromForm] UploadProfilePhotoRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            if (request.Photo == null || request.Photo.Length == 0)
            {
                throw new BadRequestException("Profile photo is required.");
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
                throw new BadRequestException("Only JPG, JPEG and PNG images are allowed.");
            }

            if (request.Photo.Length > 5 * 1024 * 1024)
            {
                throw new BadRequestException("Profile photo size cannot exceed 5 MB.");
            }

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "teachers");

            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(uploadFolder,fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Photo.CopyToAsync(stream);
            }

            var profilePhotoUrl =$"/uploads/teachers/{fileName}";

            var result = await _teacherService.UploadProfilePhotoAsync(profilePhotoUrl,userId);

            return Ok(new
            {
                Success = true,
                Message = "Profile photo uploaded successfully.",
                Data = result
            });
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher( string userId, [FromBody] AdminUpdateTeacherRequestDto request)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(actorUserId))
            {
                throw new UnauthorizedException("User identity could not be determined.");
            }

            var result = await _teacherService.UpdateTeacherAsync(request, userId, actorUserId);

            return Ok(new
            {
                Success = true,
                Message = "Teacher profile updated successfully.",
                Data = result
            });
        }
    }
}
