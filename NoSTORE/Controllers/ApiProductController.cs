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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace NoSTORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var productsModel = products.Select(p => new ProductDto(p));
            return Ok(productsModel.ToJson());
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
            if (user == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return BadRequest();
            }

            int favQuantity = user.Favorites?.Count() ?? 0;
            int cartQuantity = user.Basket?.Count() ?? 0;
            int compQuantity = user.Compares?.Sum(c => c.ProductIds.Count) ?? 0;

            return Ok(new
            {
                FavoriteQuantity = favQuantity,
                CartQuantity = cartQuantity,
                CompareQuantity = compQuantity
            });
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProducts(string productId)
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

            bool inFavorite = user.Favorites?.Contains(productId) ?? false;
            bool inCart = user.Basket?.Any(b => b.ProductId == productId) ?? false;
            bool inCompare = user.Compares?.Any(c => c.ProductIds.Contains(productId)) ?? false;

            return Ok(new
            {
                inFavorite,
                inCart,
                inCompare
            });
        }

        [HttpPost("select_product_cart")]
        public async Task<IActionResult> SelectProductAsync([FromBody] ProductRequest request)
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

            var updateDto = await _userService.SelectBasketChange(userId, request.ProductId, true);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Updated", Cart = updateDto });

            return Ok();
        }

        [HttpPost("unselect_product_cart")]
        public async Task<IActionResult> UnSelectProductAsync([FromBody] ProductRequest request)
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

            var updateDto = await _userService.SelectBasketChange(userId, request.ProductId, false);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Updated", Cart = updateDto });

            return Ok();
        }

        [HttpPost("quantity_product_cart")]
        public async Task<IActionResult> ChangeQuantity([FromBody] ProductRequest request)
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
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Added", Cart = cartDto });
            return Ok();
        }

        [HttpPost("remove_product_cart")]
        public async Task<IActionResult> FromUserCart([FromBody] ProductRequest request)
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

            var cartDto = await _userService.RemoveFromBasket(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Removed", Cart = cartDto });
            return Ok();
        }

        [HttpPost("add_product_favorite")]
        public async Task<IActionResult> ToUserFavoriteAsync([FromBody] ProductRequest request)
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

            var favDto = await _userService.InsertInFavorite(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("FavoriteChanged", favDto);
            return Ok();
        }

        [HttpPost("remove_product_favorite")]
        public async Task<IActionResult> FromUserFavorite([FromBody] ProductRequest request)
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

            var favDto = await _userService.RemoveFromFavorite(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("FavoriteChanged", favDto);
            return Ok();
        }

        [HttpPost("add_compare")]
        public async Task<IActionResult> AddToCompare([FromBody] ProductRequest request)
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

            await _userService.InsertCompare(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("ComparesChanged", new CompareUpdateDto { ActionType = "Added" });
            return Ok();
        }

        [HttpPost("remove_compare")]
        public async Task<IActionResult> RemoveFromCompare([FromBody] ProductRequest request)
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

            await _userService.RemoveCompare(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("ComparesChanged", new CompareUpdateDto { ActionType = "Removed" });
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
