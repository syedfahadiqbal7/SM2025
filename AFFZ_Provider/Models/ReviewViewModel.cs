namespace AFFZ_Provider.Models
{
    public class ReviewViewModel
    {
        public int ReviewId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceImageUrl { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UserAvatarUrl { get; set; }
        public DateTime ReviewDate { get; set; }
        public int SID { get; set; }
        public int merchantId { get; set; }
        public int RFDFU { get; set; }
    }
}
