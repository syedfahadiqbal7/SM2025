namespace AFFZ_API.Models;

public partial class LoginHistory
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public DateTime LoginTimestamp { get; set; }

    public DateTime? LogoutTimestamp { get; set; }

    public string Ipaddress { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual User User { get; set; } = null!;
}
