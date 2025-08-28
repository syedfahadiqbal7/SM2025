using System.ComponentModel.DataAnnotations;

namespace AFFZ_Customer.Models;

public partial class PaymentHistory
{
    [Key]
    public int ID { get; set; }
    public string PAYMENTTYPE { get; set; }
    public string AMOUNT { get; set; }
    public int PAYERID { get; set; }
    public int MERCHANTID { get; set; }
    public int ISPAYMENTSUCCESS { get; set; }
    public int Quantity { get; set; }
    public int SERVICEID { get; set; }
    public int RFDFU { get; set; }

    public DateTime PAYMENTDATETIME { get; set; }
    public Service? Service { get; set; }
}
