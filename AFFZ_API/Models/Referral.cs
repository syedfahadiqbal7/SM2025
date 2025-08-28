namespace AFFZ_API.Models
{
    public class Referral
    {
        public int ReferralID { get; set; }
        public int ReferrerCustomerID { get; set; }
        public int? ReferredCustomerID { get; set; }
        public string ReferralCode { get; set; }
        public string ReferralStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
