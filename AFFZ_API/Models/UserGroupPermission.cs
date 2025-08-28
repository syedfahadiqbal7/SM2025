namespace AFFZ_API.Models;

public partial class UserGroupPermission
{
    public int UserGroupPermissionId { get; set; }

    public int GroupId { get; set; }

    public int PermissionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;
}
