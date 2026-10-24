using System.ComponentModel.DataAnnotations;

namespace ReCenterHub.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }


        [Display(Name = "Google Authenticator code")]
        [StringLength(7, MinimumLength = 6,
            ErrorMessage = "Enter a valid six-digit authenticator code.")]
        public string? AuthenticatorCode { get; set; }


    }
}