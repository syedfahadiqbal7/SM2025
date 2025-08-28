using AFFZ_API.Utils;

namespace AFFZ_API.Models;

public partial class ChatterMessage
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
}
public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTime { get; set; }
}