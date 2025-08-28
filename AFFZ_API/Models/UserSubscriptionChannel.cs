namespace AFFZ_API.Models;

public partial class UserSubscriptionChannel
{
    public int SubscriptionId { get; set; }

    public int? UserId { get; set; }

    public int? SubscriptionType { get; set; }
}
