namespace AFFZ_Customer.Models;

public partial class Service
{
    public int serviceId { get; set; }

    public int? categoryId { get; set; }

    public int? merchantId { get; set; }

    public int SID { get; set; }

    public string? description { get; set; }
    public string? serviceName { get; set; }

    public int? servicePrice { get; set; }

    public object Category { get; set; }

    public object Merchant { get; set; }
    public int RequiresEligibility { get; set; }
}


public partial class SubCatPage
{
    public int? ServicePrice { get; set; }
    public int? CatId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string Location { get; set; } = null!;
}