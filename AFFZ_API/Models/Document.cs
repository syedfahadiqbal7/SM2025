namespace AFFZ_API.Models;

public partial class Document
{
    public int DocumentId { get; set; }

    public int UploadedBy { get; set; }

    public string DocumentName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public int? MerchantId { get; set; }

    public int? MerchantUserId { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Merchant? Merchant { get; set; }

    public virtual MerchantUser? MerchantUser { get; set; }

    public virtual User ModifiedByNavigation { get; set; } = null!;

    public virtual User UploadedByNavigation { get; set; } = null!;
}
