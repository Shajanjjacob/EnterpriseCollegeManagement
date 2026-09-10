using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.StudentService.Interfaces
{
    public interface IStudentService
    {
        Task<StudentResponseDto> CreateStudentAsync(CreateStudentProfileRequestDto request, string actorUserId);
    }
}
