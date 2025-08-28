using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFFZ_API.Models;

public partial class Merchant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MerchantId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string ContactInfo { get; set; } = null!;

    public string? RegistrationMethod { get; set; }

    public bool IsActive { get; set; }

    public string? CompanyRegistrationNumber { get; set; }

    public string TradingLicense { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public string? EmiratesId { get; set; }

    public bool? Deactivate { get; set; }

    public string? MerchantLocation { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<MerchantDashboard> MerchantDashboards { get; set; } = new List<MerchantDashboard>();

    public virtual ICollection<MerchantRating> MerchantRatings { get; set; } = new List<MerchantRating>();

    public virtual ICollection<MerchantUser> MerchantUsers { get; set; } = new List<MerchantUser>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<EligibilityRequest> MerchantRequests { get; set; } = new List<EligibilityRequest>();
}
