namespace AFFZ_API.Models;

public partial class DashboardItem
{
    public int DashboardItemId { get; set; }

    public string DashboardItemType { get; set; } = null!;

    public string DashboardItemName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual ICollection<AdminDashboard> AdminDashboards { get; set; } = new List<AdminDashboard>();

    public virtual ICollection<MerchantDashboard> MerchantDashboards { get; set; } = new List<MerchantDashboard>();

    public virtual ICollection<MerchantUserDashboard> MerchantUserDashboards { get; set; } = new List<MerchantUserDashboard>();

    public virtual ICollection<SuperUserDashboard> SuperUserDashboards { get; set; } = new List<SuperUserDashboard>();
}
