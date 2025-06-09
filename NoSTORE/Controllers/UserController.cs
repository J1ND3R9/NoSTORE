using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MongoDB.Driver;
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
        public async Task<IActionResult> Index(string section = "settings", string tab = null)
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

            model.Orders.Reverse();
            model.Reviews.Reverse();
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                string view = GetPartialViewName(section);
                if (view == "Partials/_Panel")
                {
                    var users = await _userService.GetAllAsync();
                    var productsP = await _productService.GetAllAsync();
                    var orders = await _orderService.GetAllAsync();

                    var page = new AdminPage()
                    {
                        Admin = model,
                        Users = users,
                        Products = productsP,
                        Orders = orders
                    };

                    string partialView = tab switch
                    {
                        "users" => "Partials/_AdminUsers",
                        "products" => "Partials/_AdminProducts",
                        "orders" => "Partials/_AdminOrders",
                        _ => "Partials/_AdminDashboard"
                    };

                    ViewBag.Tab = tab ?? "dashboard";
                    return PartialView("Partials/_Panel", page);
                }
                return PartialView(GetPartialViewName(section), model);
            }
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

    public class AdminPage()
    {
        public UserDto Admin { get; set; }
        public List<User> Users { get; set; }
        public List<Product> Products { get; set; }
        public List<Order> Orders { get; set; }
    }
}
