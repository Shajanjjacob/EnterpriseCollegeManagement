using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class ExamAttendanceService : IExamAttendanceService
    {
        private readonly AcademicDbContext _context;
        private readonly ILogger<ExamAttendanceService> _logger;
        private readonly IStudentServiceClient _studentServiceClient;

        public ExamAttendanceService( AcademicDbContext context,ILogger<ExamAttendanceService> logger,IStudentServiceClient studentServiceClient)
        {
            _context = context;
            _logger = logger;
            _studentServiceClient = studentServiceClient;
        }

      

        public async Task<ExamAttendanceResponseDto> StartExamAsync(CreateExamAttendanceRequestDto request, string studentUserId)
        {
            _logger.LogInformation( "Start exam request received. ExamId: {ExamId}, StudentUserId: {StudentUserId}", request.ExamId,
               studentUserId);

            var student = await _studentServiceClient.GetStudentByUserIdAsync(studentUserId);

            if(student == null)
            {
                _logger.LogWarning("Student profile not found. UserId: {UserId}",studentUserId);

                throw new NotFoundException("Student profile not found.");
            }

            var exams = await _context.Exams
                .Include(c => c.courseSubject)
                .ThenInclude(c => c.Course).AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted);

            if(exams == null)
            {
                _logger.LogWarning( "Exam not found. ExamId: {ExamId}",request.ExamId);

                throw new NotFoundException("Exam not found.");
            }

            if (!exams.IsPublished)
            {
                _logger.LogWarning( "Student attempted to start an unpublished exam. ExamId: {ExamId}, StudentUserId: {StudentUserId}",
                   request.ExamId,
                   studentUserId);

                throw new BadRequestException("Exam is not published.");
            }

            if(student.CourseId != exams.courseSubject.Course.Id)
            {
                throw new BadRequestException("Student is not assigned to this course.");
            }

            if (student.DepartmentId != exams.courseSubject.Course.DepartmentId)
            {
                throw new BadRequestException("Student does not belong to the required department.");
            }

            if (student.Semester != exams.courseSubject.Semester)
            {
                throw new BadRequestException("Student is not eligible for this semester exam.");
            }



            var existingAttendance = await _context.ExamAttendances.AsNoTracking()
                .AnyAsync(x => x.ExamId == request.ExamId && x.StudentUserId == studentUserId);

            if(existingAttendance)
            {
                _logger.LogWarning("Student has already started this exam. ExamId: {ExamId}, StudentUserId: {StudentUserId}",
                   request.ExamId,
                   studentUserId);

                throw new ConflictException("Student has already started this exam.");
            }

            //changes related to autosubmit answer 

            var startedAt = DateTime.UtcNow;
            var expiresAt = startedAt.AddMinutes(exams.DurationMinutes);



            var studentattendance = new ExamAttendance
            {
                ExamId = exams.Id,
                StudentUserId = studentUserId,
                StartedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,  // auto submit 
                Score = 0,
                TotalMarks = exams.TotalMarks,
                IsSubmitted = false
            };

            _context.ExamAttendances.Add(studentattendance);

            await _context.SaveChangesAsync();

            return new ExamAttendanceResponseDto
            {
                Id = studentattendance.Id,
                ExamId = studentattendance.ExamId,
                StudentUserId = studentattendance.StudentUserId,
                StartedAt = studentattendance.StartedAt,
                ExpiresAt = studentattendance.ExpiresAt, /// auto submit 
                SubmittedAt = studentattendance.SubmittedAt,
                Score = studentattendance.Score,
                TotalMarks = studentattendance.TotalMarks,
                IsSubmitted = studentattendance.IsSubmitted
            };
            
        }

        public async Task<List<ExamQuestionResponseDto>> GetExamQuestionsAsync(int attendanceId, string studentUserId)
        {
            _logger.LogInformation("Get exam questions request received. AttendanceId: {AttendanceId}, StudentUserId: {StudentUserId}",
               attendanceId,
               studentUserId);


            var student = await _studentServiceClient.GetStudentByUserIdAsync(studentUserId);
            if(student == null)
            {
                _logger.LogWarning("Student profile not found. UserId: {UserId}",studentUserId);

                throw new NotFoundException("Student profile not found.");
            }

            var attendance = await _context.ExamAttendances.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == attendanceId);
            if(attendance == null)
            {
                _logger.LogWarning("Exam attendance not found. AttendanceId: {AttendanceId}", attendanceId);

                throw new NotFoundException("Exam attendance not found.");
            }

            if(attendance.StudentUserId != studentUserId)
            {
                _logger.LogWarning("Student attempted to access another student's attendance. AttendanceId: {AttendanceId}, StudentUserId: {StudentUserId}",
                  attendanceId,
                  studentUserId);

                throw new BadRequestException("This exam attendance does not belong to the current student.");
            }

            if (attendance.IsSubmitted)
            {
                _logger.LogWarning("Student attempted to access questions after submitting exam. AttendanceId: {AttendanceId}", attendanceId);

                throw new BadRequestException("Exam has already been submitted.");
            }

            var exam = await _context.Exams.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == attendance.ExamId && !x.IsDeleted);

            if(exam == null)
            {
                _logger.LogWarning("Exam not found for attendance. AttendanceId: {AttendanceId}, ExamId: {ExamId}", attendanceId,attendance.ExamId);

                throw new NotFoundException("Exam not found.");
            }

            if (!exam.IsPublished)
            {
                _logger.LogWarning("Student attempted to access questions for unpublished exam. ExamId: {ExamId}",exam.Id);

                throw new BadRequestException("Exam is not published.");
            }

                var questions = await _context.Questions.AsNoTracking()
                    .Where(x => x.ExamId == attendance.ExamId &&x.IsDeleted)
                    .OrderBy(x => x.CreatedDate)
                    .ThenBy(x => x.Id)
                    .ToListAsync();

            _logger.LogInformation("Retrieved {QuestionCount} questions for ExamId: {ExamId}, AttendanceId: {AttendanceId}",questions.Count, attendance.ExamId,
               attendanceId);

            return questions.Select(x => new ExamQuestionResponseDto
            {
                Id = x.Id,
                QuestionText = x.QuestionText,
                OptionA = x.OptionA,
                OptionB = x.OptionB,
                OptionC = x.OptionC,
                OptionD = x.OptionD,
                Marks = x.Marks

            }).ToList();
        }

        public async Task<ExamAttendanceResponseDto> SubmitExamAsync(string studentUserId, int attendanceId)
        {
            _logger.LogInformation("Submit exam request received. AttendanceId: {AttendanceId}, StudentUserId: {StudentUserId}",attendanceId,studentUserId);

            var attendance = await _context.ExamAttendances.FirstOrDefaultAsync(x => x.Id == attendanceId);
            if(attendance == null)
            {
                _logger.LogWarning("Exam attendance not found. AttendanceId: {AttendanceId}",attendanceId);

                throw new NotFoundException("Exam attendance not found.");

            }
            if (attendance.StudentUserId != studentUserId)
            {
                _logger.LogWarning("Student attempted to submit another student's exam. AttendanceId: {AttendanceId}, StudentUserId: {StudentUserId}",
                    attendanceId,
                    studentUserId);

                throw new BadRequestException("This exam attendance does not belong to the current student.");
            }

            if (attendance.IsSubmitted)
            {
                _logger.LogWarning("Exam already submitted. AttendanceId: {AttendanceId}", attendanceId);

                throw new ConflictException("Exam has already been submitted.");
            }

            //auto submit answer related 
            //Manual submission is not allowed after expiry.

            if (DateTime.UtcNow >= attendance.ExpiresAt)
            {
                await AutoSubmitExpiredExamAsync(attendance);

                _logger.LogWarning("Manual submission attempted after exam expiry. AttendanceId: {AttendanceId}",attendanceId);

                throw new BadRequestException("Exam time has expired.");
            }



            var examExists = await _context.Exams.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == attendance.ExamId && !x.IsDeleted);

            if (examExists == null)
            {
                _logger.LogWarning("Exam not found while submitting. ExamId: {ExamId}", attendance.ExamId);

                throw new NotFoundException("Exam not found.");
            }

            var totalQuestionsCount = await _context.Questions.AsNoTracking()
                .CountAsync(x => x.ExamId == attendance.ExamId && !x.IsDeleted);

            var AnswerQuestionCount = await _context.StudentAnswers
                .Where(x => x.ExamAttendanceId == attendance.Id && !x.Question.IsDeleted)
                .Select(x => x.QuestionId).Distinct().CountAsync();

            if (totalQuestionsCount != AnswerQuestionCount)
            {
                _logger.LogWarning( "Exam submission failed because not all questions were answered. AttendanceId: {AttendanceId}, TotalQuestions: {TotalQuestions}, AnsweredQuestions: {AnsweredQuestions}",
                      attendanceId,
                      totalQuestionsCount,
                      AnswerQuestionCount);

                throw new BadRequestException($"Please answer all questions before submitting. Answered {AnswerQuestionCount} of {totalQuestionsCount} questions.");

            }

            var totalScore = await _context.StudentAnswers
                .Where(x => x.ExamAttendanceId == attendance.Id).SumAsync(x => x.MarksAwarded);
                

            //update attendance table remain fields

            attendance.Score = totalScore;
            attendance.TotalMarks = examExists.TotalMarks;
            attendance.IsSubmitted = true;
            attendance.SubmittedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();


            _logger.LogInformation("Exam submitted successfully. AttendanceId: {AttendanceId}, StudentUserId: {StudentUserId}, Score: {Score}, TotalMarks: {TotalMarks},attendncesumitted: {IsSubmitted}",
                attendanceId,
                studentUserId,
                attendance.Score,
                attendance.TotalMarks,attendance.IsSubmitted);

          
            return new ExamAttendanceResponseDto
            {
                Id = attendance.Id,
                ExamId = attendance.ExamId,
                StudentUserId = attendance.StudentUserId,
                StartedAt = attendance.StartedAt,
                SubmittedAt = attendance.SubmittedAt,
                Score = attendance.Score,
                TotalMarks = attendance.TotalMarks,
                IsSubmitted = attendance.IsSubmitted
            };

        }

        public async Task<ExamAttendanceResponseDto> PublishResultAsync(int attendanceId, string actorUserId)
        {
            var attendance = await _context.ExamAttendances.FirstOrDefaultAsync(x => x.Id ==  attendanceId);
            if(attendance == null)
            {
                _logger.LogWarning("Exam attendance not found. AttendanceId: {AttendanceId}",attendanceId);

                throw new NotFoundException("Exam attendance not found.");
            }
            if (!attendance.IsSubmitted)
            {
                _logger.LogWarning("Cannot publish result because exam is not submitted. AttendanceId: {AttendanceId}",
           attendanceId);

                throw new BadRequestException("Exam must be submitted before publishing the result.");
            }

            if (attendance.IsResultPublished)
            {
                _logger.LogWarning("Result already published. AttendanceId: {AttendanceId}", attendanceId);

                throw new ConflictException("Result has already been published.");
            }

            attendance.IsResultPublished = true;
            attendance.ResultPublishedBy = actorUserId;
            attendance.ResultPublishedDate = DateTime.UtcNow;
           await _context.SaveChangesAsync();

            _logger.LogInformation("Result published successfully. AttendanceId: {AttendanceId}, ActorUserId: {ActorUserId}",attendanceId, actorUserId);


            return new ExamAttendanceResponseDto
            {
                Id = attendance.Id,
                ExamId = attendance.ExamId,
                StudentUserId = attendance.StudentUserId,
                StartedAt = attendance.StartedAt,
                SubmittedAt = attendance.SubmittedAt,
                Score = attendance.Score,
                TotalMarks = attendance.TotalMarks,
                IsSubmitted = attendance.IsSubmitted,
                IsResultPublished = attendance.IsResultPublished,
                ResultPublishedBy = attendance.ResultPublishedBy,
                ResultPublishedDate = attendance.ResultPublishedDate
            };
        }

        public async Task<StudentExamResultResponseDto?> GetStudentResultAsync(int examId, string studentUserId)
        {
            _logger.LogInformation("Get student result request received. ExamId: {ExamId}, StudentUserId: {StudentUserId}", examId, studentUserId);


            var attendance = await _context.ExamAttendances.AsNoTracking().FirstOrDefaultAsync(x => x.ExamId == examId && x.StudentUserId == studentUserId);
            if(attendance == null)
            {
                _logger.LogWarning("Exam attendance not found for student. ExamId: {ExamId}, StudentUserId: {StudentUserId}", examId, studentUserId);

                return null;

            }

            if (!attendance.IsSubmitted)
            {
                _logger.LogWarning("Student has not submitted the exam. ExamId: {ExamId}, StudentUserId: {StudentUserId}, AttendanceId: {AttendanceId}",
            examId,
            studentUserId,
            attendance.Id);

                throw new BadRequestException("Exam has not been submitted.");
            }
            if (!attendance.IsResultPublished)
            {
                _logger.LogWarning("Exam result has not been published. ExamId: {ExamId}, StudentUserId: {StudentUserId}, AttendanceId: {AttendanceId}",
          examId,
          studentUserId,
          attendance.Id);

                throw new BadRequestException("Exam result has not been published yet.");
            }

           

            var exam = await _context.Exams.Include(x => x.courseSubject)
                .ThenInclude(x => x.Subject)
                .Include(x => x.courseSubject)
                .ThenInclude(x => x.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == examId && x.Id == attendance.ExamId && !x.IsDeleted);

            if(exam == null)
            {
                _logger.LogWarning("Exam not found while getting result. ExamId: {ExamId}, AttendanceId: {AttendanceId}", examId,attendance.Id);

                throw new NotFoundException("Exam not found.");
            }

            var department = await _studentServiceClient.GetDepartmentByIdAsync(exam.courseSubject.Course.DepartmentId);

            if(department == null)
            {
                _logger.LogWarning("Department not found while getting student result. DepartmentId: {DepartmentId}, ExamId: {ExamId}, StudentUserId: {StudentUserId}",
            exam.courseSubject.Course.DepartmentId,
            examId,
            studentUserId);

                throw new NotFoundException("Department not found.");
            }

            return new StudentExamResultResponseDto
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                CourseName = exam.courseSubject.Course.Name,
                SubjectName = exam.courseSubject.Subject.Name,
                DepartmentName = department.Name,
                Semester = exam.courseSubject.Semester,
                Score = attendance.Score,
                TotalMarks = attendance.TotalMarks,
                SubmittedAt = attendance.SubmittedAt!.Value

            };

        }

        public async Task<List<ExamSubmissionResponseDto>> GetExamSubmissionsAsync(int examId)
        {
            _logger.LogInformation("Getting exam submissions. ExamId: {ExamId}",  examId);

            var examExists = await _context.Exams.AsNoTracking().AnyAsync(x => x.Id == examId && !x.IsDeleted);
            if(!examExists)
            {
                _logger.LogWarning("Exam submissions retrieval failed. Exam not found. ExamId: {ExamId}",examId);

                throw new NotFoundException("Exam not found.");
            }

            var attendances = await _context.ExamAttendances.AsNoTracking()
                .Where(x => x.ExamId == examId && x.IsSubmitted).ToListAsync();

            var result = new List<ExamSubmissionResponseDto>();

            foreach(var attendance in attendances)
            {
                var student = await _studentServiceClient.GetStudentByUserIdAsync(attendance.StudentUserId);

                if(student == null)
                {
                    _logger.LogWarning("Student profile not found for exam attendance. " + "ExamId: {ExamId}, StudentUserId: {StudentUserId}", examId,
                        attendance.StudentUserId);

                    continue;
                }

                var submission = new ExamSubmissionResponseDto
                {
                    AdmissionNumber = student.AdmissionNumber,
                    ExamSubmitted = attendance.IsSubmitted,
                    Score = attendance.Score,
                    TotalMarks = attendance.TotalMarks,
                    SubmittedDate = attendance.SubmittedAt,
                    ResultPublished = attendance.IsResultPublished,

                };

                result.Add(submission);

            }

            return result;


        }

        //auto-submit method for exams 

       private async Task AutoSubmitExpiredExamAsync(ExamAttendance  attendance)
        {
            _logger.LogInformation("Auto-submitting expired exam. AttendanceId: {AttendanceId}, ExamId: {ExamId}",attendance.Id,attendance.ExamId);

            var examexist = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == attendance.ExamId);

            if(examexist == null)
            {
                _logger.LogWarning("Auto-submit failed. Exam not found. ExamId: {ExamId}",attendance.ExamId);

                throw new NotFoundException("Exam not found.");
            }

            var totalscore = await _context.StudentAnswers.Where(x => x.ExamAttendanceId == attendance.Id).SumAsync(x => x.MarksAwarded);

            attendance.Score = totalscore;
            attendance.TotalMarks = examexist.TotalMarks;
            attendance.IsSubmitted = true;
            attendance.SubmittedAt = attendance.ExpiresAt;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Expired exam auto-submitted successfully. AttendanceId: {AttendanceId}, Score: {Score}, TotalMarks: {TotalMarks}",
                  attendance.Id,
                  attendance.Score,
                  attendance.TotalMarks);

        }

        public async Task ProcessExpiredExamsAsync()
        {
            _logger.LogInformation("Checking for expired exam attendances.");

            var expiredAttendances = await _context.ExamAttendances
                .Where(x => !x.IsSubmitted && x.ExpiresAt <= DateTime.UtcNow).ToListAsync();

            if(expiredAttendances.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Found {Count} expired exam attendances.",expiredAttendances.Count);

            foreach (var expiredAttendance in expiredAttendances)
            {
                if (expiredAttendance.IsSubmitted)
                {
                    continue;
                }

                await AutoSubmitExpiredExamAsync(expiredAttendance);
            }
        }
    }
}
