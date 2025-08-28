namespace AFFZ_Customer.Models;

public partial class RequestForDisCountToUser
{
    public int SID { get; set; }

    public int MID { get; set; }

    public int FINALPRICE { get; set; }

    public int UID { get; set; }

    public int RFDFU { get; set; }

    public DateTime? ResponseDateTime { get; set; }

    public int? IsMerchantSelected { get; set; }

    public int? IsPaymentDone { get; set; }
}
