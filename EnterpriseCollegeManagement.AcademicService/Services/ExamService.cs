using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class ExamService : IExamService
    {
        private readonly ILogger<ExamService> _logger;
        private readonly AcademicDbContext _context;

        public ExamService(ILogger<ExamService> logger, AcademicDbContext context)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<ExamResponseDto> CreateExamAsync(CreateExamRequestDto request, string actorUserId)
        {
            _logger.LogInformation( "Creating exam. Title: {Title}, CourseSubjectId: {CourseSubjectId}, ActorUserId: {ActorUserId}",
              request.Title,
              request.CourseSubjectId,
              actorUserId);

            if(request.TotalMarks <= 0)
            {
                _logger.LogWarning(  "Exam creation failed. Invalid TotalMarks: {TotalMarks}", request.TotalMarks);

                throw new BadRequestException("Total marks must be greater than zero.");
            }
            if(request.DurationMinutes <= 0)
            {
                _logger.LogWarning(  "Exam creation failed. Invalid DurationMinutes: {DurationMinutes}",request.DurationMinutes);
                throw new BadRequestException("Duration must be greater than zero.");
            }


            var coursesubjectexists = await _context.CoursesSubjects
                .Include(x => x.Subject)
                .Include(x => x.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CourseSubjectId && !x.Course.IsDeleted && !x.Subject.IsDeleted);
            if(coursesubjectexists == null)
            {
                _logger.LogWarning( "Exam creation failed. CourseSubject not found. CourseSubjectId: {CourseSubjectId}", request.CourseSubjectId);

                throw new NotFoundException("Course subject not found.");
            }

            var exam = new Exam
            {
                CourseSubjectId = request.CourseSubjectId,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DurationMinutes = request.DurationMinutes,
                TotalMarks = request.TotalMarks,
                IsPublished = false,
                CreatedBy = actorUserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false

            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam created successfully. ExamId: {ExamId}, Title: {Title}",exam.Id,exam.Title);
            return new ExamResponseDto
            {
                Id = exam.Id,
                CourseSubjectId = exam.CourseSubjectId,
                CourseName = coursesubjectexists.Course.Name,
                SubjectName = coursesubjectexists.Subject.Name,
                Semester = coursesubjectexists.Semester,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                TotalMarks = exam.TotalMarks,
                IsPublished = exam.IsPublished,
                CreatedDate = exam.CreatedDate,
                UpdatedDate = exam.UpdatedDate

            };


        }

        public async Task<bool> DeleteExamAsync(int id, string actorUserId)
        {
            _logger.LogInformation("Deleting exam. ExamId: {ExamId}, ActorUserId: {ActorUserId}", id, actorUserId);

            var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(exam == null)
            {
                _logger.LogWarning( "Exam deletion failed. Exam not found. ExamId: {ExamId}",id);

                throw new NotFoundException( "Exam not found.");
            }
            exam.IsDeleted = true;
            exam.DeletedBy = actorUserId;
            exam.DeletedDate = DateTime.UtcNow;


          
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam soft deleted successfully. ExamId: {ExamId}, DeletedBy: {DeletedBy}", exam.Id,actorUserId);

            return true;
        }

        public async Task<List<ExamResponseDto>> GetAllExamsAsync()
        {
            _logger.LogInformation("Getting all exams.");

            var exam = await _context.Exams.Include(x => x.courseSubject)
                .ThenInclude(x => x.Course)
                .Include(x => x.courseSubject)
                .ThenInclude(x => x.Subject)
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x=> x.Id)
                .ToListAsync();

            return exam.Select(x => new ExamResponseDto
            {
                Id = x.Id,
                CourseSubjectId = x.CourseSubjectId,
                CourseName = x.courseSubject.Course.Name,
                SubjectName = x.courseSubject.Subject.Name,
                Semester = x.courseSubject.Semester,
                Title = x.Title,
                Description = x.Description,
                DurationMinutes = x.DurationMinutes,
                TotalMarks = x.TotalMarks,
                IsPublished = x.IsPublished,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            }).ToList();
        }

        public async Task<ExamResponseDto?> GetExamByIdAsync(int id)
        {
            _logger.LogInformation("Getting exam by ID. ExamId: {ExamId}",id);

            var exam = await _context.Exams.Include(x => x.courseSubject)
                .ThenInclude(x => x.Course)
                .Include(x => x.courseSubject)
                .ThenInclude(x => x.Subject)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if(exam == null)
            {
                _logger.LogWarning( "Exam not found. ExamId: {ExamId}",id);

                return null;
            }

            return new ExamResponseDto
            {
                Id = exam.Id,
                CourseSubjectId = exam.CourseSubjectId,
                CourseName = exam.courseSubject.Course.Name,
                SubjectName = exam.courseSubject.Subject.Name,
                Semester = exam.courseSubject.Semester,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                TotalMarks = exam.TotalMarks,
                IsPublished = exam.IsPublished,
                CreatedDate = exam.CreatedDate,
                UpdatedDate = exam.UpdatedDate
            };
        }

        public async Task<ExamResponseDto> UpdateExamAsync(int id, CreateExamRequestDto request, string actorUserId)
        {
            _logger.LogInformation( "Updating exam. ExamId: {ExamId}, ActorUserId: {ActorUserId}",id, actorUserId);



            if (request.TotalMarks <= 0)
            {
                _logger.LogWarning("Exam update failed. Invalid TotalMarks: {TotalMarks}",request.TotalMarks);

                throw new BadRequestException("Total marks must be greater than zero.");
            }
            if (request.DurationMinutes <= 0)
            {
                _logger.LogWarning( "Exam update failed. Invalid DurationMinutes: {DurationMinutes}",request.DurationMinutes);

                throw new BadRequestException("Duration must be greater than zero.");
            }


            var coursesubjectexists = await _context.CoursesSubjects
                .Include(x => x.Subject)
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Id == request.CourseSubjectId && !x.Subject.IsDeleted && !x.Course.IsDeleted);
               

            if (coursesubjectexists == null)
            {
                _logger.LogWarning( "Exam update failed. CourseSubject not found. CourseSubjectId: {CourseSubjectId}",request.CourseSubjectId);
                throw new NotFoundException("Course subject not found.");
            }

            var exam = await _context.Exams.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (exam == null)
            {
                _logger.LogWarning( "Exam update failed. Exam not found. ExamId: {ExamId}",id);

                throw new NotFoundException("Exam not found.");
            }

            exam.CourseSubjectId = request.CourseSubjectId;
            exam.Title = request.Title.Trim();
            exam.Description = request.Description?.Trim();
            exam.DurationMinutes = request.DurationMinutes;
            exam.TotalMarks = request.TotalMarks;

            exam.UpdatedBy = actorUserId;
            exam.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(  "Exam updated successfully. ExamId: {ExamId}", exam.Id);

            return new ExamResponseDto
            {
                Id = exam.Id,
                CourseSubjectId = exam.CourseSubjectId,
                CourseName = coursesubjectexists.Course.Name,
                SubjectName = coursesubjectexists.Subject.Name,
                Semester = coursesubjectexists.Semester,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                TotalMarks = exam.TotalMarks,
                IsPublished = exam.IsPublished,
                CreatedDate = exam.CreatedDate,
                UpdatedDate = exam.UpdatedDate
            };
        }
    }
}
