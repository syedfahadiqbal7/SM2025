using System.ComponentModel.DataAnnotations;

namespace AFFZ_API.Models
{
    public class MerchantVerificationDocumentList
    {
        [Key]
        public int MerchantVerificationDocumenListtId { get; set; }
        public string? MerchantVerificationDocumentName { get; set; }
        //public int DocumentId { get; set; }
        //public string? DocumentName { get; set; }
        //public string? Status { get; set; }
        //public string? FilePath { get; set; }
        //public string? FileType { get; set; }
    }
}
