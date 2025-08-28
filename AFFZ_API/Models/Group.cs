namespace AFFZ_API.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();

    public virtual ICollection<MerchantUser> MerchantUsers { get; set; } = new List<MerchantUser>();

    public virtual ICollection<UserGroupPermission> UserGroupPermissions { get; set; } = new List<UserGroupPermission>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
