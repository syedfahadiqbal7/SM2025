using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFFZ_Customer.Models
{
    [Table("RequestStatuses")]
    public class RequestStatuses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StatusID { get; set; }

        [Required]
        [StringLength(100)]
        public string StatusName { get; set; }

        [Required]
        [StringLength(255)]
        public string StatusDescription { get; set; }
        [Required]
        [StringLength(255)]
        public string Usertype { get; set; }
    }
}
