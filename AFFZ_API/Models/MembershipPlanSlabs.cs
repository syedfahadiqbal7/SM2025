using System.Text.Json.Serialization;

namespace AFFZ_API.Models
{
    public class MembershipPlanSlabs
    {
        public int MembershipPlanId { get; set; }  // Foreign Key to MembershipPlans

        [JsonIgnore]  // Prevent serialization of MembershipPlan to avoid self-referencing loop
        public MembershipPlan MembershipPlan { get; set; }

        public int SlabId { get; set; }  // Foreign Key to Slabs
        public Slab Slab { get; set; }
    }

}
