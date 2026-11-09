using EnterpriseCollegeManagement.Portal.Models;
using System.Net.Http.Json;
using System.Net;

namespace EnterpriseCollegeManagement.Portal.Clients
{
    public class IdentityApiClient : IIdentityApiClient
    {

        private readonly ILogger<IdentityApiClient> _logger;
        private readonly HttpClient _httpClient;

        public IdentityApiClient(ILogger<IdentityApiClient> logger, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

      

        public async Task<RegisterApiResult> RegisterAsync(RegisterViewModel request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending registration request for {Email}", request.Email);

            var response  = await _httpClient.PostAsJsonAsync("identity/api/Auth/register",request,cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<RegisterApiResult>(cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new RegisterApiResult
                {
                    Success = true,
                    Message = "Registration successful.",
                    StatusCode = (int)response.StatusCode
                };
            }
            _logger.LogWarning("Registration failed. StatusCode: {StatusCode}", response.StatusCode);

            string message = "Registration failed. Please try again.";

            if(response.StatusCode == HttpStatusCode.Conflict)
            {
                message = "Email already exists.";
            }
            else if (result != null && !string.IsNullOrEmpty(result.Message))
            {
                message = result.Message;
            }
            return new RegisterApiResult
            {
                Success = false,
                Message = message,
                StatusCode = (int)response.StatusCode
            };
        }

        public async Task<LoginApiResult> LoginAsync(LoginViewModel request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending login request for {Email}", request.Email);

            var response = await _httpClient.PostAsJsonAsync("identity/api/Auth/login", request, cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<LoginApiResult>(cancellationToken: cancellationToken);

            if(response.IsSuccessStatusCode)
            {
                return new LoginApiResult
                {
                    Success = true,
                    Message = result?.Message ?? "Login successful.",
                    Token = result?.Token,
                    Expiration = result?.Expiration,
                    StatusCode = (int)response.StatusCode,
                    RefreshToken = result?.RefreshToken,
                    RefreshTokenExpiration = result?.RefreshTokenExpiration,
                    Role = result?.Role
                    

                };
            }

            _logger.LogWarning("Login failed. StatusCode: {StatusCode}",  response.StatusCode);

            string message = "Login failed. Please try again.";

            if(result != null && !string.IsNullOrEmpty(result.Message))
            {
                message = result.Message;
            }

            return new LoginApiResult
            {
                Success = false,
                Message = message,
                StatusCode = (int)response.StatusCode
            };
        }
    }
}
