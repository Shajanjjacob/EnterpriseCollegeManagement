using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IAIQuizClient
    {
        //AI req 

        Task<List<GeneratedQuestionDto>> GenerateQuestionsAsync(QuizGenerationContextDto context); // passing context 

    }
}
