namespace AFFZ_Provider.Models
{
    public class ServiceReviewViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewDetailDto> Reviews { get; set; } = new();
    }

    public class ReviewDetailDto
    {
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}
