namespace AFFZ_Customer.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CartItem> CartItem { get; set; } // Updated to List<CartItem>

    }
}
