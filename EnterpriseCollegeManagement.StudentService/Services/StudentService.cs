using AutoMapper;
using EnterpriseCollegeManagement.StudentService.Data;
using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Entities;
using EnterpriseCollegeManagement.StudentService.Exceptions;
using EnterpriseCollegeManagement.StudentService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;

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
    }
}
