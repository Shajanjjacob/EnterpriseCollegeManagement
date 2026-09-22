using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class CourseSubjectService : ICourseSubjectService
    {
        private readonly AcademicDbContext _context;
        private readonly ILogger<CourseSubjectService> _logger;

        public CourseSubjectService(AcademicDbContext context, ILogger<CourseSubjectService> logger)
        {
            _context = context;
            _logger = logger;

        }
        public async Task<CourseSubjectResponseDto> AssignSubjectToCourseAsync(CreateCourseSubjectRequestDto request)
        {
            _logger.LogInformation("Assign subject to course started. CourseId: {CourseId}, SubjectId: {SubjectId}", request.CourseId,request.SubjectId);

            var subjectExists = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == request.SubjectId && !x.IsDeleted);
            if(subjectExists == null)
            {
                _logger.LogWarning( "Subject not found. SubjectId: {SubjectId}", request.SubjectId);

                throw new NotFoundException("Subject not found.");
            }

            var courseExists = await _context.Courses.FirstOrDefaultAsync(x => x.Id == request.CourseId && !x.IsDeleted);
            if(courseExists == null)
            {
                _logger.LogWarning( "Course not found. CourseId: {CourseId}", request.CourseId);

                throw new NotFoundException("Course not found.");
            }

            if (request.Semester <= 0)
            {
                throw new BadRequestException("Semester must be greater than zero.");
            }

            var courseSubjectExist = await _context.CoursesSubjects
                .AnyAsync(x => x.CourseId == request.CourseId && x.SubjectId == request.SubjectId);

            if (courseSubjectExist)
            {
                _logger.LogWarning("Subject already assigned to course. CourseId: {CourseId}, SubjectId: {SubjectId}", request.CourseId,request.SubjectId);

                throw new ConflictException("Subject is already assigned to this course.");
            }

            var courseSubject = new CourseSubject
            {
                SubjectId = request.SubjectId,
                CourseId = request.CourseId,
                Semester = request.Semester
                
            };

            _context.CoursesSubjects.Add(courseSubject);
            await _context.SaveChangesAsync();

            _logger.LogInformation( "Subject assigned successfully. CourseSubjectId: {CourseSubjectId}",courseSubject.Id);

            var response = new CourseSubjectResponseDto
            {
                Id = courseSubject.Id,
                CourseId = courseExists.Id,
                CourseName = courseExists.Name,
                SubjectId = subjectExists.Id,
                SubjectName = subjectExists.Name,
                Semester = courseSubject.Semester

            };

            return response;
        }

        public async Task<List<CourseSubjectResponseDto>> GetSubjectsByCourseIdAsync(int courseId)
        {

            _logger.LogInformation("Getting subjects for course. CourseId: {CourseId}",courseId);

            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == courseId && !x.IsDeleted);
            if(course == null)
            {
                throw new NotFoundException("Course not found.");
            }
            var courseSubjects = await _context.CoursesSubjects.Include(s => s.Subject)
                .AsNoTracking().Where(x => x.CourseId == courseId)
                .OrderBy(x => x.Id)
                .ToListAsync();

            var response = courseSubjects.Select(x => new CourseSubjectResponseDto
            {
                Id = x.Id,
                CourseId = course.Id,
                CourseName = course.Name,
                SubjectId = x.SubjectId,
                SubjectName = x.Subject.Name
            }).ToList();

            return response;
        }

        public async Task<bool> RemoveSubjectFromCourseAsync(int id)
        {
            _logger.LogInformation("Remove subject from course started. CourseSubjectId: {CourseSubjectId}",id);

            var courseSubjects = await _context.CoursesSubjects.FirstOrDefaultAsync(x => x.Id == id);

            if( courseSubjects == null)
            {
                _logger.LogWarning("CourseSubject assignment not found. CourseSubjectId: {CourseSubjectId}",id);
                return false;

            }

            _context.CoursesSubjects.Remove(courseSubjects);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Subject removed from course successfully. CourseSubjectId: {CourseSubjectId}", id);
            return true;
        }
    }
}
