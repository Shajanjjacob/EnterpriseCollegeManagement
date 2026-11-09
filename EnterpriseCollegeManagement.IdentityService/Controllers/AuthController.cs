using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.Interfaces;
using EnterpriseCollegeManagement.IdentityService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Claims;


namespace EnterpriseCollegeManagement.IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        private readonly GoogleLoginCodeStore _googleLoginCodeStore;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ITokenService tokenService, ILogger<AuthController> logger, GoogleLoginCodeStore googleLoginCodeStore, IConfiguration configuration)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
            _googleLoginCodeStore = googleLoginCodeStore;
            _configuration = configuration;
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

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning(
                    "Unable to identify authenticated user for password change.");

                return Unauthorized();
            }
            await _authService.ChangePasswordAsync(userId, request);



            return Ok(new
            {
                Success = true,
                Message = "Password changed successfully."
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            _logger.LogInformation("Forgot password request received for {Email}", request.Email);

            await _authService.ForgotPasswordAsync(request);

            return Ok(new
            {
                Success = true,
                Message = "If an account exists for this email, a password reset link has been generated."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            _logger.LogInformation("Password reset request received.");

            await _authService.ResetPasswordAsync(request);

            return Ok(new
            {
                Success = true,
                Message = "Password has been reset successfully."
            });
        }


        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                //RedirectUri = "/api/Auth/google-callback"
                RedirectUri = "/identity/api/Auth/google-callback"
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Google authentication failed.");

                return Unauthorized(new
                {
                    Success = false,
                    Message = "Google authentication failed."
                });
            }

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);

            var name = result.Principal?.FindFirstValue(ClaimTypes.Name);

            var googleUserId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleUserId))
            {
                _logger.LogWarning("Required Google user information was not found.");

                return BadRequest(new
                {
                    Success = false,
                    Message = "Unable to retrieve Google account information."
                });
            }

            var tokenResult = await _authService.GoogleLoginAsync(email, googleUserId, name);


            // Store token response and generate temporary handoff(BASED ON CODE FOR SHAREING TO FRONTEND)
            var handoff = _googleLoginCodeStore.Store(tokenResult);  //calling that function 

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var portalUrl = _configuration["PortalSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(portalUrl))
            {
                throw new InvalidOperationException("Portal BaseUrl is not configured.");
            }

            return Redirect($"{portalUrl.TrimEnd('/')}/Account/GoogleCallback?handoff={Uri.EscapeDataString(handoff)}");
            //handoff code is redirect to frontend and Portal receives it through the callback parameter
            // after that Portal sends that received code back to IdentityService and it check (like match code )
        }

        [HttpGet("google-token")]
        public IActionResult GetGoogleLoginToken([FromQuery] string handoff) //handoff receive from portal 
        {

            if (string.IsNullOrWhiteSpace(handoff))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Handoff value is required."
                });
            }

            var isValid = _googleLoginCodeStore.TryTake(handoff, out var tokenResult);

            if (!isValid || tokenResult == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Invalid or expired Google login request."
                });
            }

            return Ok(tokenResult);
        }
    }
}
