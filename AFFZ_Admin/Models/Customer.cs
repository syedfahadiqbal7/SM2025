
namespace AFFZ_Admin.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Password { get; set; } = null!;

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

    public string? ProfilePicture { get; set; }

    public DateTime? MemberSince { get; set; }

    public DateTime? DOB { get; set; }
}
