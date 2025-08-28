namespace AFFZ_API.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<AdminUser>? AdminUsers { get; set; } = new List<AdminUser>();

    public virtual ICollection<Customers>? Customers { get; set; } = new List<Customers>();

    public virtual ICollection<ProviderUser>? MerchantUsers { get; set; } = new List<ProviderUser>();

    public virtual ICollection<Permission>? Permissions { get; set; } = new List<Permission>();
}
