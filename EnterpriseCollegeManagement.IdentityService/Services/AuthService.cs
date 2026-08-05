using AutoMapper;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Responses;
using EnterpriseCollegeManagement.IdentityService.Entities;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseCollegeManagement.IdentityService.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMapper mapper , ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
           _logger = logger;
        }
        public Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            throw new NotImplementedException();
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
                return new RegisterResponseDto
                { 
                    Success = false,
                    Message = "Email already exists."

                };

            }
            if(request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning("Password mismatch for {Email}", request.Email);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Password and Confirm Password do not match."

                };

            }

            var newUser = _mapper.Map<ApplicationUser>(request);
            newUser.UserName = request.Email;
            _logger.LogInformation("Creating user {Email}", request.Email);

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for {Email}. Errors: {Errors}", request.Email,
                    string.Join(",", result.Errors.Select(x => x.Description)));
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            _logger.LogInformation("User {Email} registered successfully.", request.Email);


            return new RegisterResponseDto
            {
                Success = true,
                Message = "User registered successfully."
            };
        }
    }
}
