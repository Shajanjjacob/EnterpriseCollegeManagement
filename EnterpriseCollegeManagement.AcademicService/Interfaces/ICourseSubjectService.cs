using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface ICourseSubjectService
    {
        Task<CourseSubjectResponseDto> AssignSubjectToCourseAsync(CreateCourseSubjectRequestDto request);

        Task<List<CourseSubjectResponseDto>> GetSubjectsByCourseIdAsync(int courseId);

        Task<bool> RemoveSubjectFromCourseAsync(int id);
    }
}
