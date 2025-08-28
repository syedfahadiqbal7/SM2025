namespace AFFZ_API.Models
{
    public class ProviderMerchant
    {
        public int ProviderMerchantID { get; set; }
        public int ProviderID { get; set; }
        public int MerchantID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
