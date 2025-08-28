namespace AFFZ_API.Models
{
    public class PayoutTransaction
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string PayoutMethod { get; set; } = string.Empty; // e.g., Stripe, Paypal, Bank Transfer
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for merchant details
        public Merchant Merchant { get; set; }
    }

}
