using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EnterpriseCollegeManagement.IdentityService.Services
{
    public class GoogleLoginCodeStore
    {
        private readonly ConcurrentDictionary<string, GoogleLoginCodeEntry> _codes = new();

        // HERE STRORE GOOGLE LOGIN RESPONSE AND GENERATE TEM CODE 
        public string Store(GoogleLoginResponseDto response)
        {
            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            var entry = new GoogleLoginCodeEntry
            {
                Response = response,
                ExpiresAt = DateTime.UtcNow.AddMinutes(1)
            };

            _codes[code] = entry;

            return code;
        }

        // Retrieve response using code only once

        //Retrieves the response using that code and deletes the code immediately.
        public bool TryTake(string code, out GoogleLoginResponseDto? response)
        {
            response = null;

            if (!_codes.TryRemove(code, out var entry))
            {
                return false;
            }

            if (entry.ExpiresAt <= DateTime.UtcNow)//time checking 
            {
                return false;
            }

            response = entry.Response;

            return true;
        }

        private class GoogleLoginCodeEntry
        {
            public GoogleLoginResponseDto Response { get; set; } = null!;

            public DateTime ExpiresAt { get; set; }
        }
    }
}
