using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using EnterpriseCollegeManagement.IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCollegeManagement.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService  _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);

           
            return Ok(result);
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
           
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            _logger.LogInformation("Refresh token endpoint called.");

            var result = await _tokenService.RefreshTokenAsync(request.RefreshToken);

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            await _tokenService.LogoutAsync(request.RefreshToken);
            return Ok(new
            {
                Success = true,
                Message = "Logged out successfully."
            });
        }
    }
}
