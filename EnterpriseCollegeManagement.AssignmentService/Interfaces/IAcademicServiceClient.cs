using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface IAcademicServiceClient
    {
        Task<CourseSubjectServiceResponseDto?> CourseSubjectExistsAsync(int courseSubjectId);
    }
}
