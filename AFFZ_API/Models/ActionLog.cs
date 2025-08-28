namespace AFFZ_API.Models;

public partial class ActionLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public DateTime ActionTimestamp { get; set; }

    public string ActionType { get; set; } = null!;

    public string? ActionDetails { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual User User { get; set; } = null!;
}
