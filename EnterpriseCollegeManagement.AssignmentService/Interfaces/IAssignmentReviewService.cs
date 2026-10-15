using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface IAssignmentReviewService
    {
        Task<AIReviewResponseDto> ReviewAssignmentAsync(int assignmentId, int submissionId, string currentUserId, bool isAdmin);
    }
}
