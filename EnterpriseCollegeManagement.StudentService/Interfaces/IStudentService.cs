using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.StudentService.Interfaces
{
    public interface IStudentService
    {
        Task<StudentResponseDto> CreateStudentAsync(CreateStudentProfileRequestDto request, string actorUserId);

        Task<StudentResponseDto?> GetStudentByIdAsync(int id);

        Task<StudentResponseDto> UploadProfilePhotoAsync(string userId, string profilePhotoUrl);

        Task<StudentResponseDto?> GetMyProfileAsync(string userId);

        Task<StudentResponseDto?> UpdateMyProfileAsync(string userId, UpdateStudentProfileRequestDto request);

        Task<StudentResponseDto?> UpdateStudentAsync(int studentId, AdminUpdateStudentRequestDto request , string actorUserId);

        Task<PagedResponseDto<StudentResponseDto>> GetAllStudentsAsync(int pageNumber ,int pageSize);

        Task<PagedResponseDto<StudentResponseDto>> SearchStudentsAsync(StudentSearchRequestDto request);
    }
}
