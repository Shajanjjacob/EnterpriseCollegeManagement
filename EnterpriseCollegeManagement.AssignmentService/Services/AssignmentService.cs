using EnterpriseCollegeManagement.AssignmentService.Data;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;
using EnterpriseCollegeManagement.AssignmentService.Entities;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Parsing;
using System.Text.Json;

namespace EnterpriseCollegeManagement.AssignmentService.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ILogger<AssignmentService> _logger;
        private readonly AssignmentDbContext _context;
        private readonly IAcademicServiceClient _academicServiceClient;
        private readonly ITeacherServiceClient _teacherServiceClient;
        private readonly IAuditServiceClient _auditServiceClient;

        public AssignmentService(ILogger<AssignmentService> logger,
                                 AssignmentDbContext context,
                                 IAcademicServiceClient academicServiceClient,
                                 ITeacherServiceClient teacherServiceClient,
                                 IAuditServiceClient auditServiceClient)
        {
            _academicServiceClient = academicServiceClient;
            _context = context;
            _auditServiceClient= auditServiceClient;
            _logger = logger;
            _teacherServiceClient = teacherServiceClient;
        }
        public async Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentRequestDto request, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation("Creating assignment. UserId: {UserId}, CourseSubjectId: {CourseSubjectId}", currentUserId,request.CourseSubjectId);

            string teacherUserId;

            if (isAdmin)
            {
                if (!string.IsNullOrWhiteSpace(request.TeacherUserId))
                {
                    throw new ArgumentException("TeacherUserId is required for Admin.");
                }
                teacherUserId = request.TeacherUserId;
            }
            else
            {
                teacherUserId = currentUserId;
            }
         
            //get teacher profile id from teacherservice 

            var teacherId = await _teacherServiceClient.GetTeacherIdByUserIdAsync(teacherUserId);
            if(teacherId == null)
            {
                _logger.LogWarning("Teacher profile not found. UserId: {UserId}",teacherUserId);

                throw new KeyNotFoundException("Teacher profile not found.");
            }
            var courseSubject = await _academicServiceClient.CourseSubjectExistsAsync(request.CourseSubjectId);
            if(courseSubject == null)
            {
                _logger.LogWarning("CourseSubject not found. CourseSubjectId: {CourseSubjectId}", request.CourseSubjectId);

                throw new KeyNotFoundException("CourseSubject not found.");
            }

            // teacher assign to this coursesubject or not 
            var isAssigned = await _teacherServiceClient.IsTeacherAssignedToCourseSubjectAsync(teacherId.Value, request.CourseSubjectId);
            if (!isAssigned)
            {
                _logger.LogWarning("Teacher is not assigned to CourseSubject. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}", teacherId.Value, request.CourseSubjectId);

                throw new InvalidOperationException("Teacher is not assigned to this CourseSubject.");
            }

            var assignment = new Assignment
            {
                Title = request.Title,
                Description = request.Description,
                CourseSubjectId = request.CourseSubjectId,
                TeacherUserId = teacherUserId,
                DueDate = request.DueDate,
                MaxMarks = request.MaxMarks,
                IsPublished = false,
                CreatedBy = currentUserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation( "Assignment saved successfully. AssignmentId: {AssignmentId}", assignment.Id);

            await _auditServiceClient.LogAuditAsync(currentUserId, "Create", "Assignment", assignment.Id.ToString(), "Assignment created successfully.",
                 null,
                 JsonSerializer.Serialize(assignment));


            var response = new AssignmentResponseDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                CourseSubjectId = assignment.CourseSubjectId,
                TeacherUserId = assignment.TeacherUserId,
                DueDate = assignment.DueDate,
                MaxMarks = assignment.MaxMarks,
                IsPublished = assignment.IsPublished,
                CreatedDate = assignment.CreatedDate
            };

            _logger.LogInformation("Assignment creation completed. AssignmentId: {AssignmentId}", assignment.Id);

            return response;

        }

        public async Task<AssignmentResponseDto> UpdateAssignmentAsync(int id, UpdateAssignmentRequestDto request, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation("Updating assignment. AssignmentId: {AssignmentId}, UserId: {UserId}", id, currentUserId);

            var assignment = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(assignment == null)
            {
                _logger.LogWarning("Assignment not found. AssignmentId: {AssignmentId}", id);

                throw new KeyNotFoundException("Assignment not found.");
            }

            //if created by one teacher so he/she can update it (admin also can do)

            if(!isAdmin && assignment.TeacherUserId != currentUserId)
            {
                _logger.LogWarning("Unauthorized assignment update. AssignmentId: {AssignmentId}, UserId: {UserId}",id, currentUserId);

                throw new UnauthorizedAccessException("You are not allowed to update this assignment.");
            }

            string teacherUserId;

            if (isAdmin)
            {
                if (string.IsNullOrWhiteSpace(request.TeacherUserId))
                {
                    throw new ArgumentException("TeacherUserId is required when an Admin updates an assignment.");
                }
                teacherUserId = request.TeacherUserId;
            }
            else
            {
                teacherUserId = currentUserId;
            }

            var teacherId = await _teacherServiceClient.GetTeacherIdByUserIdAsync(teacherUserId);
            if(teacherId == null)
            {
                _logger.LogWarning("Teacher profile not found. UserId: {UserId}", teacherUserId);

                throw new KeyNotFoundException("Teacher profile not found.");
            }

            var courseSubject = await _academicServiceClient.CourseSubjectExistsAsync(request.CourseSubjectId);
            if(courseSubject == null)
            {
                _logger.LogWarning("CourseSubject not found. CourseSubjectId: {CourseSubjectId}", request.CourseSubjectId);

                throw new KeyNotFoundException("CourseSubject not found.");
            }

            var isTeacherAssign = await _teacherServiceClient.IsTeacherAssignedToCourseSubjectAsync(teacherId.Value, request.CourseSubjectId);
            if(!isTeacherAssign)
            {
                throw new InvalidOperationException("Teacher is not assigned to this CourseSubject.");
            }

            //if student start submission then need lock update 

            var hasSubmissions = await _context.AssignmentSubmissions.AnyAsync(x => x.AssignmentId == id && !x.IsDeleted);

            if (hasSubmissions)
            {
                _logger.LogWarning( "Assignment update blocked because submission records exist. AssignmentId: {AssignmentId}", id);

                throw new InvalidOperationException("Assignment cannot be updated because student submission records exist.");
            }

            var oldValue = JsonSerializer.Serialize(new
            {
                assignment.Title,
                assignment.Description,
                assignment.CourseSubjectId,
                assignment.TeacherUserId,
                assignment.DueDate,
                assignment.MaxMarks,
                assignment.IsPublished
            });

            //update
            assignment.Title = request.Title;
            assignment.Description = request.Description;
            assignment.CourseSubjectId = request.CourseSubjectId;
            assignment.TeacherUserId = teacherUserId;
            assignment.DueDate = request.DueDate;
            assignment.MaxMarks = request.MaxMarks;
            assignment.UpdatedBy = currentUserId;
            assignment.UpdatedDate = DateTime.UtcNow;

           await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment updated successfully. AssignmentId: {AssignmentId}",  assignment.Id);

            var newValue = JsonSerializer.Serialize(new
            {
                assignment.Title,
                assignment.Description,
                assignment.CourseSubjectId,
                assignment.TeacherUserId,
                assignment.DueDate,
                assignment.MaxMarks,
                assignment.IsPublished
            });

            await _auditServiceClient.LogAuditAsync(
                 currentUserId,
                "UPDATE",
                "Assignment",
                assignment.Id.ToString(),
                "Assignment updated successfully.",
                oldValue,
                newValue);

            return new AssignmentResponseDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                CourseSubjectId = assignment.CourseSubjectId,
                TeacherUserId = assignment.TeacherUserId,
                DueDate = assignment.DueDate,
                MaxMarks = assignment.MaxMarks,
                IsPublished = assignment.IsPublished,
                CreatedDate = assignment.CreatedDate
            };

        }

        public async Task<bool> DeleteAssignmentAsync(int id, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation("Deleting assignment. AssignmentId: {AssignmentId}, UserId: {UserId}", id, currentUserId);

            
            var assignment = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment not found. AssignmentId: {AssignmentId}", id);

                throw new KeyNotFoundException("Assignment not found.");
            }

            
            if (!isAdmin && assignment.TeacherUserId != currentUserId)
            {
                _logger.LogWarning("Unauthorized assignment deletion. AssignmentId: {AssignmentId}, UserId: {UserId}",  id, currentUserId);

                throw new UnauthorizedAccessException("You are not allowed to delete this assignment.");
            }

            //anyone start submit assignment ?
            var hasSubmissions = await _context.AssignmentSubmissions.AnyAsync(x => x.AssignmentId == id && !x.IsDeleted);

            if (hasSubmissions)
            {
                _logger.LogWarning("Assignment deletion blocked because submission records exist. AssignmentId: {AssignmentId}",id);

                throw new InvalidOperationException("Assignment cannot be deleted because student submission records exist.");
            }

           
            var oldValues = JsonSerializer.Serialize(new
            {
                assignment.Title,
                assignment.Description,
                assignment.CourseSubjectId,
                assignment.TeacherUserId,
                assignment.IsPublished,
                assignment.IsDeleted
            });

          
            assignment.IsDeleted = true;
            assignment.DeletedBy = currentUserId;
            assignment.DeletedDate = DateTime.UtcNow;
            assignment.UpdatedBy = currentUserId;
            assignment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment soft-deleted successfully. AssignmentId: {AssignmentId}",assignment.Id);

           
            var newValues = JsonSerializer.Serialize(new
            {
                assignment.Title,
                assignment.Description,
                assignment.CourseSubjectId,
                assignment.TeacherUserId,
                assignment.IsPublished,
                assignment.IsDeleted
            });

            
            await _auditServiceClient.LogAuditAsync(
                currentUserId,
                "DELETE",
                "Assignment",
                assignment.Id.ToString(),
                "Assignment soft-deleted successfully.",
                oldValues,
                newValues);

            return true;
        }

        public async Task<AssignmentResponseDto> GetAssignmentByIdAsync(int id)
        {
            _logger.LogInformation("Getting assignment by ID. AssignmentId: {AssignmentId}", id);

            var assignment = await _context.Assignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment not found. AssignmentId: {AssignmentId}", id);

                throw new KeyNotFoundException("Assignment not found.");
            }

            return new AssignmentResponseDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                CourseSubjectId = assignment.CourseSubjectId,
                TeacherUserId = assignment.TeacherUserId,
                DueDate = assignment.DueDate,
                MaxMarks = assignment.MaxMarks,
                IsPublished = assignment.IsPublished,
                CreatedDate = assignment.CreatedDate
            };
        }

        public async Task<List<AssignmentResponseDto>> GetMyAssignmentsAsync(string currentUserId, bool isAdmin)
        {
            //for admin  show all assignemts
            var query = _context.Assignments.AsNoTracking().Where(x => !x.IsDeleted);

            if (!isAdmin)
            {
                query = query.Where(x => x.TeacherUserId == currentUserId);
            }
            var assignments = await query.OrderBy(x => x.CreatedDate).ToListAsync();

            _logger.LogInformation("Retrieved {Count} assignments. UserId: {UserId}, IsAdmin: {IsAdmin}",  assignments.Count, currentUserId, isAdmin);

            return assignments.Select(x => new AssignmentResponseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CourseSubjectId = x.CourseSubjectId,
                TeacherUserId = x.TeacherUserId,
                DueDate = x.DueDate,
                MaxMarks = x.MaxMarks,
                IsPublished = x.IsPublished,
                CreatedDate = x.CreatedDate

            }).ToList();
        }

        public async Task<AssignmentResponseDto> PublishAssignmentAsync(int id,PublishAssignmentRequestDto request, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation("Updating assignment publish status. AssignmentId: {AssignmentId}, UserId: {UserId}", id, currentUserId);
            var assignment  = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(assignment == null)
            {
                _logger.LogWarning("Assignment not found. AssignmentId: {AssignmentId}", id);

                throw new KeyNotFoundException("Assignment not found.");
            }

            //who created 
            if(!isAdmin && assignment.TeacherUserId != currentUserId)
            {
                _logger.LogWarning("Unauthorized publish status update. AssignmentId: {AssignmentId}, UserId: {UserId}",id, currentUserId);

                throw new UnauthorizedAccessException("You are not allowed to publish this assignment.");
            }

            var oldValues = JsonSerializer.Serialize(new
            {
                assignment.IsPublished
            });

            //update 
            assignment.IsPublished = request.IsPublished;
            assignment.UpdatedBy = currentUserId;
            assignment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment publish status updated. AssignmentId: {AssignmentId}, IsPublished: {IsPublished}", id, assignment.IsPublished);


            var newValues = JsonSerializer.Serialize(new
            {
                assignment.IsPublished
            });

            await _auditServiceClient.LogAuditAsync(currentUserId,
                    "UPDATE",
                    "Assignment",
                    assignment.Id.ToString(),
                    request.IsPublished
                        ? "Assignment published successfully."
                        : "Assignment unpublished successfully.",
                    oldValues,
                    newValues);


            return new AssignmentResponseDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                CourseSubjectId = assignment.CourseSubjectId,
                TeacherUserId = assignment.TeacherUserId,
                DueDate = assignment.DueDate,
                MaxMarks = assignment.MaxMarks,
                IsPublished = assignment.IsPublished,
                CreatedDate = assignment.CreatedDate
            };
        }


    }
}
