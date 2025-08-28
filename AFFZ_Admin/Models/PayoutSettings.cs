namespace AFFZ_Admin.Models
{
    public class PayoutSettings
    {
        public int Id { get; set; }
        public int PayoutNoOfDays { get; set; } // Number of days to process
        public int PayoutsPerMonth { get; set; } // Number of payouts per month
        public decimal MinimumPayoutAmount { get; set; } // Minimum payout amount
        public decimal PayoutCommission { get; set; } // Commission percentage
        public bool IsEnabled { get; set; } // Enable/Disable payout
        public bool AutoProcess { get; set; } // Auto process status
    }
}
