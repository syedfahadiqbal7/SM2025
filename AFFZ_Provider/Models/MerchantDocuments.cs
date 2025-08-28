namespace AFFZ_Provider.Models
{
    public class MerchantDocuments
    {
        public int MDID { get; set; }                // Primary Key for MerchantDocuments
        public string? FileName { get; set; }          // File name of the uploaded document
        public string? ContentType { get; set; }       // Content type (e.g., image/jpeg, application/pdf)
        public long FileSize { get; set; }            // Size of the uploaded file
        public string? FolderName { get; set; }        // Directory where the file is stored
        public string? Status { get; set; }            // Verification status (e.g., "Verified", "Not Yet Verified", "Under Review")
        public DateTime DocumentAddedDate { get; set; }    // Date when the document was added
        public DateTime? DocumentModifiedDate { get; set; }  // Date when the document was modified
        public int MerchantId { get; set; }           // Foreign key to link to Merchant
        public string? UploadedBy { get; set; }        // The ID of the user who uploaded the document

        // Additional properties if needed
    }
}
