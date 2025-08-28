namespace AFFZ_Admin.Models;

public partial class SubMenu
{
    public int? subMenuId { get; set; }

    public string? subMenuName { get; set; } = null!;

    public string? subMenuUrl { get; set; } = null!;

    public string? description { get; set; }

    public int? menuId { get; set; }
}
