using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;

namespace EnterpriseCollegeManagement.IdentityService.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);

        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request);

        Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
