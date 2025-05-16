using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NoSTORE.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly UserService _userService;
        private readonly ProductService _productService;

        public FavoriteController(UserService userSerive, ProductService productService)
        {
            _userService = userSerive;
            _productService = productService;
        }

        [Route("~/favorite")]
        public async Task<IActionResult> Index()
        {
            var user = new User();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                user = await _userService.GetUserById(userId);
            }
            else
            {
                // Гость
            }
            var favorites = user.Favorites;
            var favoritesProducts = await _productService.GetByIdsAsync(favorites.Select(s => s.ProductId).ToList());

            var favoriteItems = favorites.Select(f => new FavoriteItemViewModel
            {
                Product = favoritesProducts.FirstOrDefault(p => p.Id == f.ProductId),
                Date = f.Date
            })
                .Where(i => i.Product != null)
                .ToList();

            favoriteItems = favoriteItems.OrderByDescending(f => f.Date).ToList();

            return View(favoriteItems);
        }
    }
}
