using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;

namespace EnterpriseCollegeManagement.TeacherService.Interfaces
{
    public interface IAcademicServiceClient
    {
        Task<CourseResponseDto?> GetCourseByIdAsync(int courseId);

        Task<SubjectResponseDto?> GetSubjectByIdAsync(int subjectId);

        Task<List<CourseSubjectResponseDto>> GetSubjectsByCourseIdAsync(int courseId);
    }
}
