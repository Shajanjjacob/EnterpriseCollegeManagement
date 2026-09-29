using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IStudentAnswerService
    {
        Task<StudentAnswerResponseDto> SaveAnswerAsync(CreateStudentAnswerRequestDto request, string studentUserId);
    }
}
