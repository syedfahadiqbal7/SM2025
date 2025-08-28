namespace AFFZ_API.Models;

public partial class MerchantUserDashboard
{
    public int MerchantUserDashboardId { get; set; }

    public int DashboardItemId { get; set; }

    public int MerchantUserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual DashboardItem DashboardItem { get; set; } = null!;

    public virtual MerchantUser MerchantUser { get; set; } = null!;
}
