namespace AFFZ_Admin.Models;

public partial class Permission
{
    public int permissionId { get; set; }

    public int? roleId { get; set; }

    public int? menuId { get; set; }

    public bool canCreate { get; set; }

    public bool canRead { get; set; }

    public bool canUpdate { get; set; }

    public bool canDelete { get; set; }

    public bool canView { get; set; }

    public DateTime createdDate { get; set; } = DateTime.UtcNow;

    public int? createdBy { get; set; }

    public DateTime modifyDate { get; set; } = DateTime.UtcNow;

    public int? modifiedBy { get; set; }

    public virtual Menu? menu { get; set; }
}
