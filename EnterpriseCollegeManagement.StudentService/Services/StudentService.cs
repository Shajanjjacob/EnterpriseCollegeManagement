using AutoMapper;
using EnterpriseCollegeManagement.StudentService.Data;
using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Entities;
using EnterpriseCollegeManagement.StudentService.Exceptions;
using EnterpriseCollegeManagement.StudentService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using System.Drawing.Printing;

namespace EnterpriseCollegeManagement.StudentService.Services
{
    public class StudentService : IStudentService
    {

        private readonly StudentDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentService> _logger;

        private readonly IIdentityServiceClient _identityServiceClient;
        public StudentService(StudentDbContext context, IMapper mapper, ILogger<StudentService> logger, IIdentityServiceClient identityServiceClient)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _identityServiceClient = identityServiceClient;
        }


        public async Task<StudentResponseDto> CreateStudentAsync(CreateStudentProfileRequestDto request, string actorUserId)
        {
            _logger.LogInformation("Student profile creation started. TargetUserId: {TargetUserId}, ActorUserId: {ActorUserId}", request.UserId,actorUserId);

            var identityUser = await _identityServiceClient.GetUserAsync(request.UserId);
            if(identityUser == null)
            {
                _logger.LogWarning("Student profile creation failed. User not found. UserId: {UserId}",request.UserId);

                throw new NotFoundException("User not found.");
            }
                
            if(!string.Equals(identityUser.Role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning( "Student profile creation failed. User is not a Student. UserId: {UserId}, Role: {Role}", request.UserId,
           identityUser.Role);

                throw new BadRequestException("Student profile can only be created for users with Student role.");
            }

            var  existingStudent =  await _context.Students.FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsDeleted);

            if(existingStudent != null)
            {
                _logger.LogWarning("Student profile already exists. UserId: {UserId}", request.UserId);

                throw new ConflictException("Student profile already exists.");
            }

            var departmentExists = await _context.Departments.AnyAsync(x => x.Id == request.DepartmentId);

            if(!departmentExists)
            {
                _logger.LogWarning( "Student profile creation failed. Department not found. DepartmentId: {DepartmentId}", request.DepartmentId);

                throw new NotFoundException("Department not found.");
            }

            var admissionNumberExists = await _context.Students.AnyAsync(x => x.AdmissionNumber == request.AdmissionNumber && !x.IsDeleted );

            if(admissionNumberExists)
            {
                _logger.LogWarning("Student profile creation failed. Admission number already exists. AdmissionNumber: {AdmissionNumber}",request.AdmissionNumber);

                throw new ConflictException("Admission number already exists.");
            }

            var student = _mapper.Map<Student>(request);

            student.CreatedBy = actorUserId;
            student.CreatedDate = DateTime.UtcNow;
            student.IsDeleted = false;

            _context.Students.Add(student);

            await _context.SaveChangesAsync();

            _logger.LogInformation( "Student profile created successfully. StudentId: {StudentId}, UserId: {UserId}",student.Id,student.UserId);

            await _context.Entry(student).Reference(x => x.Department).LoadAsync();

           
            var response = _mapper.Map<StudentResponseDto>(student);

            return response;
        }

       

        public async Task<StudentResponseDto?> GetStudentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching student profile. StudentId: {StudentId}",id);

            var student = await _context.Students
                .Include(d => d.Department)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if(student == null)
            {
                _logger.LogWarning("Student profile not found. StudentId: {StudentId}",id);

                return null;
            }

            var response = _mapper.Map<StudentResponseDto>(student);

            _logger.LogInformation("Student profile retrieved successfully. StudentId: {StudentId}",id);

            return response;
        }

        public async Task<StudentResponseDto> UploadProfilePhotoAsync( string userId, string profilePhotoUrl)
        {
            var student = await _context.Students
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (student == null)
            {
                throw new NotFoundException( "Student profile not found.");
            }

            student.ProfilePhotoUrl = profilePhotoUrl;
            student.UpdatedBy = userId;
            student.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<StudentResponseDto>(student);
        }


        public async Task<StudentResponseDto?> GetMyProfileAsync(string userId)
        {
            _logger.LogInformation( "Fetching current student profile. UserId: {UserId}", userId);

            var student =  await _context.Students.
                Include(d => d.Department)
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

            if( student == null)
            {
                _logger.LogWarning( "Student profile not found. UserId: {UserId}", userId);

                return null;
            }

            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<StudentResponseDto?> UpdateMyProfileAsync(string userId, UpdateStudentProfileRequestDto request)
        {
            _logger.LogInformation("Student profile update started. UserId: {UserId}", userId);


            var student = await _context.Students
                .Include(d => d.Department)
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

            if(student == null)
            {

                _logger.LogWarning("Student profile not found for update. UserId: {UserId}",userId);

                return null;
            }

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.DateOfBirth = request.DateOfBirth;
            student.Phone = request.Phone;
            student.Address = request.Address;

            student.UpdatedBy = userId;
            student.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Student profile updated successfully. StudentId: {StudentId}, UserId: {UserId}",student.Id,userId);

            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<StudentResponseDto?> UpdateStudentAsync(int studentId, AdminUpdateStudentRequestDto request, string actorUserId)
        {
            _logger.LogInformation( "Admin student update started. StudentId: {StudentId}, AdminUserId: {AdminUserId}",studentId, actorUserId);


            var student = await _context.Students
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);

            if( student == null )
            {
                _logger.LogWarning( "Student profile not found. StudentId: {StudentId}", studentId);

                return null;
            }


            var departmentExists = await _context.Departments
                .AnyAsync(x => x.Id == request.DepartmentId);

            if(!departmentExists)
            {
                _logger.LogWarning( "Department not found. DepartmentId: {DepartmentId}",request.DepartmentId);

                throw new NotFoundException("Department not found.");
            }

            var admissionNumberExists = await _context.Students
                .AnyAsync(x => x.AdmissionNumber == request.AdmissionNumber && x.Id != studentId && !x.IsDeleted);

            if (admissionNumberExists)
            {
                _logger.LogWarning("Admission number already exists. AdmissionNumber: {AdmissionNumber}",request.AdmissionNumber);

                throw new ConflictException("Admission number already exists.");
            }

            if(request.DateOfBirth.Date >  DateTime.UtcNow.Date)
            {
                _logger.LogWarning( "Invalid date of birth. StudentId: {StudentId}", studentId);

                throw new BadRequestException("Date of birth cannot be in the future.");
            }

            if (request.EnrollmentDate.Date <request.DateOfBirth.Date)
            {
                _logger.LogWarning(  "Invalid enrollment date. StudentId: {StudentId}",studentId);

                throw new BadRequestException("Enrollment date cannot be before date of birth.");
            }

            student.AdmissionNumber = request.AdmissionNumber;
            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.DateOfBirth = request.DateOfBirth;
            student.Phone = request.Phone;
            student.Address = request.Address;
            student.DepartmentId = request.DepartmentId;
            student.EnrollmentDate = request.EnrollmentDate;

            student.UpdatedBy = actorUserId;
            student.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            //load new dpt

            await _context.Entry(student).Reference(x => x.Department).LoadAsync();

            _logger.LogInformation( "Student updated successfully. StudentId: {StudentId}, AdminUserId: {AdminUserId}",studentId,actorUserId);

       
            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<PagedResponseDto<StudentResponseDto>> GetAllStudentsAsync(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching students. PageNumber: {PageNumber}, PageSize: {PageSize}",pageNumber, pageSize);

            var query =  _context.Students.
                Include(X => X.Department)
                .Where(X => !X.IsDeleted);

            var totalCount = await query.CountAsync();

            var student = await query.OrderBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var studentresponse = _mapper.Map<List<StudentResponseDto>>(student);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            _logger.LogInformation( "Students retrieved successfully. Count: {Count}, TotalCount: {TotalCount}", studentresponse.Count,totalCount);

            return new PagedResponseDto<StudentResponseDto>
            {
                Items = studentresponse,
                pageNumber = pageNumber,
                pageSize = pageSize,
                TotalPage = totalPages,
                TotalCount = totalCount

            };
        }

        public async Task<PagedResponseDto<StudentResponseDto>> SearchStudentsAsync(StudentSearchRequestDto request)
        {
            _logger.LogInformation(
        "Fetching students. PageNumber: {PageNumber}, PageSize: {PageSize}, Search: {Search}, StudentId: {StudentId}, DepartmentId: {DepartmentId}",
        request.PageNumber,
        request.PageSize,
        request.search,
        request.StudentId,
        request.DepartmentId);


            var query =  _context.Students.AsNoTracking().Include(d => d.Department).Where(x => !x.IsDeleted);

            //studentid

            if (request.StudentId.HasValue)
            {
                query = query.Where(x => x.Id == request.StudentId.Value);
            }

            //addmissionno:

            if (!string.IsNullOrWhiteSpace(request.AdmissionNumber))
            {
                var admissionNumber =  request.AdmissionNumber.Trim();

                query = query.Where(x => x.AdmissionNumber.Contains(request.AdmissionNumber));
            }

            //dept

            if(request.DepartmentId.HasValue)
            {
                query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
            }

            //general all 

            if (!string.IsNullOrWhiteSpace(request.search))
            {
                var search =request.search.Trim();

                query = query.Where(x => x.AdmissionNumber.Contains(search) ||

                x.FirstName.Contains(search) || x.LastName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var student = await query.OrderBy(x => x.Id).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageNumber).ToListAsync();

            var studentResponses = _mapper.Map<List<StudentResponseDto>>(student);


            var totalPages =  (int)Math.Ceiling(totalCount / (double)request.PageSize);

            _logger.LogInformation("Students retrieved successfully. ReturnedCount: {ReturnedCount}, TotalCount: {TotalCount}",
                studentResponses.Count,
                totalCount);

            return new PagedResponseDto<StudentResponseDto>
            {
                Items = studentResponses,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPage = totalPages
            };
        }

        public async Task<bool> DeleteStudentAsync(int studentId, string actorUserId)
        {
            _logger.LogInformation("Student soft delete started. StudentId: {StudentId}, AdminUserId: {AdminUserId}",  studentId,actorUserId);

            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId  && !x.IsDeleted);

            if(student == null)
            {
                _logger.LogWarning( "Student not found or already deleted. StudentId: {StudentId}",studentId);

                return false;
            }

            student.IsDeleted = true;
            student.DeletedDate = DateTime.UtcNow;
            student.DeletedBy = actorUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation( "Student soft deleted successfully. StudentId: {StudentId}, AdminUserId: {AdminUserId}",studentId,actorUserId);

            return true;

        }
    }
}
