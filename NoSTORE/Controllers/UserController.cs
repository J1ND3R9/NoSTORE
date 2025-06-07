using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NoSTORE.Models;
using NoSTORE.Models.DTO;
using NoSTORE.Services;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;

namespace NoSTORE.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly OrderService _orderService;
        private readonly ProductService _productService;
        private readonly ReviewService _reviewService;

        public UserController(UserService userService, OrderService orderService, ReviewService reviewService, ProductService productService)
        {
            _userService = userService;
            _orderService = orderService;
            _reviewService = reviewService;
            _productService = productService;
        }

        [Route("~/profile/{section?}")]
        public async Task<IActionResult> Index(string section = "settings")
        {
            ViewBag.Section = section;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);

            var model = new UserDto(
                user.Id,
                user.Nickname,
                user.AvatarExtension,
                user.Email,
                user.Phone,
                user.RoleId,
                user.Favorites,
                user.Basket,
                user.PCs,
                await _orderService.GetOrdersByUserId(user.Id),
                await _reviewService.GetUserReviews(user.Id),
                user.RegistrationDate
            );

            var productIds = model.Reviews.Select(r => r.ProductId).Distinct().ToList();
            var products = await _productService.GetByIdsAsync(productIds);
            var productDictionary = products.ToDictionary(p => p.Id, p => p);

            foreach (var review in model.Reviews)
            {
                if (productDictionary.TryGetValue(review.ProductId, out var product))
                {
                    review.Product = new ProductDto(product);
                }
            }

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return PartialView(GetPartialViewName(section), model);
            }
            model.Orders.Reverse();
            model.Reviews.Reverse();
            return View(model);
        }

        private string GetPartialViewName(string section)
        {
            return section switch
            {
                "basket" => "Partials/_Basket",
                "favorites" => "Partials/_Favorites",
                "reviews" => "Partials/_Reviews",
                "orders" => "Partials/_Orders",
                "panel" => "Partials/_Panel",
                _ => "Partials/_Settings"
            };
        }

        [HttpGet("/api/admin/check")]
        public async Task<IActionResult> IsAdmin()
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (await _userService.UserIsAdmin(userId))
                return Ok(new
                {
                    isAdmin = true
                });
            return Ok(new
            {
                isAdmin = false
            });
        }
    }
}
