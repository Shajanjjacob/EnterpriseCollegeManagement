using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface IAssignmentSubmissionService
    {
        Task<AssignmentSubmissionResponseDto> SubmitAssignmentAsync(SubmitAssignmentRequestDto request, string studentUserId);
        Task<List<AssignmentSubmissionResponseDto>> GetMySubmissionsAsync(string studentUserId);

        Task<List<AssignmentSubmissionResponseDto>> GetSubmissionsByAssignmentAsync(int assignmentId,string currentUserId, bool isAdmin);
        //list of all assignment submitted by student based onn  single assignmntid 


        Task<AssignmentSubmissionResponseDto> GetSubmissionByIdAsync(int id, string currentUserId,bool isAdmin);

        Task<AssignmentSubmissionResponseDto> GradeSubmissionAsync(int id, GradeSubmissionRequestDto request, string currentUserId,  bool isAdmin);
    }
}
