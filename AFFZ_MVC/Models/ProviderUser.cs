namespace AFFZ_Customer.Models;

public partial class ProviderUser
{
    public int ProviderId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateOnly? DOB { get; set; }

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

    public IFormFile? ProfileImage { get; set; }

    public IFormFile? PassportFile { get; set; }

    public IFormFile? EmiratesIdFile { get; set; }

    public IFormFile? DrivingLicenseFile { get; set; }

    public virtual object Role { get; set; }

}
