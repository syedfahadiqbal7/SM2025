namespace AFFZ_API.Models;

public partial class MenuPermission
{
    public int MenuPermissionId { get; set; }

    public int RoleId { get; set; }

    public int? UserId { get; set; }

    public int? GroupId { get; set; }

    public int MenuId { get; set; }

    public int PermissionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual Group? Group { get; set; }

    public virtual Menu Menu { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual User? User { get; set; }
}
