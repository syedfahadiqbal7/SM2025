namespace AFFZ_API.Models;

public partial class MerchantRating
{
    public int RatingId { get; set; }

    public int RatedByUserId { get; set; }

    public int RatedMerchantId { get; set; }

    public int RatingValue { get; set; }

    public string? Comments { get; set; }

    public DateTime RatedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime ModifyDate { get; set; }

    public int ModifiedBy { get; set; }

    public virtual User RatedByUser { get; set; } = null!;

    public virtual Merchant RatedMerchant { get; set; } = null!;
}
