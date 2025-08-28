namespace AFFZ_Admin.Models
{
    public partial class MembershipPaymentHistory
    {
        public int ID { get; set; }
        public string PAYMENTTYPE { get; set; }
        public string AMOUNT { get; set; }
        public int PAYERID { get; set; }
        public int ISPAYMENTSUCCESS { get; set; }
        public int Quantity { get; set; }
        public DateTime PAYMENTDATETIME { get; set; }
        public string Duration { get; set; }
        public int MembershipId { get; set; }
        public string MemberShipname { get; set; }
    }
}
