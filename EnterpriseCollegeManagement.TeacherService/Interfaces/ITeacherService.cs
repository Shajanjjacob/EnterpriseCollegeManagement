using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;

namespace EnterpriseCollegeManagement.TeacherService.Interfaces
{
    public interface ITeacherService
    {
        Task<TeacherResponseDto> CreateTeacherAsync(CreateTeacherProfileRequestDto request ,string actorUserId);

        Task<TeacherResponseDto?> GetMyProfileAsync(string userId);

        Task<TeacherResponseDto> UpdateMyProfileAsync(UpdateTeacherProfileRequestDto request, string userId);

        Task<TeacherResponseDto> UpdateTeacherAsync(AdminUpdateTeacherRequestDto request, string userId, string actorUserId);

        Task<TeacherResponseDto> UploadProfilePhotoAsync(string profilePhotoUrl, string userId);
    }
}
