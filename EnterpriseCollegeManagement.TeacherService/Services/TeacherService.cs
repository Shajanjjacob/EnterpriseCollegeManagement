using EnterpriseCollegeManagement.TeacherService.Data;
using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;
using EnterpriseCollegeManagement.TeacherService.Entities;
using EnterpriseCollegeManagement.TeacherService.Exceptions;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EnterpriseCollegeManagement.TeacherService.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IIdentityServiceClient _identityServiceClient;
        private readonly IAcademicServiceClient _academicServiceClient;
        private readonly IStudentServiceClient _studentServiceClient;
        private readonly TeacherDbContext _context;
        private readonly ILogger<TeacherService> _logger;
        private readonly IAuditServiceClient _auditServiceClient;

        public TeacherService(IIdentityServiceClient identityServiceClient,
                              IAcademicServiceClient academicServiceClient,
                              IStudentServiceClient studentServiceClient,
                              IAuditServiceClient auditServiceClient,
                              TeacherDbContext context,
                              ILogger<TeacherService> logger)
        {
            _identityServiceClient = identityServiceClient;
            _academicServiceClient = academicServiceClient;
            _studentServiceClient = studentServiceClient;
            _auditServiceClient = auditServiceClient;
            _logger = logger;
            _context = context;
        }


        public async Task<TeacherResponseDto> CreateTeacherAsync(CreateTeacherProfileRequestDto request, string actorUserId)
        {
            _logger.LogInformation("Creating teacher profile for UserId: {UserId}",request.UserId);

            var user = await _identityServiceClient.GetUserByIdAsync(request.UserId);

            if(user == null)
            {
                _logger.LogWarning("Cannot create teacher profile. User not found in IdentityService. UserId: {UserId}", request.UserId);

                throw new NotFoundException($"User with ID '{request.UserId}' was not found.");
            }

            if (!string.Equals(user.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Cannot create teacher profile. User does not have Teacher role. UserId: {UserId}, Role: {Role}",request.UserId,user.Role);

                throw new BadRequestException( "The selected user does not have the Teacher role.");
            }

            var existingTeacher = await _context.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsDeleted);

            if(existingTeacher != null)
            {
                _logger.LogWarning("Teacher profile already exists. UserId: {UserId}, TeacherId: {TeacherId}", request.UserId, existingTeacher.Id);

                throw new ConflictException("Teacher profile already exists for this user.");
            }

            var teacher = new Teacher
            {
                UserId = request.UserId,
                EmployeeCode = request.EmployeeCode,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,

                IsActive = true,

                CreatedBy = actorUserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher profile created successfully. TeacherId: {TeacherId}",teacher.Id);

            await _auditServiceClient.LogAuditAsync(actorUserId, "CREATE", "Teacher", teacher.Id.ToString(), "Teacher profile created successfully.",
                null,
                JsonSerializer.Serialize(new  //converted into json formatt
                {
                    teacher.Id,
                    teacher.UserId,
                    teacher.EmployeeCode,
                    teacher.FirstName,
                    teacher.LastName,
                    teacher.Phone
                }));

            return new TeacherResponseDto
            {
                Id = teacher.Id,
                UserId = teacher.UserId,
                EmployeeCode = teacher.EmployeeCode,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Phone = teacher.Phone,
                ProfilePhotoUrl = teacher.ProfilePhotoUrl,
                IsActive = teacher.IsActive
            };
        }

        public async Task<TeacherResponseDto?> GetMyProfileAsync(string userId)
        {
            _logger.LogInformation("Fetching teacher profile. UserId: {UserId}", userId);

            var teacher = await _context.Teachers.FirstOrDefaultAsync(te => te.UserId == userId && !te.IsDeleted);
            if(teacher == null)
            {
                _logger.LogWarning("Teacher profile not found. UserId: {UserId}",userId);

                throw new NotFoundException("Teacher profile not found.");
            }

            _logger.LogInformation("Teacher profile retrieved successfully. TeacherId: {TeacherId}, UserId: {UserId}",teacher.Id,userId);



            return new TeacherResponseDto
            {
                Id = teacher.Id,
                UserId = teacher.UserId,
                EmployeeCode = teacher.EmployeeCode,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Phone = teacher.Phone,
                ProfilePhotoUrl = teacher.ProfilePhotoUrl,
                IsActive = teacher.IsActive
            };


        }

        public async Task<TeacherResponseDto> UpdateMyProfileAsync(UpdateTeacherProfileRequestDto request, string userId)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
            _logger.LogInformation("Updating teacher profile. UserId: {UserId}",userId);


            if (teacher == null)
            {
                _logger.LogWarning("Teacher profile not found. UserId: {UserId}", userId);

                throw new NotFoundException("Teacher profile not found.");
            }

            //storeing old values in audit table 

            var oldValues = JsonSerializer.Serialize(new
            {
                teacher.FirstName,
                teacher.LastName,
                teacher.Phone
            });

            teacher.FirstName = request.FirstName;
            teacher.LastName = request.LastName;
            teacher.Phone = request.Phone;
            teacher.UpdatedBy = userId;
            teacher.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher profile updated successfully. TeacherId: {TeacherId}, UserId: {UserId}", teacher.Id, userId);

            var newValues = JsonSerializer.Serialize(new
            {
                teacher.FirstName,
                teacher.LastName,
                teacher.Phone
            });

            await _auditServiceClient.LogAuditAsync(userId, "UPDATE","Teacher", teacher.Id.ToString(), "Teacher profile updated successfully.",
                        oldValues,
                        newValues);

            return new TeacherResponseDto
            {
                Id = teacher.Id,
                UserId = teacher.UserId,
                EmployeeCode = teacher.EmployeeCode,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Phone = teacher.Phone,
                ProfilePhotoUrl = teacher.ProfilePhotoUrl,
                IsActive = teacher.IsActive
            };

        }

        public async Task<TeacherResponseDto> UpdateTeacherAsync(AdminUpdateTeacherRequestDto request, string userId, string actorUserId)
        {
            _logger.LogInformation("Updating teacher profile. UserId: {UserId}, ActorUserId: {ActorUserId}",userId, actorUserId);

            var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
            if(teacher == null)
            {
                _logger.LogWarning("Teacher profile not found. UserId: {UserId}",userId);

                throw new NotFoundException("Teacher profile not found.");
            }

            var oldValues = JsonSerializer.Serialize(new
            {
                teacher.EmployeeCode,
                teacher.FirstName,
                teacher.LastName,
                teacher.Phone,
                teacher.IsActive
            });

            teacher.EmployeeCode = request.EmployeeCode;
            teacher.FirstName = request.FirstName;
            teacher.LastName = request.LastName;
            teacher.Phone = request.Phone;
            teacher.IsActive = request.IsActive;
            teacher.UpdatedBy = actorUserId;
            teacher.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var newValues = JsonSerializer.Serialize(new
            {
                teacher.EmployeeCode,
                teacher.FirstName,
                teacher.LastName,
                teacher.Phone,
                teacher.IsActive
            });

            await _auditServiceClient.LogAuditAsync(actorUserId, "UPDATE", "Teacher", teacher.Id.ToString(),"Teacher profile updated by Admin.",oldValues,
            newValues);

            _logger.LogInformation("Teacher profile updated successfully. TeacherId: {TeacherId}, ActorUserId: {ActorUserId}",teacher.Id, actorUserId);

            return new TeacherResponseDto
            {
                Id = teacher.Id,
                UserId = teacher.UserId,
                EmployeeCode = teacher.EmployeeCode,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Phone = teacher.Phone,
                ProfilePhotoUrl = teacher.ProfilePhotoUrl,
                IsActive = teacher.IsActive
            };

            
        }

        public async Task<TeacherResponseDto> UploadProfilePhotoAsync(string profilePhotoUrl, string userId)
        {
            _logger.LogInformation("Updating teacher profile photo. UserId: {UserId}",userId);

            var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

            if (teacher == null)
            {
                _logger.LogWarning("Teacher profile not found while updating profile photo. UserId: {UserId}",userId);

                throw new NotFoundException("Teacher profile not found.");
            }

            var oldPhotoUrl = teacher.ProfilePhotoUrl;

            teacher.ProfilePhotoUrl = profilePhotoUrl;
            teacher.UpdatedBy = userId;
            teacher.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditServiceClient.LogAuditAsync(userId,"UPDATE","Teacher",teacher.Id.ToString(), "Teacher profile photo updated.", oldPhotoUrl,
                profilePhotoUrl);

            _logger.LogInformation("Teacher profile photo updated successfully. TeacherId: {TeacherId}",teacher.Id);

            return new TeacherResponseDto
            {
                Id = teacher.Id,
                UserId = teacher.UserId,
                EmployeeCode = teacher.EmployeeCode,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Phone = teacher.Phone,
                ProfilePhotoUrl = teacher.ProfilePhotoUrl,
                IsActive = teacher.IsActive
            };
        }
    }
}
