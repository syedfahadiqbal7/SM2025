namespace AFFZ_Admin.Models;

public class Menu
{
    public int MenuId { get; set; }

    public string? MenuName { get; set; } = null!;

    public string? MenuUrl { get; set; } = null!;

    public string? Description { get; set; }

    public string? MenuIcon { get; set; }
    public string? UserType { get; set; }
    public int? IsVisible { get; set; }
    public virtual List<Permission>? permissions { get; set; } = new List<Permission>();

    public virtual List<SubMenu>? subMenus { get; set; } = new List<SubMenu> { };
}
