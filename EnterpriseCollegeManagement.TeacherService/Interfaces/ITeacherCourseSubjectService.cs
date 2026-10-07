using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;

namespace EnterpriseCollegeManagement.TeacherService.Interfaces
{
    public interface ITeacherCourseSubjectService
    {
        Task<TeacherCourseSubjectResponseDto> AssignCourseSubjectAsync(AssignTeacherCourseSubjectRequestDto request, string actorUserId);

        Task<List<TeacherCourseSubjectResponseDto>> GetAssignedCourseSubjectsAsync(int teacherId);

        Task  RemoveCourseSubjectAsync(int teacherId,int courseSubjectId,string actorUserId);
    }
}
