using EnterpriseCollegeManagement.StudentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.StudentService.Interfaces
{
    public interface ICourseServiceClient
    {
        Task<CourseResponseDto?> GetCourseByIdAsync(int courseId);
    }
}
