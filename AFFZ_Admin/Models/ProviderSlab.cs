using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFFZ_Admin.Models
{
    public class ProviderSlab
    {
        [Key]
        public int ProviderSlabID { get; set; }

        [Required]
        [ForeignKey("ProviderUser")]
        public int ProviderID { get; set; }


        [Required]
        [StringLength(20)]
        public string SelectedModel { get; set; } // 'Percentage' or 'Fixed'

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ProviderUser ProviderUser { get; set; }
    }
}
