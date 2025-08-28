using AFFZ_API.Utils;

namespace AFFZ_API.Models;

public partial class ChatMessage
{
    public int MessageId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string MessageType { get; set; } = null!;
    [EmailOrPhoneNotAllowed]
    public string MessageContent { get; set; } = null!;
    public DateTime MessageTimestamp { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime ModifyDate { get; set; }
    public int ModifiedBy { get; set; }
    public int? MerchantId { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual Merchant? Merchant { get; set; }
    public virtual User ModifiedByNavigation { get; set; } = null!;
    public virtual User Receiver { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
}
