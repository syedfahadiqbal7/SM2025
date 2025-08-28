using System.ComponentModel.DataAnnotations.Schema;

namespace AFFZ_API.Models
{
    public class ReviewSummaryDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string ReviewsJson { get; set; }

        [NotMapped]
        public List<ReviewDetailDto> Reviews { get; set; }
    }

    public class ReviewDetailDto
    {
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}
