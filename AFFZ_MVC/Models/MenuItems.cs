namespace AFFZ_Customer.Models
{
    public class MenuItems
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; } = null!;

        public string MenuUrl { get; set; } = null!;

        public string? Description { get; set; }

        public string? MenuIcon { get; set; }
    }
}
