using System.ComponentModel.DataAnnotations;

namespace AFFZ_API.Models
{
    public class M_SericeDocumentListBinding
    {
        [Key]
        public int Id { get; set; } // Consider adding a primary key if not present
        public int? CategoryID { get; set; }
        public int? ServiceDocumentListId { get; set; }
    }
}
