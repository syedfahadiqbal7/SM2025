namespace AFFZ_Customer.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int CartID { get; set; }
        public int ServiceID { get; set; }
        public int? MID { get; set; }
        public string? ServiceName { get; set; } // Added ServiceName property
        public decimal ServicePrice { get; set; } // Added Price property
        public int Quantity { get; set; }
        public int CustomerID { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
