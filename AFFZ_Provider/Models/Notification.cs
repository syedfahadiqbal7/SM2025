namespace AFFZ_Provider.Models
{
    //handle adding, retrieving, and marking notifications as read.
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } // For Users in AFFZ_Customer
        public string MerchantId { get; set; } // For Providers in AFFZ_Provider
        public string Message { get; set; }
        public string ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string RedirectToActionUrl { get; set; }
        public int MessageFromId { get; set; }
        public string SenderType { get; set; }
        public string NotificationType { get; set; }
        public int SID { get; set; }
        // Constructor to set default values
        public Notification()
        {
            SenderType = "Provider";          // Default value for SenderType
            NotificationType = "Alert";    // Default value for NotificationType
        }
    }
}
