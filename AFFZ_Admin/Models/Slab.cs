using System.ComponentModel.DataAnnotations;

namespace AFFZ_Admin.Models
{
    public class Slab
    {
        [Key]
        public int SlabID { get; set; }
        [Required]
        public string? SlabName { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "LowerLimit must be a positive number.")]
        public double LowerLimit { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "UpperLimit must be a positive number.")]
        public double UpperLimit { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Fixed amount must be a positive number.")]
        public decimal Fixed { get; set; } = 0.00m;  // Default value for Fixed

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Percentage amount must be a positive number.")]
        public decimal Percentage { get; set; } = 0.00m;  // Default value for Percentage

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }
        public bool IsDefaultSlab { get; set; }
        // Many-to-Many relationship with MembershipPlans
        public List<MembershipPlanSlab> MembershipPlanSlabs { get; set; } = new List<MembershipPlanSlab>();
    }
}
