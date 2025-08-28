namespace AFFZ_API.Models
{
    public class PaymentRequest
    {
        public int UserId { get; set; }
        public int ServiceId { get; set; }
        public int MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "AED";
        public string PaymentMethod { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RFDFU { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public PaymentHistory? PaymentRecord { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class PaymentRefundRequest
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RefundMethod { get; set; } = string.Empty;
    }

    public class PaymentRefundResult
    {
        public bool IsSuccess { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public decimal RefundedAmount { get; set; }
    }
}
