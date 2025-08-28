using System.ComponentModel.DataAnnotations;

namespace AFFZ_Admin.Models
{
    public class ServicesListViewModel
    {
        [Key]
        public int ServiceListID { get; set; }
        public string ServiceName { get; set; }
        public string? ServiceImage { get; set; } // Stores the file path
        public IFormFile? UploadedImage { get; set; } // Temporarily holds the uploaded file
    }
}
