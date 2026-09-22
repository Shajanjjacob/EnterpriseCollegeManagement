using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;

namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface ISubjectService
    {
        Task<SubjectResponseDto> CreateSubjectAsync(CreateSubjectRequestDto request, string actorUserId);

        Task<SubjectResponseDto?> UpdateSubjectAsync(int id, UpdateSubjectRequestDto request, string actorUserId);

        Task<bool> DeleteSubjectAsync(string actorUserId, int id);

        Task<SubjectResponseDto?> GetSubjectByIdAsync(int id);

        Task<List<SubjectResponseDto>> GetAllSubjectsAsync();
    }
}
