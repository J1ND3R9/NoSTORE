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
using NoSTORE.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NoSTORE.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace NoSTORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiProductController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ProductService _productService;
        private readonly ReviewService _reviewService;
        private readonly IHubContext<UserHub> _userHub;
        private readonly IConfiguration _configuration;
        public ApiProductController(UserService userService, ProductService productService, IHubContext<UserHub> hubContext, IConfiguration configuration, ReviewService reviewService)
        {
            _userService = userService;
            _productService = productService;
            _userHub = hubContext;
            _configuration = configuration;
            _reviewService = reviewService;
        }

        private string GetUserIdFromJwtOrCookie(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                var jwtSettings = _configuration.GetSection("JwtOptions").Get<AuthSettings>();

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

                try
                {
                    var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    return userId;
                }
                catch
                {
                    // JWT невалиден — fallback на cookie
                }
            }

            if (User.Identity.IsAuthenticated)
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (Request.Cookies.TryGetValue("GuestId", out var guestId))
                {
                    return guestId;
                }
            }

            return null;
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
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

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
        public async Task<IActionResult> FetchStatusesProducts(string productId)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

            var user = await _userService.GetUserById(userId);

            if (user == null)
            {
                return Ok(new
                {
                    inFavorite = false,
                    inCart = false,
                    inCompare = false
                });
            }

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

        [HttpGet("get/{productId}")]
        public async Task<IActionResult> GetProductById(string productId)
        {
            var product = new ProductDto(await _productService.GetByIdAsync(productId));
            var reviews = await _reviewService.GetReviewsByProduct(product.Id);
            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
            var users = await _userService.GetUsersByIdsAsync(userIds);
            var userDictionary = users.ToDictionary(u => u.Id, u => u);

            product.Reviews = reviews.Select(r => new ReviewDto(r)).ToList();
            foreach (var review in product.Reviews)
            {
                if (userDictionary.TryGetValue(review.UserId, out var user))
                {
                    review.User = new UserReview
                    {
                        Id = user.Id,
                        AvatarExt = user.AvatarExtension,
                        Nickname = user.Nickname
                    };
                }
            }
            return Ok(product.ToJson());
        }

        [HttpPost("select_product_cart")]
        public async Task<IActionResult> SelectProductAsync([FromBody] ProductRequest request)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

            var updateDto = await _userService.SelectBasketChange(userId, request.ProductId, true);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Updated", Cart = updateDto });

            return Ok();
        }

        [HttpPost("unselect_product_cart")]
        public async Task<IActionResult> UnSelectProductAsync([FromBody] ProductRequest request)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

            var updateDto = await _userService.SelectBasketChange(userId, request.ProductId, false);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Updated", Cart = updateDto });

            return Ok();
        }

        [HttpPost("quantity_product_cart")]
        public async Task<IActionResult> ChangeQuantity([FromBody] ProductRequest request)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

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
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

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
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

            var cartDto = await _userService.RemoveFromBasket(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("CartChanged", new CartUpdateDto { ActionType = "Removed", Cart = cartDto });
            return Ok();
        }

        [HttpPost("add_product_favorite")]
        public async Task<IActionResult> ToUserFavoriteAsync([FromBody] ProductRequest request)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

            var favDto = await _userService.InsertInFavorite(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("FavoriteChanged", favDto);
            return Ok();
        }

        [HttpGet("getCart")]
        public async Task<IActionResult> GetCart()
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();
            var user = await _userService.GetUserById(userId);

            var basket = user?.Basket;
            if (basket == null || !basket.Any())
            {
                return Ok(new CartApiDto());
            }

            var productIds = basket.Select(b => b.ProductId).ToList();
            var products = await _productService.GetByIdsAsync(productIds);

            var cartItems = basket.Select(b =>
            {
                var product = products.FirstOrDefault(p => p.Id == b.ProductId);
                return new CartItemApiDto
                {
                    Product = product != null ? new ProductDto(product) : null,
                    Quantity = b.Quantity,
                    IsSelected = b.IsSelected ?? true
                };
            })
            .Where(item => item.Product != null)
            .ToList();

            int totalCost = cartItems.Where(i => i.IsSelected).Sum(i => i.TotalPrice);
            int selectedCount = cartItems.Count(i => i.IsSelected);

            var result = new CartApiDto
            {
                Items = cartItems,
                TotalCost = totalCost,
                SelectedCount = selectedCount
            };
            result.Items.Reverse();
            return Ok(result.ToJson());
        }

        [HttpGet("getFavorite")]
        public async Task<IActionResult> GetFavorite()
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();
            var user = await _userService.GetUserById(userId);

            var favorite = user?.Favorites;
            if (favorite == null || !favorite.Any())
            {
                return Ok(new List<ProductDto>());
            }

            var products = await _productService.GetByIdsAsync(favorite);
            var productsDto = products.Select(s => new ProductDto(s)).ToList();
            productsDto.Reverse();

            return Ok(productsDto.ToJson());
        }

        [HttpPost("remove_product_favorite")]
        public async Task<IActionResult> FromUserFavorite([FromBody] ProductRequest request)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

            var favDto = await _userService.RemoveFromFavorite(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("FavoriteChanged", favDto);
            return Ok();
        }

        [HttpPost("add_compare")]
        public async Task<IActionResult> AddToCompare([FromBody] ProductRequest request)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);

            if (userId == null)
                return Unauthorized();

            await _userService.InsertCompare(userId, request.ProductId);
            await _userHub.Clients.User(userId).SendAsync("ComparesChanged", new CompareUpdateDto { ActionType = "Added" });
            return Ok();
        }

        [HttpPost("remove_compare")]
        public async Task<IActionResult> RemoveFromCompare([FromBody] ProductRequest request)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);


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

    public class CartApiDto
    {
        public List<CartItemApiDto> Items { get; set; } = new();
        public int TotalCost { get; set; }
        public int SelectedCount { get; set; }
    }

    public class CartItemApiDto
    {
        public ProductDto Product { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; }
        public int TotalPrice => Product?.Price * Quantity ?? 0;
    }
}
