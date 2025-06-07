using NoSTORE.Models.ViewModels;
namespace NoSTORE.Models.DTO
{
    public class CheckoutDto
    {
        public User? User { get; set; }
        public List<CartViewModel> Items { get; set; } = new List<CartViewModel>();
    }
}
