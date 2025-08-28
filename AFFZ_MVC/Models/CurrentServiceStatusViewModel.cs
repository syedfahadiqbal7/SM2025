namespace AFFZ_Customer.Models
{
    public class CurrentServiceStatusViewModel
    {
        public int CurrentServiceStatusId { get; set; }  // Primary key, auto-increment
        public int UId { get; set; }                     // User ID
        public int MId { get; set; }                     // Merchant ID
        public int RFDFU { get; set; }                     // Service ID
        public string CurrentStatus { get; set; }        // Current status (e.g., Starting Process, Failed)
    }
}
