using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ReCenterHub.Services;
using ReCenterHub.ViewModels;
using System.Security.Claims;

namespace ReCenterHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiAuthService _apiAuthService;

        public AccountController(ApiAuthService apiAuthService)
        {

            _apiAuthService = apiAuthService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {

                return View(model);

            }

            var result = await _apiAuthService.LoginAsync( model.Email, model.Password, model.AuthenticatorCode);

            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {

                ModelState.AddModelError(string.Empty,"Login failed. If Google Authenticator is enabled, enter your current six-digit code and try again.");

                return View(model);

            }

            var user = await _apiAuthService.GetCurrentUserAsync(result.AccessToken);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Could not load your account.");

                return View(model);
            }

            // Keep the API token for calls to the API.
            HttpContext.Session.SetString("AccessToken", result.AccessToken);

            // Sign the user in once so every [Authorize] page recognises them.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Email ?? model.Email),
                new Claim(ClaimTypes.Email, user.Email ?? model.Email)
            };

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");

        }
        [HttpGet]
        public IActionResult Register()
        {

            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            if (!ModelState.IsValid)
            {

                return View(model);

            }
            var result = await _apiAuthService.RegisterAsync(model.Email, model.Password);

            if (result)

            {

                return RedirectToAction("Login", "Account");

            }

            ModelState.AddModelError(string.Empty, "Registration failed.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {

            HttpContext.Session.Remove("AccessToken");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}