using System.ComponentModel.DataAnnotations;

namespace ReCenterHub.ViewModels
{
    public class TwoFactorSetupViewModel
    {
        public string SharedKey { get; set; } = string.Empty;

        public string AuthenticatorUri { get; set; } = string.Empty;

        [Required]
        [StringLength(7, MinimumLength = 6)]
        [Display(Name = "Google Authenticator code")]
        public string Code { get; set; } = string.Empty;

        public bool IsEnabled { get; set; }

        public List<string> RecoveryCodes { get; set; } = new();
    }
}