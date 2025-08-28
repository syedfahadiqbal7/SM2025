namespace AFFZ_API.Models
{
    public class Wallet
    {
        public int WalletID { get; set; }
        public int CustomerID { get; set; }
        public decimal Points { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
