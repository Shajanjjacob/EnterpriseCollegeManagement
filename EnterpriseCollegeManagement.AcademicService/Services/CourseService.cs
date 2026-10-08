
using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using System.Text.Json;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class CourseService : ICourseService
    {
        private readonly AcademicDbContext _context;
        private readonly ILogger<CourseService> _logger;
        private readonly IStudentServiceClient _studentServiceClient;

        private readonly IRedisCacheService _redisCacheService; //added readis interface 
      

        public CourseService(AcademicDbContext context, ILogger<CourseService> logger, IStudentServiceClient studentServiceClient,
            IRedisCacheService redisCacheService)
        {
            _context = context;
            _logger = logger;
            _studentServiceClient = studentServiceClient;
            _redisCacheService = redisCacheService;
            
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

            //REDIS
            var cacheKey = $"course:{id}";  // made a key based on course id 

            var cachedCourse = await _redisCacheService.GetAsync(cacheKey); //calling function to get 

            if(cachedCourse != null) //if data is in cache
            {
                _logger.LogInformation("Course found in Redis cache. CourseId: {CourseId}",id);

                return JsonSerializer.Deserialize<CourseResponse?>(cachedCourse); //convert json text into dtos formate (c# object)
            }

            if (cachedCourse == null)
            {
                _logger.LogInformation("Course not found in Redis cache. Fetching from database. CourseId: {CourseId}",id);
            }

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
            //REDIS
            //if data is not in cache then 

            var courseJson = JsonSerializer.Serialize(response); //covert dtos to json 
            await _redisCacheService.SetAsync(cacheKey, courseJson,TimeSpan.FromMinutes(20)); //set value in redis using key 

            return response;
        }

        //search 
        public async Task<PagedResponseDto<CourseResponse>> GetCoursesAsync(string? search, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Get courses started. Search: {Search}, PageNumber: {PageNumber}, PageSize: {PageSize}", search, pageNumber, pageSize);


            var query = _context.Courses.AsNoTracking().Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
            }

            var totalrecords = await query.CountAsync();

            //pagination

            var courses = await query.OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalrecords / pageSize);

            var items = new List<CourseResponse>();

            foreach (var course in courses)
            {
                var department = _studentServiceClient.GetDepartmentByIdAsync(course.DepartmentId);

                if(department == null)
                {
                    _logger.LogWarning("Department not found while getting courses. CourseId: {CourseId}, DepartmentId: {DepartmentId}", course.Id,
                        course.DepartmentId);

                    continue;
                }

                var respone = new CourseResponse
                {
                    Id = course.Id,
                    Code = course.Code,
                    Name = course.Name,
                    Description = course.Description,
                    DurationYears = course.DurationYears,
                    DepartmentId = course.DepartmentId,
                    
                };

                items.Add(respone);
            }

            return new PagedResponseDto<CourseResponse>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalrecords,
                TotalPages = totalPages
            };

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
