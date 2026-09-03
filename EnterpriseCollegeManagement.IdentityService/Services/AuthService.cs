using AutoMapper;
using EnterpriseCollegeManagement.IdentityService.Data;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;
using EnterpriseCollegeManagement.IdentityService.Exceptions;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EnterpriseCollegeManagement.IdentityService.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        private readonly ITokenService _tokenService;

        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IMapper mapper , ILogger<AuthService> logger , ITokenService tokenService ,IAuditService auditService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
            _auditService = auditService;
            _context = context;
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            _logger.LogInformation( "Password change requested. UserId: {UserId}", userId);

            if(request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("Password change failed due to password confirmation mismatch. UserId: {UserId}", userId);

                throw new BadRequestException(
                    "New password and confirm password do not match.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                _logger.LogWarning("Password change failed. User not found. UserId: {UserId}",userId);

                throw new NotFoundException("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            await _auditService.LogAsync(
                            user.Id,
                            "PasswordChanged",
                            "ApplicationUser",
                            user.Id,
                            "User password was changed successfully.",
                            null,
                            null);

            var refreshTokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();

            foreach(var refreshToken in refreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();


            if (!result.Succeeded)
            {
                var error = string.Join(" ", result.Errors.Select(x => x.Description));

                _logger.LogWarning("Password change failed. UserId: {UserId}. Errors: {Errors}", userId,error);

                throw new BadRequestException(error);
            }


            _logger.LogInformation("Password changed successfully and active refresh tokens revoked. UserId: {UserId}, RevokedTokenCount: {Count}",
                userId,
                refreshTokens.Count);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            _logger.LogInformation("Forgot password request received for {Email}",request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if(user == null)
            {
                _logger.LogWarning("Forgot password requested for a non-existing account.");

                
                return;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink =
                       $"https://localhost:7219/api/Auth/reset-password" +
                       $"?email={Uri.EscapeDataString(user.Email!)}" +
                       $"&token={Uri.EscapeDataString(token)}";

            _logger.LogInformation(
                "Password reset link generated successfully. UserId: {UserId}",
                user.Id);
            //for Development
            Console.WriteLine("======================================");
            Console.WriteLine("PASSWORD RESET LINK");
            Console.WriteLine(resetLink);
            Console.WriteLine("======================================");
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt started for email: {Email}", request.Email);
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed. No user found with email: {Email}", request.Email);

                throw new UnauthorizedException("Invalid email or password.");

            }
            _logger.LogInformation("Verifying password for user: {Email}", request.Email);
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed. Invalid password for email: {Email}", request.Email);

                throw new UnauthorizedException("Invalid email or password.");
            }

           await _auditService.LogAsync(
                user.Id,
                "Register",
                "ApplicationUser",
                user.Id,
                "User registered successfully."
                );

            

            _logger.LogInformation("User {Email} logged in successfully.", request.Email);


            var tokenResult =  await _tokenService.GenerateTokenAsync(user);

            await _auditService.LogAsync(

                user.Id,
                "Login",
                "ApplicationUser",
                user.Id,
                "User logged in successfully."


                );

            return new LoginResponseDto
            {
                Success = true,
                Message = "Login successful.", 
                Token = tokenResult.Token,
                Expiration = tokenResult.Expiration,
            };

           

        }

        

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("Registration started for {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed. Email already exists: {Email}", request.Email);


                throw new ConflictException("Email already exists.");

            }
            if(request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning("Password mismatch for {Email}", request.Email);


                throw new BadRequestException("Password and Confirm Password do not match.");

            }

            var newUser = _mapper.Map<ApplicationUser>(request);
            newUser.UserName = request.Email;
           
            _logger.LogInformation("Creating user {Email}", request.Email);

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                _logger.LogWarning( "Registration failed for {Email}. Errors: {Errors}", request.Email, errors);

                throw new BadRequestException(errors);
            }

            //default role assign

            var roleResult = await _userManager.AddToRoleAsync(newUser, "Student");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(e => e.Description));

                _logger.LogError(
                    "Failed to assign Student role to user {UserId}. Errors: {Errors}",
                    newUser.Id,
                    errors);

                throw new BadRequestException("User created but failed to assign default Student role.");
            }
            _logger.LogInformation("User {Email} registered successfully.", request.Email);


            return new RegisterResponseDto
            {
                Success = true,
                Message = "User registered successfully."
            };
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            _logger.LogInformation("Password reset request received.");

            if(request.NewPassword !=  request.ConfirmPassword)
            {
                _logger.LogWarning("Password reset failed due to password confirmation mismatch.");

                throw new BadRequestException(
                    "New password and confirm password do not match.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if(user == null)
            {
                _logger.LogWarning( "Password reset failed. User not found.");

                throw new BadRequestException("Invalid password reset request.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if(!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(x => x.Description));

                _logger.LogWarning("Password reset failed. UserId: {UserId}. Errors: {Errors}", user.Id, errors);

                throw new BadRequestException(errors);
            }

          await _auditService.LogAsync(
            user.Id,
            "PasswordReset",
            "ApplicationUser",
            user.Id,
            "User password was reset successfully.",
            null,
            null);

            //revoke refresh Token

            var refreshTokens = await _context.RefreshTokens.Where(x => x.UserId == user.Id && !x.IsRevoked).ToListAsync();

            foreach(var refreshToken in refreshTokens)
            {
                refreshToken.IsRevoked= true;
                refreshToken.RevokedDate= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Password reset successfully. Active refresh tokens revoked. UserId: {UserId}, RevokedTokenCount: {Count}",
                user.Id,
                refreshTokens.Count);
        }

        #region





        #endregion
    }
}
