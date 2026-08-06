using EnterpriseCollegeManagement.IdentityService.Configurations;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Unicode;

namespace EnterpriseCollegeManagement.IdentityService.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(ILogger<TokenService> logger, IOptions<JwtSettings> options)
        {
            _logger = logger;
           _jwtSettings = options.Value;
        }

        public async Task<TokenResultDto> GenerateTokenAsync(ApplicationUser user)
        {
            _logger.LogInformation("Generating JWT token for user: {Email}", user.Email);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub , user.Id),
                new Claim(JwtRegisteredClaimNames.Email , user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti ,Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,user.UserName!)

            };

            var SecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var SigningCredentials  = new SigningCredentials(SecretKey,SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:_jwtSettings.Issuer,
                audience:_jwtSettings.Audience,
                claims:claims,
                expires:DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials:SigningCredentials
               

                );

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);

            _logger.LogInformation("JWT generated successfully for user: {Email}", user.Email);

            return new TokenResultDto
            {
                Token = jwt,
                Expiration = token.ValidTo
            };
        }
    }
}
