using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface ICourseService
    {
        Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request, string actorUserId);

        Task<CourseResponse?> UpdateCourseAsync(UpdateCourseRequestDto request, string actorUserId, int id);

        Task<CourseResponse?> GetCourseByIdAsync(int id);

        Task<bool> DeleteCourseAsync(int id, string actorUserId);
    }
}
