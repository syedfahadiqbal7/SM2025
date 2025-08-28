using System.ComponentModel.DataAnnotations.Schema;

namespace AFFZ_Admin.Models
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

        // Many-to-Many Relationship
        public List<MembershipPlanSlab> MembershipPlanSlabs { get; set; } = new List<MembershipPlanSlab>();

        // Helper property for multiple slab selection
        [NotMapped]
        public List<int> SelectedSlabIds { get; set; } = new List<int>();
    }



}
