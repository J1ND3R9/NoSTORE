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
            string userId = "";
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (Request.Cookies.TryGetValue("GuestId", out var guestId))
                {
                    userId = guestId;
                }
                else
                {
                    // Гость без куки — крайне редкая ситуация
                    userId = null;
                }
            }
            if (userId == null)
                return Unauthorized();
            var user = await _userService.GetUserById(userId);
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

        [HttpGet("/compare/{category}")]
        public async Task<IActionResult> GetCompareSection(string category)
        {
            string userId = "";
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (Request.Cookies.TryGetValue("GuestId", out var guestId))
                {
                    userId = guestId;
                }
                else
                {
                    // Гость без куки — крайне редкая ситуация
                    userId = null;
                }
            }
            if (userId == null)
                return Unauthorized();
            var user = await _userService.GetUserById(userId);
            var compares = user.Compares;
            CompareDto dto = new();
            foreach (var compare in compares)
            {
                if (compare.Category != category)
                    continue;
                List<Product> products = new();
                foreach (var productId in compare.ProductIds)
                {
                    var product = await _productService.GetByIdAsync(productId);
                    products.Add(product);
                }
                dto.Compares[compare.Category] = products;
            }
            return PartialView("_ComparePartial", dto.Compares.Values.FirstOrDefault());
        }
    }
}
