using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IQuizGenerationService
    {
        Task<GenerateQuizResponseDto> GenerateQuizAsync(GenerateQuizRequestDto request ,string actorUserId);
     }
}
