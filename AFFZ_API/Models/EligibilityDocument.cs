namespace AFFZ_API.Models
{
    public class EligibilityDocument
    {
        public int DocumentID { get; set; }
        public int RequestID { get; set; }
        public int FileID { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string ReasonForRejection { get; set; }
        public DateTime UploadedDate { get; set; }

        // Navigation properties
        public EligibilityRequest EligibilityRequest { get; set; }
        public UploadedFile File { get; set; }
    }
}
