
using System.ComponentModel.DataAnnotations;

namespace AFFZ_API.Models
{
    public class AmountChangeRequests
    {
        [Key]
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int ProviderId { get; set; }
        public decimal RequestedAmount { get; set; }
        public string? Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAt { get; set; }
    }
}