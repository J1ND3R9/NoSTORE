using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using NoSTORE.Models;
using NoSTORE.Services;
using System.Security.Claims;

namespace NoSTORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApiProductController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ProductService _productService;
        public ApiProductController(UserService userService, ProductService productService)
        {
            _userService = userService;
            _productService = productService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> AllProducts()
        {
            var products = await _productService.GetAllAsync();
            var productsModel = products.Select(p => new ProductSearchModel(p));
            return Ok(productsModel.ToJson());
        }

        [HttpGet("search")]
        public async Task<IActionResult> QueryProducts(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<Product>());
            var products = await _productService.GetAllAsync();
            var results = products.Where(p => p.Name.ToLower().Contains(query.ToLower()) || p.Description.ToLower().Contains(query.ToLower())).Take(5).Select(p => new ProductSearchModel(p)).ToList();
            return Ok(results.ToJson());
        }
        [HttpGet("getQuantities")]
        public async Task<IActionResult> GetQuantity()
        {
            string userId = "";
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                // Гость
            }
            var user = await _userService.GetUserById(userId);

            int favQuantity = user.Favorites?.Count() ?? 0;
            int cartQuantity = user.Basket?.Count() ?? 0;

            return Ok(new
            {
                FavoriteQuantity = favQuantity,
                CartQuantity = cartQuantity
            });
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProducts(string productId)
        {
            if (User.Identity.IsAuthenticated)
            {

            }
            else
            {
                // Гость
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId);

            bool inFavorite = user.Favorites?.Select(f => f.ProductId).Contains(productId) ?? false;
            bool inCart = user.Basket?.Any(b => b.ProductId == productId) ?? false;

            return Ok(new
            {
                inFavorite,
                inCart
            });
        }

        [HttpPost("select_product_cart")]
        public async Task<IActionResult> SelectProductAsync([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            await _userService.SelectBasketChange(userId, request.ProductId, true);
            return Ok();
        }

        [HttpPost("unselect_product_cart")]
        public async Task<IActionResult> UnSelectProductAsync([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            await _userService.SelectBasketChange(userId, request.ProductId, false);
            return Ok();
        }

        [HttpPost("quantity_product_cart")]
        public async Task<IActionResult> ChangeQuantity([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            if (request.Quantity == null)
                request.Quantity = 1;

            await _userService.ChangeQuantityInBasket(userId, request.ProductId, (int)request.Quantity);

            return Ok();
        }

        [HttpPost("add_product_cart")]
        public async Task<IActionResult> ToUserCartAsync([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            if (request.ProductIds != null)
            {
                foreach (var productId in request.ProductIds)
                {
                    await _userService.InsertInBasket(userId, productId);
                }
            }
            else
            {
                await _userService.InsertInBasket(userId, request.ProductId);
            }
            return Ok();

        }

        [HttpPost("remove_product_cart")]
        public async Task<IActionResult> FromUserCart([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            await _userService.RemoveFromBasket(userId, request.ProductId);
            return Ok();
        }

        [HttpPost("add_product_favorite")]
        public async Task<IActionResult> ToUserFavoriteAsync([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            await _userService.InsertInFavorite(userId, request.ProductId);
            return Ok();

        }

        [HttpPost("remove_product_favorite")]
        public async Task<IActionResult> FromUserFavorite([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            await _userService.RemoveFromFavorite(userId, request.ProductId);
            return Ok();
        }
    }

    public class ProductRequest
    {
        public string ProductId { get; set; }
        public int? Quantity { get; set; }
        public string[]? ProductIds { get; set; }
    }
}
