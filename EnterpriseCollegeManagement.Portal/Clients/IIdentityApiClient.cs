using EnterpriseCollegeManagement.Portal.Models;

namespace EnterpriseCollegeManagement.Portal.Clients
{
    public interface IIdentityApiClient
    {
        Task<RegisterApiResult> RegisterAsync(RegisterViewModel request, CancellationToken cancellationToken = default);

        Task<LoginApiResult> LoginAsync(LoginViewModel request, CancellationToken cancellationToken = default);
    }
}
