namespace NoSTORE.Models
{
    public class BasketItemViewModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; }
        public int TotalPrice => Product.FinalPrice * Quantity;
    }
}
