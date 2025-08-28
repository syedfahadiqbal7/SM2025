using System.ComponentModel.DataAnnotations;

namespace AFFZ_Customer.Models
{
    public class ConfigurationSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "VAT percentage must be between 0 and 100.")]
        public double VATPercentage { get; set; } = 5.0; // Default to 5%

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Service charge must be a positive number.")]
        public decimal ServiceCharge { get; set; } = 50.0m; // Default to 50 AED

        [Required]
        public bool IsServiceChargePerQuantity { get; set; } = false; // Default to overall amount
    }

}
