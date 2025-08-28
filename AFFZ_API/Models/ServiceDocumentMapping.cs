namespace AFFZ_API.Models
{
    public class ServiceDocumentMapping
    {
        public int ServiceID { get; set; }
        public int ServiceDocumentListId { get; set; }

        public Service? Service { get; set; }
        public ServiceDocumenList? ServiceDocumentList { get; set; }
    }
}
