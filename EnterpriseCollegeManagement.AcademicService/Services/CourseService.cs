
using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class CourseService : ICourseService
    {
        private readonly AcademicDbContext _context;
        private readonly ILogger<CourseService> _logger;
        private readonly IStudentServiceClient _studentServiceClient;
      

        public CourseService(AcademicDbContext context, ILogger<CourseService> logger, IStudentServiceClient studentServiceClient)
        {
            _context = context;
            _logger = logger;
            _studentServiceClient = studentServiceClient;
            
        }


        public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request, string actorUserId)
        {
            _logger.LogInformation(
                 "Course creation started. CourseName: {CourseName}, DepartmentId: {DepartmentId}, ActorUserId: {ActorUserId}",
                 request.Name,
                 request.DepartmentId,
                 actorUserId);

            //service to service communication 

            var department = await _studentServiceClient.GetDepartmentByIdAsync(request.DepartmentId);

            if(department == null)
            {
                _logger.LogWarning( "Course creation failed. Department not found. DepartmentId: {DepartmentId}",request.DepartmentId);

                throw new NotFoundException("Department not found.");
            }

            var codeExists = await _context.Courses.AnyAsync(x => x.Code == request.Code && !x.IsDeleted);

            if (codeExists)
            {
                _logger.LogWarning( "Course creation failed. Course code already exists. Code: {Code}", request.Code);

                throw new ConflictException("Course code already exists.");
            }

            var course = new Course
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                DurationYears = request.DurationYears,
                DepartmentId = request.DepartmentId,
                CreatedBy = actorUserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation( "Course created successfully. CourseId: {CourseId}, DepartmentId: {DepartmentId}",
               course.Id,
               course.DepartmentId);

            var response = new CourseResponse
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                DurationYears = course.DurationYears,
                DepartmentId = course.DepartmentId,
                DepartmentName = department.Name
            };

            response.DepartmentName = department.Name;
            return response;
        }

        public async Task<bool> DeleteCourseAsync(int id, string actorUserId)
        {
            _logger.LogInformation("Course deletion started. CourseId: {CourseId}, ActorUserId: {ActorUserId}", id,actorUserId);

            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == id &&!x.IsDeleted);

            if (course == null)
            {
                _logger.LogWarning( "Course not found for deletion. CourseId: {CourseId}",id);

                return false;
            }

            course.IsDeleted = true;
            course.DeletedBy = actorUserId;
            course.DeletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation( "Course soft deleted successfully. CourseId: {CourseId}",id);

            return true;
        }

        public async Task<CourseResponse?> GetCourseByIdAsync(int id)
        {
            _logger.LogInformation("Get course by ID started. CourseId: {CourseId}", id);

            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(course == null)
            {
                _logger.LogWarning( "Course not found. CourseId: {CourseId}",id);

                return null;
            }

            var department = await _studentServiceClient.GetDepartmentByIdAsync(course.DepartmentId);
            if(department == null)
            {
                _logger.LogWarning( "Department not found for course. CourseId: {CourseId}, DepartmentId: {DepartmentId}",course.Id, course.DepartmentId);

                throw new NotFoundException("Department not found.");
            }
            var response = new CourseResponse
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                DurationYears = course.DurationYears,
                DepartmentId = course.DepartmentId,
                DepartmentName = department.Name
            };

            return response;
        }




        

        public async Task<CourseResponse?> UpdateCourseAsync(UpdateCourseRequestDto request, string actorUserId, int id)
        {
            _logger.LogInformation( "Course update started. CourseId: {CourseId}, ActorUserId: {ActorUserId}", id,actorUserId);

            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(course == null)
            {
                _logger.LogWarning(  "Course not found. CourseId: {CourseId}", id);
                return null;

            }

            var department = await _studentServiceClient.GetDepartmentByIdAsync(request.DepartmentId);

            if (department == null)
            {
                _logger.LogWarning("Course update failed. Department not found. DepartmentId: {DepartmentId}", request.DepartmentId);

                throw new NotFoundException("Department not found.");
            }

            var codeexist = await _context.Courses.AnyAsync(x => x.Code == request.Code && !x.IsDeleted && x.Id != id);

            if (codeexist)
            {
                _logger.LogWarning("Course update failed. Course code already exists. Code: {Code}", request.Code);

                throw new ConflictException("Course code already exists.");
            }

            course.Name = request.Name;
            course.Code = request.Code;
            course.Description = request.Description;
            course.DepartmentId = department.Id;
            course.DurationYears = request.DurationYears;
            course.UpdatedBy = actorUserId;
            course.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation( "Course updated successfully. CourseId: {CourseId}", course.Id);



            var response = new CourseResponse
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                DurationYears = course.DurationYears,
                DepartmentId = course.DepartmentId,
                DepartmentName = department.Name
            };

            return response;
        }
    }
}
