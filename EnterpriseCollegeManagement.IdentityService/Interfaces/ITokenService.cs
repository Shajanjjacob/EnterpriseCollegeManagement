using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;

namespace EnterpriseCollegeManagement.IdentityService.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResultDto> GenerateTokenAsync(ApplicationUser user);
    }
}
