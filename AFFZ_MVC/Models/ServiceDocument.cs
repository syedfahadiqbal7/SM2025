namespace AFFZ_Customer.Models;

public partial class ServiceDocument
{
    public int ServiceDocumentId { get; set; }

    public int? ServiceId { get; set; }

    public string? DocumentDetail { get; set; }
}
