using System.ComponentModel.DataAnnotations;

namespace AFFZ_Provider.Models.Partial
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; }
    }
}
