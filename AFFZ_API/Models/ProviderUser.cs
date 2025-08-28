using System.ComponentModel.DataAnnotations;

namespace AFFZ_API.Models;

public partial class ProviderUser
{
    public int ProviderId { get; set; }

    public string? ProviderName { get; set; } = null!;

    public string? Email { get; set; } = null!;

    public string? Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public int? ModifiedBy { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ProfilePicture { get; set; }

    public string? Address { get; set; }

    public string? PostalCode { get; set; }

    public string? Passport { get; set; }

    public string? EmiratesId { get; set; }

    public string? DrivingLicense { get; set; }

    public DateTime? MemberSince { get; set; }
    public bool isVerified { get; set; }
    public virtual Role? Role { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? TokenExpiry { get; set; }

}

public class ForgotPasswordModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
}

public class ResetPasswordModel
{
    public string Token { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [RegularExpression(
        "^(?=.*[A-Z])(?=.*[!@#$&*])(?=.*[0-9])(?=.*[a-z]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters, include an uppercase letter, a symbol, and a number.")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}

