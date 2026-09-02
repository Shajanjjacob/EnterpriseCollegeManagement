using EnterpriseCollegeManagement.IdentityService.Configurations;
using EnterpriseCollegeManagement.IdentityService.Data;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;
using EnterpriseCollegeManagement.IdentityService.Exceptions;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace EnterpriseCollegeManagement.IdentityService.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TokenService(ILogger<TokenService> logger, IOptions<JwtSettings> options, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
           _jwtSettings = options.Value;
            _userManager = userManager;
            _context = context;
        }

        public async Task<TokenResultDto> GenerateTokenAsync(ApplicationUser user)
        {
            _logger.LogInformation("Generating JWT token for user: {Email}", user.Email);

            var roles = await  _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub , user.Id),
                    new Claim(JwtRegisteredClaimNames.Email , user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti ,Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name,user.UserName!)
                 };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


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

            //refreshToken 

            var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays);

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                CreateAt = DateTime.UtcNow,
                ExpiresAt = refreshTokenExpiration,
                IsRevoked = false

            };
            _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();


             _logger.LogInformation("JWT and refresh token generated successfully for user: {Email}",user.Email);


            return new TokenResultDto
            {
                Token = jwt,
                Expiration = token.ValidTo,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiration = refreshTokenExpiration
            };
        }

        public async Task<TokenResultDto> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Refresh token request received.");

            var storedToken = await _context.RefreshTokens
           .Include(x=> x.user)
           .FirstOrDefaultAsync( x => x.Token == refreshToken);

            if (storedToken == null)
            {
                _logger.LogWarning( "Refresh token not found.");

                throw new UnauthorizedException( "Invalid refresh token.");
            }
            if (storedToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token has been revoked. UserId: {UserId}",storedToken.UserId);

                throw new UnauthorizedException("Refresh token has been revoked.");
            }

            var user = storedToken.user;

            if(user == null)
            {
                _logger.LogWarning("User associated with refresh token was not found. UserId: {UserId}",storedToken.UserId);

                throw new UnauthorizedException("User not found.");
            }

            //change status of revoked

            storedToken.IsRevoked = true;
            storedToken.RevokedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Old refresh token revoked. UserId: {UserId}",user.Id);

            //Generat enew JWT and refresh token
            return await GenerateTokenAsync(user);
        }
    }
}
