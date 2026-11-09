using EnterpriseCollegeManagement.Portal.Clients;
using EnterpriseCollegeManagement.Portal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnterpriseCollegeManagement.Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIdentityApiClient _identityApiClient;
        private readonly IGoogleAuthApiClient _googleAuthApiClient;


        public AccountController(IIdentityApiClient identityApiClient, IGoogleAuthApiClient googleAuthApiClient)
        {
            _identityApiClient = identityApiClient;
            _googleAuthApiClient = googleAuthApiClient;
        }

        [HttpGet] //for getting Registerview
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //prevent Cross-Site Request Forgery (CSRF) attacks
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _identityApiClient.RegisterAsync(model, cancellationToken);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;  //to show message in login ppage 
                return Redirect("Login");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _identityApiClient.LoginAsync(model);

            if (result.Success)
            {
                //TempData["SuccessMessage"] = result.Message;
                //return Redirect("Home");
                //TempData["LoginExpiration"] = result.Expiration?.ToString("O");
                //TempData["RefrshtokenExpiration"] = result.RefreshTokenExpiration?.ToString("O");



                if (string.IsNullOrWhiteSpace(result.Token))
                {
                    ModelState.AddModelError(string.Empty, "Login succeeded, but no access token was returned.");
                    return View(model);

                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,model.Email)
                };

                if (!string.IsNullOrEmpty(result.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, result.Role));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                //stroring token in server side session 

                HttpContext.Session.SetString("AccessToken", result.Token);//jwt

                if (!string.IsNullOrWhiteSpace(result.RefreshToken))
                {
                    HttpContext.Session.SetString("RefreshToken", result.RefreshToken);
                }

                if (result.Expiration.HasValue)
                {
                    HttpContext.Session.SetString("AccessTokenExpiration", result.Expiration.Value.ToString("O"));
                }
                if (result.RefreshTokenExpiration.HasValue)
                {
                    HttpContext.Session.SetString("RefreshTokenExpiration", result.RefreshTokenExpiration.Value.ToString("O"));
                }


                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                TempData["SuccessMessage"] = result.Message;
                return RedirectToRoleHome(result.Role);
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
     
        public async Task<IActionResult> GoogleCallback([FromQuery] string handoff, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(handoff))
            {
                TempData["ErrorMessage"] = "Google login reference is missing.";
                return RedirectToAction("Login");
            }

            var result = await _googleAuthApiClient.GetGoogleLoginResponseAsync(handoff, cancellationToken);

            if(result == null || !result.Success || string.IsNullOrWhiteSpace(result.Token))
            {
                TempData["ErrorMessage"] = result?.Message ?? "Google login failed.";

                return RedirectToAction("Login");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.Email ?? "GoogleUser"),
                new Claim(ClaimTypes.Email, result.Email ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(result.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, result.Role));
            }

            var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            //storeing token in server side 
            HttpContext.Session.SetString("AccessToken", result.Token);

            if (!string.IsNullOrWhiteSpace(result.RefreshToken))
            {
                HttpContext.Session.SetString("RefreshToken", result.RefreshToken);
            }

            if (result.Expiration.HasValue)
            {
                HttpContext.Session.SetString("AccessTokenExpiration",  result.Expiration.Value.ToString("O"));
            }

            if (result.RefreshTokenExpiration.HasValue)
            {
                HttpContext.Session.SetString("RefreshTokenExpiration",result.RefreshTokenExpiration.Value.ToString("O"));
            }

            //cookie authentication

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            TempData["SuccessMessage"] = result.Message;

            return RedirectToRoleHome(result.Role);
        }



        //redirect to user dashboard 
        private IActionResult RedirectToRoleHome(string role)
        {
           if(role == "Admin")
            {
                return RedirectToAction("Index", "AdminDashboard");
            }
            else if(role == "Teacher")
            {
                return RedirectToAction("Index", "TeacherDashboard");
            }
           else if(role == "Student")
            {
                return RedirectToAction("Index", "StudentDashboard");

            }
           else
            {
                return RedirectToAction("Login");
            }
        }
    }
}
