namespace AFFZ_Provider.Models
{
    public class MembershipPlanSlab
    {
        public int MembershipPlanId { get; set; }  // Foreign Key to MembershipPlans
        public MembershipPlan MembershipPlan { get; set; }

        public int SlabId { get; set; }  // Foreign Key to Slabs
        public Slab Slab { get; set; }
    }

}
