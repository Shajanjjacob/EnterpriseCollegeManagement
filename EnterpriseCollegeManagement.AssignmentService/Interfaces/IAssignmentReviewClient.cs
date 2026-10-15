using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface IAssignmentReviewClient
    {
        Task<AIReviewResponseDto> ReviewAssignmentAsync(AssignmentReviewContextDto context);  //passing our object contains assignment and submission details 
    }
}
