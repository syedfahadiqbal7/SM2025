namespace AFFZ_API.Models;

public partial class MerchantDashboard
{
    public int MerchantDashboardId { get; set; }

    public int DashboardItemId { get; set; }

    public int MerchantId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual DashboardItem DashboardItem { get; set; } = null!;

    public virtual Merchant Merchant { get; set; } = null!;
}
