namespace AFFZ_Admin.Models
{
    public class CustomerTransferViewModel
    {
        public int ID { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public int MerchantID { get; set; }
        public string CompanyName { get; set; }
        public bool IsPaymentSuccess { get; set; }
        public int ServiceID { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public int Quantity { get; set; }
        public int RFDFU { get; set; }
        public string ServiceImage { get; set; }
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
    }
}
