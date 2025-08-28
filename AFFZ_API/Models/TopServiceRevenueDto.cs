namespace AFFZ_API.Models
{
    public class TopServiceRevenueDto
    {
        public int? ServiceId { get; set; } // Changed to nullable int
        public string? ServiceName { get; set; } // Changed to nullable int
        public decimal TotalRevenue { get; set; }
    }
}
