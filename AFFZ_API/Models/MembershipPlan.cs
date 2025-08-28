using System.ComponentModel.DataAnnotations.Schema;

namespace AFFZ_API.Models
{
    public class MembershipPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = "Monthly"; // Monthly/Yearly
        public int ServicesLimit { get; set; }
        public int StaffLimit { get; set; }
        public string AppointmentsLimit { get; set; } = "Unlimited";
        public bool Gallery { get; set; }
        public bool AdditionalServices { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property for many-to-many relationship
        public ICollection<MembershipPlanSlabs> MembershipPlanSlabs { get; set; } = new List<MembershipPlanSlabs>();
        // Helper property for managing multiple slab selection
        [NotMapped]
        public List<int> SelectedSlabIds { get; set; } = new List<int>();
    }

}
