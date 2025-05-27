using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Models.DTO;
using NoSTORE.Services;
using System.Security.Claims;

namespace NoSTORE.Controllers
{
    public class CompareController : Controller
    {
        private readonly UserService _userService;
        private readonly ProductService _productService;
        public CompareController(UserService userService, ProductService productService)
        {
            _userService = userService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            User user = new();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                user = await _userService.GetUserById(userId);
            }
            else
            {
                // Гость
            }
            var compares = user.Compares;
            CompareDto dto = new();
            foreach (var compare in compares)
            {
                List<Product> products = new();
                foreach (var productId in compare.ProductIds)
                {
                    var product = await _productService.GetByIdAsync(productId);
                    products.Add(product);
                }
                dto.Compares[compare.Category] = products;
            }
            return View(dto);
        }
    }
}
