using AutoMapper;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;
using EnterpriseCollegeManagement.IdentityService.Exceptions;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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


        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IMapper mapper , ILogger<AuthService> logger , ITokenService tokenService ,IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
            _auditService = auditService;
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

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
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

            _logger.LogInformation("User {Email} registered successfully.", request.Email);


            return new RegisterResponseDto
            {
                Success = true,
                Message = "User registered successfully."
            };
        }

        #region

      


        #endregion
    }
}
