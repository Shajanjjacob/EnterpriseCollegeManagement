using AutoMapper;
using EnterpriseCollegeManagement.StudentService.Data;
using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Entities;
using EnterpriseCollegeManagement.StudentService.Exceptions;
using EnterpriseCollegeManagement.StudentService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.StudentService.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly StudentDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(StudentDbContext context, IMapper mapper, ILogger<DepartmentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentRequestDto request, string actorUserId)
        {
            _logger.LogInformation("Department creation started. Code: {Code}", request.Code);



            if (string.IsNullOrWhiteSpace(request.Code))
            {
                _logger.LogWarning( "Department creation failed. Code is empty.");

                throw new BadRequestException("Department code is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("Department creation failed. Name is empty.");

                throw new BadRequestException( "Department name is required.");
            }


            var name = request.Name.Trim();
            var code = request.Code.Trim().ToUpperInvariant(); ;

            var exist = await _context.Departments.AnyAsync(x => x.Code == code && !x.IsDeleted);

            if (exist)
            {
                _logger.LogWarning("Department code already exists. Code: {Code}", code);

                throw new ConflictException("Department code already exists.");
            }

            var department = new Department
            {
                Name = name,
                Code = code,

                CreatedBy = actorUserId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Departments.Add(department);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Department created successfully. DepartmentId: {DepartmentId}",department.Id);

            return _mapper.Map<DepartmentResponseDto>(department);

        }

        public async Task<bool> DeleteDepartmentAsync(int id, string actorUserId)
        {
            _logger.LogInformation( "Department deletion started. DepartmentId: {DepartmentId}",id);

            var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if(department == null)
            {
                return false;
            }

            var hasStudents = await _context.Students
                .AnyAsync(x => x.DepartmentId == id && !x.IsDeleted);

            if (hasStudents)
            {
                throw new ConflictException("Department cannot be deleted because students are assigned to it.");
            }

            department.IsDeleted = true;
            department.DeletedDate = DateTime.UtcNow;
            department.DeletedBy = actorUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation( "Department soft deleted successfully. DepartmentId: {DepartmentId}",id);

            return true;

        }

        public async Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync()
        {
            _logger.LogInformation("Fetching all departments.");

            var departments = await _context.Departments
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToListAsync();

            _logger.LogInformation( "Departments retrieved successfully. Count: {Count}",departments.Count);


            return _mapper.Map<List<DepartmentResponseDto>>(departments);

        }

        public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching department. DepartmentId: {DepartmentId}",id);

            var department = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if(department == null)
            {
                _logger.LogWarning("Department not found. DepartmentId: {DepartmentId}",id);


                return null;
            }

            return _mapper.Map<DepartmentResponseDto>(department);

        }

        public async Task<DepartmentResponseDto?> UpdateDepartmentAsync(UpdateDepartmentRequestDto request, string actorUserId, int id)
        {
            _logger.LogInformation("Department update started. DepartmentId: {DepartmentId}",id);

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                throw new BadRequestException("Department code is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Department name is required.");
            }



            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if(department == null)
            {
                return null;
            }

            var code = request.Code.Trim().ToUpperInvariant(); ;
            var name = request.Name.Trim();

            var codeExists = await _context.Departments
                .AnyAsync(x => x.Id != id && x.Code == code && !x.IsDeleted);

            if (codeExists)
            {
                throw new ConflictException("Department code already exists.");
            }



            department.Name = name;
            department.Code = code;

            department.UpdatedBy = actorUserId;
            department.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Department updated successfully. DepartmentId: {DepartmentId}", id);

            return _mapper.Map<DepartmentResponseDto?>(department);
        }
    }


}
