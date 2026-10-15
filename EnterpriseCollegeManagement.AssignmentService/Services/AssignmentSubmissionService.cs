using EnterpriseCollegeManagement.AssignmentService.Data;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;
using EnterpriseCollegeManagement.AssignmentService.Entities;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;

namespace EnterpriseCollegeManagement.AssignmentService.Services
{
    public class AssignmentSubmissionService : IAssignmentSubmissionService
    {
        private readonly ILogger<AssignmentSubmissionService> _logger;
        private readonly AssignmentDbContext _context;
        private readonly IStudentServiceClient _studentServiceClient;
        private readonly IAuditServiceClient _auditServiceClient;
        private readonly IAcademicServiceClient _academicServiceClient;
        private readonly IFileStorageService _fileStorageService;

        public AssignmentSubmissionService(ILogger<AssignmentSubmissionService> logger, AssignmentDbContext context,
                                                IStudentServiceClient studentServiceClient,
                                            IAuditServiceClient auditServiceClient, IAcademicServiceClient academicServiceClient,
                                            IFileStorageService fileStorageService )
        {
            _context = context;
            _logger = logger;
            _auditServiceClient = auditServiceClient;
            _studentServiceClient = studentServiceClient;
            _academicServiceClient = academicServiceClient;
            _fileStorageService = fileStorageService;
        }

        public async Task<AssignmentSubmissionResponseDto> SubmitAssignmentAsync(SubmitAssignmentRequestDto request, string studentUserId)
        {
            _logger.LogInformation("Assignment submission started. UserId: {UserId}, AssignmentId: {AssignmentId}",studentUserId,request.AssignmentId);

            if (string.IsNullOrWhiteSpace(request.SubmissionText) && (request.File == null || request.File.Length == 0))
            {
                _logger.LogWarning("Submission content is missing. UserId: {UserId}, AssignmentId: {AssignmentId}",  studentUserId,  request.AssignmentId);

                throw new ArgumentException("Provide submission text or upload a PDF.");
            }

            _logger.LogInformation("Fetching assignment. AssignmentId: {AssignmentId}", request.AssignmentId);

            var assignment = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == request.AssignmentId && !x.IsDeleted);
            if(assignment == null)
            {
                _logger.LogWarning("Assignment not found. AssignmentId: {AssignmentId}", request.AssignmentId);

                throw new KeyNotFoundException("Assignment not found.");
            }
            if (!assignment.IsPublished)
            {
                _logger.LogWarning("Assignment is not published. AssignmentId: {AssignmentId}",assignment.Id);
                throw new InvalidOperationException("This assignment is not published.");
            }
            if(DateTime.UtcNow > assignment.DueDate)
            {
                _logger.LogWarning("Assignment deadline has passed. AssignmentId: {AssignmentId}",assignment.Id);


                throw new InvalidOperationException("The submission deadline has passed.");
            }
            _logger.LogInformation("Fetching student profile. UserId: {UserId}", studentUserId);

            var student = await  _studentServiceClient.GetStudentByUserIdAsync(studentUserId);
            if(studentUserId == null)
            {
                _logger.LogWarning("Student profile not found. UserId: {UserId}", studentUserId);

                _logger.LogWarning("Student profile not found. UserId: {UserId}",studentUserId);


                throw new KeyNotFoundException("Student profile not found.");
            }

            _logger.LogInformation("Fetching course-subject details. CourseSubjectId: {CourseSubjectId}", assignment.CourseSubjectId);

            var courseSubject = await _academicServiceClient.CourseSubjectExistsAsync(assignment.CourseSubjectId);
            if(courseSubject == null)
            {
                _logger.LogWarning("Course-subject information not found. CourseSubjectId: {CourseSubjectId}",assignment.CourseSubjectId);
                throw new InvalidOperationException("Course subject information not found.");
            }

            if(student.CourseId != courseSubject.CourseId || student.Semester != courseSubject.Semester)
            {
                _logger.LogWarning("Student is not eligible for assignment. UserId: {UserId}, AssignmentId: {AssignmentId}", studentUserId, assignment.Id);


                throw new InvalidOperationException("You are not eligible to submit this assignment.");
            }

            var alreadySubmitted = await _context.AssignmentSubmissions
                .AnyAsync(x => x.AssignmentId == assignment.Id && x.StudentUserId == studentUserId &&!x.IsDeleted);
            if(alreadySubmitted)
            {
                _logger.LogWarning("Duplicate submission rejected. UserId: {UserId}, AssignmentId: {AssignmentId}", studentUserId,assignment.Id);
                throw new InvalidOperationException("You have already submitted this assignment.");

            }
            string? filePath = null;

            if (request.File != null && request.File.Length > 0)
            {
                _logger.LogInformation("Saving submission PDF. UserId: {UserId}, AssignmentId: {AssignmentId}", studentUserId,
                     assignment.Id);


                filePath = await _fileStorageService.SavePdfAsync(request.File);
                _logger.LogInformation("Submission PDF saved successfully. AssignmentId: {AssignmentId}", assignment.Id);
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = assignment.Id,
                StudentUserId = studentUserId,
                SubmissionText = request.SubmissionText,
                FileUrl = filePath,
                SubmittedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = studentUserId
            };

            _context.AssignmentSubmissions.Add(submission);

            //save file to database

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Assignment submission saved. SubmissionId: {SubmissionId}, UserId: {UserId}",
                submission.Id,
                studentUserId);


            await _auditServiceClient.LogAuditAsync(
                    studentUserId,
                    "Submit",
                    "AssignmentSubmission",
                    submission.Id.ToString(),
                    $"Student submitted assignment {assignment.Id}.",
                    null,
                    JsonSerializer.Serialize(new
                    {
                        submission.AssignmentId,
                        submission.StudentUserId,
                        submission.FileUrl,
                        submission.SubmittedAt
                    }));

            _logger.LogInformation("Audit event recorded. SubmissionId: {SubmissionId}",  submission.Id);


            return new AssignmentSubmissionResponseDto
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentUserId = submission.StudentUserId,
                SubmissionText = submission.SubmissionText,
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt,
                MarksObtained = submission.MarksObtained,
                Feedback = submission.Feedback,
                GradedAt = submission.GradedAt
            };


        }

        public async Task<List<AssignmentSubmissionResponseDto>> GetMySubmissionsAsync(string studentUserId)
        {
            _logger.LogInformation("Fetching assignment submissions for student. UserId: {UserId}", studentUserId);

            if (string.IsNullOrWhiteSpace(studentUserId))
            {
                _logger.LogWarning("Cannot fetch submissions because StudentUserId is empty.");

                throw new ArgumentException("Student user ID is required.",nameof(studentUserId));
            }
            var submissions = await _context.AssignmentSubmissions.AsNoTracking()
                .Where(x => x.StudentUserId == studentUserId && !x.IsDeleted)
                .OrderBy(x => x.SubmittedAt)
                .Select(x => new AssignmentSubmissionResponseDto
                {
                    Id = x.Id,
                    AssignmentId = x.AssignmentId,
                    StudentUserId = x.StudentUserId,
                    SubmissionText = x.SubmissionText,
                    FileUrl = x.FileUrl,
                    SubmittedAt = x.SubmittedAt,
                    MarksObtained = x.MarksObtained,
                    Feedback = x.Feedback,
                    GradedAt = x.GradedAt
                }).ToListAsync();

            _logger.LogInformation("Retrieved {SubmissionCount} submissions for student. UserId: {UserId}",submissions.Count, studentUserId);

            return submissions;
        }

        public async Task<AssignmentSubmissionResponseDto> GetSubmissionByIdAsync(int id, string currentUserId, bool isAdmin)
        {
            //ADMIN,TEACHER,STUDENT can use this function 

            _logger.LogInformation("Fetching submission {SubmissionId} by user {UserId}. IsAdmin: {IsAdmin}", id,currentUserId, isAdmin);

           
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("Cannot fetch submission because user ID is empty.");

                throw new ArgumentException("User ID is required.");
            }

          //passing sumisssion id 
            var submission = await _context.AssignmentSubmissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (submission == null)
            {
                _logger.LogWarning("Submission {SubmissionId} not found.", id);

                throw new KeyNotFoundException("Submission not found.");
            }

          
            var assignment = await _context.Assignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == submission.AssignmentId &&  !x.IsDeleted);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found for submission {SubmissionId}.", submission.AssignmentId,id);

                throw new KeyNotFoundException("Assignment not found.");
            }

         
            if (!isAdmin &&  submission.StudentUserId != currentUserId &&  assignment.TeacherUserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} is not authorized to access submission {SubmissionId}.", currentUserId, id);

                throw new UnauthorizedAccessException("You are not authorized to view this submission.");
            }

            
            var result = new AssignmentSubmissionResponseDto
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentUserId = submission.StudentUserId,
                SubmissionText = submission.SubmissionText,
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt,
                MarksObtained = submission.MarksObtained,
                Feedback = submission.Feedback,
                GradedAt = submission.GradedAt
            };

            _logger.LogInformation("Submission {SubmissionId} retrieved successfully.",  id);

            return result;



        }

        public async Task<List<AssignmentSubmissionResponseDto>> GetSubmissionsByAssignmentAsync(int assignmentId, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation("Fetching submissions for assignment {AssignmentId} by user {UserId}. IsAdmin: {IsAdmin}",
                assignmentId,
                currentUserId,
                isAdmin);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("Cannot fetch assignment submissions because user ID is empty.");

                throw new ArgumentException("User ID is required.", nameof(currentUserId));

            }

            var assignment = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == assignmentId && !x.IsDeleted);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} was not found.", assignmentId);

                throw new KeyNotFoundException("Assignment not found.");
            }

            if (!isAdmin && assignment.TeacherUserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} is not authorized to view submissions for assignment {AssignmentId}.", currentUserId, assignmentId);

                throw new UnauthorizedAccessException("You are not authorized to view these submissions.");
            }

            var submissions = await _context.AssignmentSubmissions.AsNoTracking()
                .Where(x => x.AssignmentId == assignmentId && !x.IsDeleted)
                .OrderBy(x => x.SubmittedAt)
                .Select(x => new AssignmentSubmissionResponseDto
                {
                    Id = x.Id,
                    AssignmentId = x.AssignmentId,
                    StudentUserId = x.StudentUserId,
                    SubmissionText = x.SubmissionText,
                    FileUrl = x.FileUrl,
                    SubmittedAt = x.SubmittedAt,
                    MarksObtained = x.MarksObtained,
                    Feedback = x.Feedback,
                    GradedAt = x.GradedAt
                }).ToListAsync();

            _logger.LogInformation("Retrieved {SubmissionCount} submissions for assignment {AssignmentId}.", submissions.Count, assignmentId);

            return submissions;

        }

        public async Task<AssignmentSubmissionResponseDto> GradeSubmissionAsync(int id, GradeSubmissionRequestDto request, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation("Grading started. SubmissionId: {SubmissionId}, UserId: {UserId}",id, currentUserId);

            if(string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("Cannot grade submission because user ID is empty.");

                throw new ArgumentException("User ID is required.");
            }

            var submission = await _context.AssignmentSubmissions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(submission == null)
            {
                _logger.LogWarning("Submission not found. SubmissionId: {SubmissionId}", id);

                throw new KeyNotFoundException("Submission not found.");
            }

            var assignment = await _context.Assignments.FirstOrDefaultAsync(x => x.Id == submission.AssignmentId && !x.IsDeleted);
            if(assignment == null)
            {
                _logger.LogWarning("Assignment not found for submission {SubmissionId}.", id);

                throw new KeyNotFoundException("Assignment not found.");
            }

            if(!isAdmin && assignment.TeacherUserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} is not authorized to grade submission {SubmissionId}.", currentUserId,id);

                throw new UnauthorizedAccessException("You are not authorized to grade this submission.");
            }

            if(request.MarksObtained > assignment.MaxMarks || request.MarksObtained < 0)
            {
                _logger.LogWarning("Invalid marks {MarksObtained} for submission {SubmissionId}. Maximum marks: {MaxMarks}", request.MarksObtained,  id,
                 assignment.MaxMarks);

                throw new ArgumentException($"Marks must be between 0 and {assignment.MaxMarks}.");
            }

            var oldValues = JsonSerializer.Serialize(new
            {
                submission.MarksObtained,
                submission.Feedback,
                submission.GradedAt
            });

            submission.MarksObtained = request.MarksObtained;
            submission.Feedback = request.Feedback;
            submission.GradedAt = DateTime.UtcNow;
            submission.UpdatedDate = DateTime.UtcNow;
            submission.UpdatedBy = currentUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Submission graded successfully. SubmissionId: {SubmissionId}, Marks: {MarksObtained}", id, submission.MarksObtained);

            var newValues = JsonSerializer.Serialize(new
            {
                submission.MarksObtained,
                submission.Feedback,
                submission.GradedAt
            });

            await _auditServiceClient.LogAuditAsync(
                        currentUserId,
                        "Grade",
                        "AssignmentSubmission",
                        submission.Id.ToString(),
                        $"Submission {submission.Id} was graded.",
                        oldValues,
                        newValues);

            _logger.LogInformation("Grading completed. SubmissionId: {SubmissionId}, GradedBy: {UserId}", id,currentUserId);

          
            return new AssignmentSubmissionResponseDto
            {
                Id = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentUserId = submission.StudentUserId,
                SubmissionText = submission.SubmissionText,
                FileUrl = submission.FileUrl,
                SubmittedAt = submission.SubmittedAt,
                MarksObtained = submission.MarksObtained,
                Feedback = submission.Feedback,
                GradedAt = submission.GradedAt
            };
        }
       
       
    }
}
