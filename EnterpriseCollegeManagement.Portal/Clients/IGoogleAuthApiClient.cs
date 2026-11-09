using EnterpriseCollegeManagement.Portal.Models;

namespace EnterpriseCollegeManagement.Portal.Clients
{
    public interface IGoogleAuthApiClient
    {
        Task<GoogleLoginApiResponse?> GetGoogleLoginResponseAsync(string handoff, CancellationToken cancellationToken = default);
    }
}
