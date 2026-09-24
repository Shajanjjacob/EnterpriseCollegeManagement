using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IQuestionService
    {

        Task<QuestionResponseDto> CreateQuestionAsync(CreateQuestionRequestDto request, string actorUserId);

        Task<QuestionResponseDto?> GetQuestionByIdAsync(int id);
        Task<List<QuestionResponseDto>> GetQuestionsByExamIdAsync(int examId);

        Task<QuestionResponseDto> UpdateQuestionAsync(int id , CreateQuestionRequestDto request, string actorUserId);

        Task<bool> DeleteQuestionAsync(int id, string actorUserId);
    }
}
