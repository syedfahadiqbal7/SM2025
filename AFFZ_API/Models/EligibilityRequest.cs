namespace AFFZ_API.Models
{
    public class EligibilityRequest
    {
        public int RequestID { get; set; } // Primary key
        public int CustomerID { get; set; } // Foreign key to Customers
        public int ServiceID { get; set; } // Foreign key to Service
        public int MerchantID { get; set; } // Foreign key to ProviderUser
        public int StatusID { get; set; } // Foreign key to RequestStatuses
        public string RequestDetails { get; set; }
        public string Nationality { get; set; }
        public string ReasonForRejection { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsRequestRejected { get; set; }
        public bool IsRespondedByMerchant { get; set; }

        // Navigation properties
        public virtual Customers Customer { get; set; }
        public virtual Merchant Merchant { get; set; }
        public virtual Service Service { get; set; }
        public virtual RequestStatuses Status { get; set; }
        public virtual ICollection<EligibilityDocument> EligibilityDocuments { get; set; } = new List<EligibilityDocument>();
    }

}
