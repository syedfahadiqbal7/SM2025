namespace AFFZ_API.Models
{
    public class PayoutTransactionViewModel
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string? MerchantName { get; set; }
        public string? PayoutMethod { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
