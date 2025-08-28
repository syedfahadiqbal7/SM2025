namespace AFFZ_API.Models;

public partial class SubMenu
{
    public int SubMenuId { get; set; }

    public string SubMenuName { get; set; } = null!;

    public string SubMenuUrl { get; set; } = null!;

    public string? Description { get; set; }

    public int? MenuId { get; set; }

    public virtual Menu? Menu { get; set; }
}
