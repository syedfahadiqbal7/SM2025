namespace AFFZ_API.Models;

public partial class MerchantUser
{
    public int MerchantUserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int MerchantId { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public int? GroupId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public bool? IsAdmin { get; set; }

    public DateTime? Lastlogindate { get; set; }

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

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Group? Group { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual ICollection<MerchantUserDashboard> MerchantUserDashboards { get; set; } = new List<MerchantUserDashboard>();

    public virtual ICollection<MerchantUserRating> MerchantUserRatings { get; set; } = new List<MerchantUserRating>();

    public virtual Role Role { get; set; } = null!;
}
