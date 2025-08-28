namespace AFFZ_API.Models;

public partial class Policy
{
    public int PolicyId { get; set; }

    public string PolicyName { get; set; } = null!;

    public string? PolicyContent { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }
}
