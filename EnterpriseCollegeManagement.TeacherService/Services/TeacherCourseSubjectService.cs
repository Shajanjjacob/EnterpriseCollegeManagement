using EnterpriseCollegeManagement.TeacherService.Data;
using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;
using EnterpriseCollegeManagement.TeacherService.Entities;
using EnterpriseCollegeManagement.TeacherService.Exceptions;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Parsing;
using System.Text.Json;

namespace EnterpriseCollegeManagement.TeacherService.Services
{
    public class TeacherCourseSubjectService : ITeacherCourseSubjectService
    {
        private readonly IAuditServiceClient _auditServiceClient;
        private readonly IIdentityServiceClient _identityServiceClient;
        private readonly IAcademicServiceClient _academicServiceClient;
        private readonly IStudentServiceClient _studentServiceClient;
        private readonly TeacherDbContext _context;
        private readonly ILogger<TeacherCourseSubjectService> _logger;

        public TeacherCourseSubjectService(IAuditServiceClient auditServiceClient,
                                           IIdentityServiceClient identityServiceClient,
                                           IAcademicServiceClient academicServiceClient,
                                           IStudentServiceClient studentServiceClient,
                                           TeacherDbContext context,
                                           ILogger<TeacherCourseSubjectService> logger)
        {
            _auditServiceClient = auditServiceClient;
            _academicServiceClient = academicServiceClient;
            _studentServiceClient = studentServiceClient;
            _logger = logger;
            _identityServiceClient = identityServiceClient;
            _context = context;
        }
        


        public async Task<TeacherCourseSubjectResponseDto> AssignCourseSubjectAsync(AssignTeacherCourseSubjectRequestDto request, string actorUserId)
        {
            _logger.LogInformation("Assigning CourseSubject. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}, ActorUserId: {ActorUserId}",
               request.TeacherId,
               request.CourseSubjectId,
               actorUserId);

            var teacher = await _context.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(x=> x.Id == request.TeacherId && !x.IsDeleted && x.IsActive);

            if(teacher == null )
            {
                _logger.LogWarning("Teacher not found. TeacherId: {TeacherId}", request.TeacherId);

                throw new NotFoundException("Teacher not found.");
            }

            var courseSubject = await _academicServiceClient.GetCourseSubjectByIdAsync(request.CourseSubjectId);

            if(courseSubject == null )
            {
                _logger.LogWarning("CourseSubject not found. CourseSubjectId: {CourseSubjectId}", request.CourseSubjectId);

                throw new NotFoundException("CourseSubject not found.");
            }

            var course = await _academicServiceClient.GetCourseByIdAsync(courseSubject.CourseId);

            if(course == null)
            {
                _logger.LogWarning("Course not found. CourseId: {CourseId}", courseSubject.CourseId);

                throw new NotFoundException("Course not found.");
            }

            var subject =  await _academicServiceClient.GetSubjectByIdAsync(courseSubject.SubjectId);
            if(subject == null)
            {
                _logger.LogWarning("Department not found. DepartmentId: {DepartmentId}",course.DepartmentId);

                throw new NotFoundException("Department not found.");
            }


            var department = await _studentServiceClient.GetDepartmentByIdAsync(course.DepartmentId);
            if(department == null)
            {
                _logger.LogWarning("Department not found. DepartmentId: {DepartmentId}",course.DepartmentId);

                throw new NotFoundException("Department not found.");
            }

            var existingAssignment = await _context.TeacherCourseSubjects.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TeacherId == request.TeacherId && x.CourseSubjectId == request.CourseSubjectId && !x.IsDeleted);

            if (existingAssignment != null)
            {
                _logger.LogWarning("CourseSubject already assigned to Teacher. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}",request.TeacherId,
                request.CourseSubjectId);

                throw new ConflictException("This CourseSubject is already assigned to the teacher.");
            }

            var courseSubjectAssignment = new TeacherCourseSubject
            {
                TeacherId = request.TeacherId,
                CourseSubjectId = request.CourseSubjectId,
                AssignedDate = DateTime.UtcNow,
                CreatedBy = actorUserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.TeacherCourseSubjects.Add(courseSubjectAssignment);
            await _context.SaveChangesAsync();


            await _auditServiceClient.LogAuditAsync(actorUserId, "CREATE", "TeacherCourseSubject", courseSubjectAssignment.Id.ToString(), "CourseSubject assigned to teacher.",
                 null, JsonSerializer.Serialize(new
                 {
                     courseSubjectAssignment.Id,
                     courseSubjectAssignment.TeacherId,
                     courseSubjectAssignment.CourseSubjectId
                 }));

            return new TeacherCourseSubjectResponseDto
            {
                Id = courseSubjectAssignment.Id,
                TeacherId = courseSubjectAssignment.TeacherId,
                CourseSubjectId = courseSubjectAssignment.CourseSubjectId,

                CourseId = courseSubject.CourseId,
                CourseName = course.Name,

                SubjectId = courseSubject.SubjectId,
                SubjectName = courseSubject.SubjectName,

                Semester = courseSubject.Semester,

                DepartmentId = course.DepartmentId,
                DepartmentName = department.Name,

                AssignedDate = courseSubjectAssignment.AssignedDate
            };
        }

        public async Task<List<TeacherCourseSubjectResponseDto>> GetAssignedCourseSubjectsAsync(int teacherId)
        {
            _logger.LogInformation("Getting assigned CourseSubjects for Teacher. TeacherId: {TeacherId}",teacherId);

            var teacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == teacherId && !x.IsDeleted);
            if(teacher == null)
            {

            }

            var courseSubjects = await _context.TeacherCourseSubjects.AsNoTracking()
                .Where(x => x.TeacherId == teacherId && !x.IsDeleted).ToListAsync();

            var response  = new List<TeacherCourseSubjectResponseDto>();

            foreach(var courseSubject in courseSubjects)
            {
                var Teachercouresubject = await _academicServiceClient.GetCourseSubjectByIdAsync(courseSubject.CourseSubjectId);
                if(Teachercouresubject == null)
                {
                    _logger.LogWarning("CourseSubject not found in AcademicService. CourseSubjectId: {CourseSubjectId}",courseSubject.CourseSubjectId);

                    continue;
                }

                var course = await _academicServiceClient.GetCourseByIdAsync(Teachercouresubject.CourseId);
                if(course == null)
                {
                    _logger.LogWarning( "Course not found in AcademicService. CourseId: {CourseId}",Teachercouresubject.CourseId);

                    continue;
                }

                var subject = await _academicServiceClient.GetSubjectByIdAsync(Teachercouresubject.SubjectId);
                if(subject == null)
                {
                    _logger.LogWarning("Department not found in StudentService. DepartmentId: {DepartmentId}", Teachercouresubject.SubjectId);

                    continue;
                }

                var department = await _studentServiceClient.GetDepartmentByIdAsync(course.DepartmentId);
                if(department == null)
                {
                    _logger.LogWarning("Department not found in StudentService. DepartmentId: {DepartmentId}",course.DepartmentId);

                    continue;
                }

                var result = new TeacherCourseSubjectResponseDto
                {
                    Id = courseSubject.Id,
                    TeacherId = courseSubject.TeacherId,
                    CourseSubjectId = courseSubject.CourseSubjectId,

                    CourseId = course.Id,
                    CourseName = course.Name,

                    SubjectId = subject.Id,
                    SubjectName = subject.Name,

                    Semester = Teachercouresubject.Semester,

                    DepartmentId = course.DepartmentId,
                    DepartmentName = department.Name,

                    AssignedDate = courseSubject.AssignedDate
                };


                response.Add(result);
                
            }

            _logger.LogInformation("Assigned CourseSubjects retrieved successfully. TeacherId: {TeacherId}, Count: {Count}",teacherId, response.Count);

            return response;

        }

        public async Task RemoveCourseSubjectAsync(int teacherId, int courseSubjectId, string actorUserId)
        {
            _logger.LogInformation("Removing CourseSubject assignment. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}, ActorUserId: {ActorUserId}",
              teacherId,
              courseSubjectId,
              actorUserId);

          
            var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == teacherId && !x.IsDeleted);

            if (teacher == null)
            {
                _logger.LogWarning("Teacher not found. TeacherId: {TeacherId}",teacherId);

                throw new NotFoundException("Teacher not found.");
            }

            
            var assignment = await _context.TeacherCourseSubjects.FirstOrDefaultAsync(x =>  x.TeacherId == teacherId &&
                    x.CourseSubjectId == courseSubjectId && !x.IsDeleted);

            if (assignment == null)
            {
                _logger.LogWarning("CourseSubject assignment not found. TeacherId: {TeacherId}, CourseSubjectId: {CourseSubjectId}", teacherId,
                    courseSubjectId);

                throw new NotFoundException( "CourseSubject assignment not found.");
            }

           
            assignment.IsDeleted = true;
            assignment.DeletedBy = actorUserId;
            assignment.DeletedDate = DateTime.UtcNow;
            assignment.UpdatedBy = actorUserId;
            assignment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("CourseSubject assignment removed successfully. AssignmentId: {AssignmentId}",assignment.Id);

           
            await _auditServiceClient.LogAuditAsync(actorUserId,"DELETE", "TeacherCourseSubject",assignment.Id.ToString(),"CourseSubject assignment removed from teacher.",
                JsonSerializer.Serialize(new
                {
                    assignment.TeacherId,
                    assignment.CourseSubjectId
                }),
                null);
        }
    }
}
