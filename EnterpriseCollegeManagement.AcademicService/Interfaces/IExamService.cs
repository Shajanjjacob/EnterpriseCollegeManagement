using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IExamService
    {
        Task<ExamResponseDto> CreateExamAsync(CreateExamRequestDto request, string actorUserId);

        Task<ExamResponseDto?> GetExamByIdAsync(int id);

        Task<List<ExamResponseDto>> GetAllExamsAsync();

        Task<ExamResponseDto> UpdateExamAsync(int id, CreateExamRequestDto request, string actorUserId);

        Task<bool> DeleteExamAsync(int id, string actorUserId);
    }
}
