namespace AFFZ_Customer.Models
{
    public class RequestForDiscountViewModel
    {
        public int RFDTM { get; set; }
        public int SID { get; set; }
        public int UID { get; set; }
        public int MID { get; set; }
        public string ServiceName { get; set; }
        public int? ServicePrice { get; set; }
        public DateTime RequestDatetime { get; set; }
    }
}
