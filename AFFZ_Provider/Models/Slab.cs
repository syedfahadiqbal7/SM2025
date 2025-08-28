namespace AFFZ_Provider.Models
{
    public class Slab
    {
        public int SlabID { get; set; }
        public string? SlabName { get; set; }
        public double LowerLimit { get; set; }
        public double UpperLimit { get; set; }
        public decimal Fixed { get; set; } = 0.00m;  // Default value for Fixed
        public decimal Percentage { get; set; } = 0.00m;  // Default value for Percentage
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDefaultSlab { get; set; }
        // Many-to-Many relationship with MembershipPlans
        public List<MembershipPlanSlab> MembershipPlanSlabs { get; set; } = new List<MembershipPlanSlab>();
    }
}
