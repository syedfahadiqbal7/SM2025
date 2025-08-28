namespace AFFZ_Provider.Models
{
    public partial class Merchant
    {
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
    }
}
