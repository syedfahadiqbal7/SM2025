using System.ComponentModel.DataAnnotations;

namespace AFFZ_Provider.Models
{
    public class M_SericeDocumentListBinding
    {
        [Key]
        public int Id { get; set; } // Consider adding a primary key if not present
        [Required]
        public int? CategoryID { get; set; }
        [Required]
        public int? ServiceDocumentListId { get; set; }
    }


}
