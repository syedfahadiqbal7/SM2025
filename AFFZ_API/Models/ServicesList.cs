using System.ComponentModel.DataAnnotations;

namespace AFFZ_API.Models
{
    public class ServicesList
    {
        [Key]
        public int ServiceListID { get; set; }
        public string ServiceName { get; set; }
        public string ServiceImage { get; set; }

    }
}
