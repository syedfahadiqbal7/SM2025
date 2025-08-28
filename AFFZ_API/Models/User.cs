namespace AFFZ_API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; }

    public int RoleId { get; set; }

    public int? GroupId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public string? PhoneNumber { get; set; }

    public int? SubscriptionChannelId { get; set; }

    public DateTime? Lastlogindate { get; set; }
    //public DateTime LastLoginDate { get; internal set; }
    public string? Rfu1 { get; set; }

    public string? Rfu2 { get; set; }

    public string? Rfu3 { get; set; }

    public string? Rfu4 { get; set; }

    public string? Rfu5 { get; set; }

    public string? Rfu6 { get; set; }

    public string? Rfu7 { get; set; }

    public string? Rfu8 { get; set; }

    public string? Rfu9 { get; set; }

    public string? Rfu10 { get; set; }

    public virtual ICollection<ActionLog> ActionLogs { get; set; } = new List<ActionLog>();

    public virtual ICollection<ChatMessage> ChatMessageCreatedByNavigations { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatMessage> ChatMessageModifiedByNavigations { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatMessage> ChatMessageReceivers { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatMessage> ChatMessageSenders { get; set; } = new List<ChatMessage>();

    public virtual ICollection<Document> DocumentCreatedByNavigations { get; set; } = new List<Document>();

    public virtual ICollection<Document> DocumentModifiedByNavigations { get; set; } = new List<Document>();

    public virtual ICollection<Document> DocumentUploadedByNavigations { get; set; } = new List<Document>();

    public virtual Group? Group { get; set; }

    public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    public virtual ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();

    public virtual ICollection<MerchantRating> MerchantRatings { get; set; } = new List<MerchantRating>();

    public virtual ICollection<MerchantUserRating> MerchantUserRatings { get; set; } = new List<MerchantUserRating>();

    public virtual Role Role { get; set; } = null!;
    public string FirstName { get; internal set; }
    public string LastName { get; internal set; }
}
