using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using NoSTORE.Models.DTO;
using NoSTORE.Hubs;
using NoSTORE.Models;
using NoSTORE.Services;
using System.Security.Claims;
using MongoDB.Driver;

namespace NoSTORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApiProductController : ControllerBase
    {
            private readonly UserService _userService;
            private readonly ProductService _productService;
            private readonly IHubContext<UserHub> _userHub;
            public ApiProductController(UserService userService, ProductService productService, IHubContext<UserHub> hubContext)
            {
                _userService = userService;
                _productService = productService;
                _userHub = hubContext;
            }

        [HttpGet("all")]
        public async Task<IActionResult> AllProducts()
        {
            var products = await _productService.GetAllAsync();
            var productsModel = products.Select(p => new ProductSearchDto(p));
            return Ok(productsModel.ToJson());
        }

        [HttpGet("search")]
        public async Task<IActionResult> QueryProducts(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<Product>());
            var products = await _productService.GetAllAsync();
            var results = products.Where(p => p.Name.ToLower().Contains(query.ToLower()) || p.Description.ToLower().Contains(query.ToLower())).Take(5).Select(p => new ProductSearchDto(p)).ToList();
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

            bool inFavorite = user.Favorites?.Contains(productId) ?? false;
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

            var updateDto = await _userService.SelectBasketChange(userId, request.ProductId, true);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Updated", Cart = updateDto});

            return Ok();
        }

        [HttpPost("unselect_product_cart")]
        public async Task<IActionResult> UnSelectProductAsync([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var updateDto = await _userService.SelectBasketChange(userId, request.ProductId, false);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Updated", Cart = updateDto});

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

                var updatedProduct = await _userService.ChangeQuantityInBasket(userId, request.ProductId, (int)request.Quantity);

                var updateDto = new CartUpdateDto
                {
                    ActionType = updatedProduct.Quantity <= 0 ? "Removed" : "Updated",
                    Cart = updatedProduct
                };

                await _userHub.Clients.User(userId).SendAsync("CartChanged", updateDto);
                return Ok();
            }

        [HttpPost("add_product_cart")]
        public async Task<IActionResult> ToUserCartAsync([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var cartDto = new CartDto();
            if (request.ProductIds != null)
            {
                foreach (var productId in request.ProductIds)
                {
                    cartDto = await _userService.InsertInBasket(userId, productId);
                }
            }
            else
            {
                cartDto = await _userService.InsertInBasket(userId, request.ProductId);
            }
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Added", Cart = cartDto});
            return Ok();
        }

        [HttpPost("remove_product_cart")]
        public async Task<IActionResult> FromUserCart([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var cartDto = await _userService.RemoveFromBasket(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Removed", Cart = cartDto});
            return Ok();
        }

        [HttpPost("add_product_favorite")]
        public async Task<IActionResult> ToUserFavoriteAsync([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var favDto = await _userService.InsertInFavorite(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("FavoriteChanged", favDto);
            return Ok();
        }

        [HttpPost("remove_product_favorite")]
        public async Task<IActionResult> FromUserFavorite([FromBody] ProductRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var favDto = await _userService.RemoveFromFavorite(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("FavoriteChanged", favDto);
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
