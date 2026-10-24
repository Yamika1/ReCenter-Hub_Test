using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using ReCenterHub.Services;
using ReCenterHub.ViewModels;

namespace ReCenterHub.Controllers
{
    [Authorize]
    public class SecurityController : Controller
    {
        private readonly TwoFactorApiService _service;

        public SecurityController(TwoFactorApiService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> SetupAuthenticator()
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _service.BeginSetupAsync(token);

                return View(CreateViewModel(result));
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Could not load authenticator setup. Please try again.";

                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupAuthenticator(
            TwoFactorSetupViewModel model)
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove(nameof(model.SharedKey));
            ModelState.Remove(nameof(model.AuthenticatorUri));

            if (!ModelState.IsValid)
            {
                await ReloadSetupAsync(model, token);
                return View(model);
            }

            try
            {
                var result = await _service.EnableAsync(token, model.Code);

                if (!result.IsTwoFactorEnabled)
                {
                    ModelState.AddModelError(
                        nameof(model.Code),
                        "The code could not be verified.");

                    await ReloadSetupAsync(model, token);
                    return View(model);
                }

                model.IsEnabled = true;
                model.Code = string.Empty;
                model.RecoveryCodes =
                    result.RecoveryCodes ?? new List<string>();

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    nameof(model.Code),
                    "The code was invalid or setup failed. Check the code and try again.");

                await ReloadSetupAsync(model, token);
                return View(model);
            }
        }

        private async Task ReloadSetupAsync(
            TwoFactorSetupViewModel model,
            string token)
        {
            var result = await _service.BeginSetupAsync(token);
            model.SharedKey = result.SharedKey ?? string.Empty;
            var rebuilt = CreateViewModel(result);
            model.AuthenticatorUri = rebuilt.AuthenticatorUri;
            model.QrCodeBase64 = rebuilt.QrCodeBase64;
        }


        private TwoFactorSetupViewModel CreateViewModel(
            TwoFactorApiResponse result)
        {
            var key = (result.SharedKey ?? string.Empty)
                .Replace(" ", "")
                .Replace("-", "");

            var accountEmail = User.FindFirst(
                System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.Identity?.Name
                ?? "user";

            var issuer = Uri.EscapeDataString("Re-Center Hub");
            var account = Uri.EscapeDataString(accountEmail);

            var authenticatorUri =
                $"otpauth://totp/{issuer}:{account}" +
                $"?secret={key}&issuer={issuer}&digits=6";

            var qrCodeBase64 = string.Empty;

            if (!string.IsNullOrEmpty(key))
            {
                using var generator = new QRCodeGenerator();
                using var qrData = generator.CreateQrCode(
                    authenticatorUri, QRCodeGenerator.ECCLevel.Q);
                var png = new PngByteQRCode(qrData).GetGraphic(10);
                qrCodeBase64 = Convert.ToBase64String(png);
            }

            return new TwoFactorSetupViewModel
            {
                SharedKey = result.SharedKey ?? string.Empty,
                AuthenticatorUri = authenticatorUri,
                QrCodeBase64 = qrCodeBase64,
                IsEnabled = result.IsTwoFactorEnabled
            };
        }

    }
}