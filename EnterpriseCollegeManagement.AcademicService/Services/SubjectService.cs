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
    public class SubjectService : ISubjectService
    {
        private readonly AcademicDbContext _context;
        private readonly ILogger<SubjectService> _logger;
        private readonly IStudentServiceClient _studentServiceClient;

        public SubjectService(AcademicDbContext context, ILogger<SubjectService> logger, IStudentServiceClient studentServiceClient)
        {
            _context = context;
            _logger = logger;
            _studentServiceClient = studentServiceClient;
        }

        public async Task<SubjectResponseDto> CreateSubjectAsync(CreateSubjectRequestDto request, string actorUserId)
        {
            _logger.LogInformation("Subject creation started. SubjectName: {SubjectName}, DepartmentId: {DepartmentId}, ActorUserId: {ActorUserId}",
               request.Name,
               request.DepartmentId,
               actorUserId);

            var department = await _studentServiceClient.GetDepartmentByIdAsync(request.DepartmentId);

            if(department == null)
            {
                _logger.LogWarning("Subject creation failed. Department not found. DepartmentId: {DepartmentId}",request.DepartmentId);

                throw new NotFoundException("Department not found.");
            }

            var subjectcodeExists = await _context.Subjects.AnyAsync(x => x.Code == request.Code && !x.IsDeleted);
            if (subjectcodeExists)
            {
                _logger.LogWarning( "Subject creation failed. Subject code already exists. Code: {Code}",request.Code);

                throw new ConflictException( "Subject code already exists.");
            }

            var subject = new Subject
            {
                Code = request.Code,
                Name = request.Name,
                Credits = request.Credits,
                DepartmentId = request.DepartmentId,

                CreatedBy = actorUserId,
                CreatedDate = DateTime.UtcNow,

                IsDeleted = false
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subject created successfully. SubjectId: {SubjectId}, DepartmentId: {DepartmentId}",subject.Id,subject.DepartmentId);

            var response = new SubjectResponseDto
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Credits = subject.Credits,
                DepartmentId = subject.DepartmentId,
                DepartmentName = department.Name
            };

            return response;
        }

        public async Task<bool> DeleteSubjectAsync(string actorUserId, int id)
        {
            _logger.LogInformation( "Subject deletion started. SubjectId: {SubjectId}, ActorUserId: {ActorUserId}",id, actorUserId);

            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(subject == null)
            {
                _logger.LogWarning( "Subject not found for deletion. SubjectId: {SubjectId}", id);

                return false;
            }

            subject.IsDeleted = true;
            subject.DeletedBy = actorUserId;
            subject.DeletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Subject soft deleted successfully. SubjectId: {SubjectId}",id);

            return true;
        }

        public async Task<List<SubjectResponseDto>> GetAllSubjectsAsync()
        {
            var subjects = await _context.Subjects.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x=> x.Id).ToListAsync();

            var response = new List<SubjectResponseDto>();

            foreach(var subject in subjects)
            {
                var department = await _studentServiceClient.GetDepartmentByIdAsync(subject.DepartmentId);

                response.Add(new SubjectResponseDto
                {
                    Id = subject.Id,
                    Code = subject.Code,
                    Name = subject.Name,
                    Credits = subject.Credits,
                    DepartmentId = subject.DepartmentId,
                    DepartmentName = department?.Name ?? "Unknown"

                });
            }
            return response;

        }

        public async Task<SubjectResponseDto?> GetSubjectByIdAsync(int id)
        {
            _logger.LogInformation( "Get subject by ID started. SubjectId: {SubjectId}", id);

            var subject = await _context.Subjects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);  
            
            if(subject == null)
            {
                _logger.LogWarning( "Subject not found. SubjectId: {SubjectId}",id);

                return null;

            }
            var department = await _studentServiceClient.GetDepartmentByIdAsync(subject.DepartmentId);

            if(department == null)
            {
                _logger.LogWarning("Department not found for subject. SubjectId: {SubjectId}, DepartmentId: {DepartmentId}", subject.Id, subject.DepartmentId);

                throw new NotFoundException("Department not found.");

            }

            var response = new SubjectResponseDto
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Credits = subject.Credits,
                DepartmentId = subject.DepartmentId,
                DepartmentName = department.Name

            };

            return response;
        }

        public async Task<SubjectResponseDto?> UpdateSubjectAsync(int id, UpdateSubjectRequestDto request, string actorUserId)
        {
            _logger.LogInformation( "Subject update started. SubjectId: {SubjectId}, ActorUserId: {ActorUserId}", id,actorUserId);

            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if(subject == null)
            {
                _logger.LogWarning("Subject not found. SubjectId: {SubjectId}",id);

                return null;
            }

            var department = await _studentServiceClient.GetDepartmentByIdAsync(request.DepartmentId);

            if (department == null)
            {
                _logger.LogWarning("Subject update failed. Department not found. DepartmentId: {DepartmentId}", request.DepartmentId);

                throw new NotFoundException("Department not found.");
            }

            var subjectCodeExists = await _context.Subjects.AnyAsync(x => x.Code == request.Code && x.Id != id && !x.IsDeleted);
            if (subjectCodeExists)
            {
                _logger.LogWarning( "Subject update failed. Subject code already exists. Code: {Code}",request.Code);

                throw new ConflictException("Subject code already exists.");
            }

            subject.Code = request.Code;
            subject.Name = request.Name;
            subject.Credits = request.Credits;
            subject.DepartmentId = request.DepartmentId;
            subject.UpdatedBy = actorUserId;
            subject.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Subject updated successfully. SubjectId: {SubjectId}", subject.Id);

            var respons = new SubjectResponseDto
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Credits = subject.Credits,
                DepartmentId = subject.DepartmentId,
                DepartmentName = department.Name
            };
            return respons;
        }
    }
}
