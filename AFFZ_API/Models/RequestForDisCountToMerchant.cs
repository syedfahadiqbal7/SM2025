namespace AFFZ_API.Models;

public partial class RequestForDisCountToMerchant
{
    public int RFDTM { get; set; }

    public int SID { get; set; }

    public int MID { get; set; }

    public int UID { get; set; }

    public int? IsResponseSent { get; set; }

    public DateTime? RequestDateTime { get; set; }
}
