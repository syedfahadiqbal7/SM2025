namespace AFFZ_API.Models;

public partial class UserRolePermission
{
    public int UserRolePermissionId { get; set; }

    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public bool CanCreate { get; set; }

    public bool CanRead { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
