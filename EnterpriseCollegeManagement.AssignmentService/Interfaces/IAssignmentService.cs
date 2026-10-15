using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface IAssignmentService
    {
        Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentRequestDto request, string currentUserId, bool isAdmin);  //admin or teacher  = currentUserId
        Task<AssignmentResponseDto> GetAssignmentByIdAsync(int id);
        Task<List<AssignmentResponseDto>> GetMyAssignmentsAsync(string currentUserId, bool isAdmin);
        Task<AssignmentResponseDto> UpdateAssignmentAsync(int id,UpdateAssignmentRequestDto request, string currentUserId, bool isAdmin);

        Task<AssignmentResponseDto> PublishAssignmentAsync(int id,PublishAssignmentRequestDto request, string currentUserId, bool isAdmin);
        Task<bool> DeleteAssignmentAsync(int id,string currentUserId, bool isAdmin);
    }
}
