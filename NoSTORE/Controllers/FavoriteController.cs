using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Models.DTO;
using NoSTORE.Models.ViewModels;
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
            var user = await _userService.GetUserById(userId);
            var favorites = user.Favorites;
            var favoritesProducts = await _productService.GetByIdsAsync(favorites);

            favoritesProducts.Reverse();

            return View(favoritesProducts);
        }

        [HttpGet]
        public async Task<IActionResult> GetFavoritePartial()
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
            var favoritesProducts = await _productService.GetByIdsAsync(favorites);

            favoritesProducts.Reverse();

            return PartialView("_FavoritePartial", favoritesProducts);
        }
    }
}
