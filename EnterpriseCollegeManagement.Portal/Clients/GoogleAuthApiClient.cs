using EnterpriseCollegeManagement.Portal.Models;

namespace EnterpriseCollegeManagement.Portal.Clients
{
    public class GoogleAuthApiClient : IGoogleAuthApiClient
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleAuthApiClient> _logger;

        public GoogleAuthApiClient(HttpClient httpClient, ILogger<GoogleAuthApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
       
        public async Task<GoogleLoginApiResponse?> GetGoogleLoginResponseAsync(string handoff, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(handoff))
            {
                _logger.LogWarning("Google login handoff was empty.");
                return null;
            }

            using var response = await _httpClient.GetAsync($"identity/api/Auth/google-token?handoff={Uri.EscapeDataString(handoff)}", cancellationToken);

             if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google login response request failed with status code {StatusCode}.", response.StatusCode);

                return null;
            }

            return await response.Content.ReadFromJsonAsync<GoogleLoginApiResponse>(cancellationToken: cancellationToken);
        }
    }
}
