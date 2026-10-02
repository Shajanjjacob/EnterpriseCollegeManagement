using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IExamAttendanceService
    {
        Task<ExamAttendanceResponseDto> StartExamAsync(CreateExamAttendanceRequestDto request, string studentUserId);

        Task<List<ExamQuestionResponseDto>> GetExamQuestionsAsync(int attendanceId, string studentUserId);

        Task<ExamAttendanceResponseDto> SubmitExamAsync(string studentUserId, int attendanceId);

        Task<ExamAttendanceResponseDto> PublishResultAsync(int attendanceId, string actorUserId); //admin/teacher publish result 1st 

        Task<StudentExamResultResponseDto?> GetStudentResultAsync(int examId, string studentUserId);

        Task<List<ExamSubmissionResponseDto>> GetExamSubmissionsAsync(int examId);

        Task ProcessExpiredExamsAsync();   //autosubmit answer 
    }
}
