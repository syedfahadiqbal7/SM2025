namespace AFFZ_API.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public int? RoleId { get; set; }

    public int? MenuId { get; set; }

    public bool? CanCreate { get; set; }

    public bool? CanRead { get; set; }

    public bool? CanUpdate { get; set; }

    public bool? CanDelete { get; set; }

    public bool? CanView { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifyDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Menu? Menu { get; set; }

    public virtual Role? Role { get; set; }
}
