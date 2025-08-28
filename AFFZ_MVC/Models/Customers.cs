using System.ComponentModel.DataAnnotations;

namespace AFFZ_Customer.Models;

public partial class Customers
{
    public int? CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? Email { get; set; }

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public int? ModifiedBy { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? PostalCode { get; set; }
    public string? ReferrerCode { get; set; }

    public string? ProfilePicture { get; set; }

    public DateTime? MemberSince { get; set; }

    public DateOnly? DOB { get; set; }

    public IFormFile? ProfileImage { get; set; }

    public string? VerificationToken { get; set; } // Verification token for email
    public DateTime? TokenExpiry { get; set; }    // Expiry date for token
    public bool IsEmailVerified { get; set; }     // Email verification flag
}

public class CForgotPasswordModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
}


public class CResetPasswordModel
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