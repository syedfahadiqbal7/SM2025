using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFFZ_Provider.Models
{
    [Table("TrackServiceStatusHistory")]
    public class TrackServiceStatusHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int HistoryID { get; set; }
        [Required]
        public int RFDFU { get; set; }
        [Required]
        public int StatusID { get; set; }
        [Required]
        public int ChangedByID { get; set; }

        [Required]
        public DateTime ChangedOn { get; set; }

        [Required]
        public string Comments { get; set; }
        [Required]
        public string ChangedByUserType { get; set; }
    }
}
