using System.ComponentModel.DataAnnotations;

namespace AFFZ_API.Models
{
    public class M_SericeDefaultDocumentList
    {
        [Key]
        public int MSericeDefaultDocumentListID { get; set; }
        public int ServiceID { get; set; }
        public int DocumentID { get; set; }
    }
}
