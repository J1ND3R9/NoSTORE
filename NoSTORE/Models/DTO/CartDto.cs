namespace NoSTORE.Models.DTO
{
    public class CartDto
    {
        public ProductDto Product { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; }
        public int TotalPrice => Product.FinalPrice * Quantity;
    }
}
