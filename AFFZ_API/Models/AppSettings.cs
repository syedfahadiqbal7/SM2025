namespace AFFZ_API.Models
{
    public class AppSettings
    {
        public string UserUrl { get; set; }
        public string ProviderUrl { get; set; }
        public string BaseIpAddress { get; set; }
        public string PublicDomain { get; set; }//ApiHttpsPort
        public string ApiHttpsPort { get; set; }//ApiHttpsPort
        public string MerchantHttpsPort { get; set; }//ApiHttpsPort
        public string CustomerHttpsPort { get; set; }//ApiHttpsPort
    }
}
