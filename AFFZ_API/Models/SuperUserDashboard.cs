namespace AFFZ_API.Models;

public partial class SuperUserDashboard
{
    public int SuperUserDashboardId { get; set; }

    public int DashboardItemId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual DashboardItem DashboardItem { get; set; } = null!;
}
