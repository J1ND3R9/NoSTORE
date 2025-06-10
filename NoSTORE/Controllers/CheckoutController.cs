using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization.Serializers;
using NoSTORE.Models;
using NoSTORE.Models.DTO;
using NoSTORE.Services;
using NoSTORE.Settings;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZXing.QrCode;
using ZXing.Rendering;

namespace NoSTORE.Controllers
{
    public class ReceiptDocument : IDocument
    {
        public CheckoutDto Checkout { get; set; }
        public string Id { get; set; }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A6);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Content().Column(column =>
                {
                    column.Spacing(5);
                    column.Item().Image(Placeholders.Image(100, 50)).FitWidth();
                    column.Item().Text($"ЧЕК #{Id}").Bold().FontSize(16).AlignCenter();
                    column.Item().Text($"От {DateTime.UtcNow}");

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    foreach (var item in Checkout.Items)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"{item.Product.Name} x{item.Quantity}");
                            row.ConstantItem(60).AlignRight().Text($"{item.TotalPrice:C}");
                        });
                    }

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Итого:").Bold();
                        row.ConstantItem(60).AlignRight().Text($"{Checkout.Items.Sum(x => x.TotalPrice):C}").Bold();
                    });

                    column.Item().PaddingTop(20);
                    column.Item().AlignCenter().AspectRatio(1).Background(Colors.White).Svg(size =>
                    {
                        var writer = new QRCodeWriter();
                        var qrCode = writer.encode("https://example.com/cheque/123908123", ZXing.BarcodeFormat.QR_CODE, (int)size.Width, (int)size.Height);
                        var renderer = new SvgRenderer { FontName = "Lato" };
                        return renderer.Render(qrCode, ZXing.BarcodeFormat.EAN_13, null).Content;
                    });
                });
            });
        }
    }
    public class CheckoutController : Controller
    {
        private readonly UserService _userService;
        private readonly OrderService _orderService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        public CheckoutController(UserService userService, OrderService orderService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _userService = userService;
            _orderService = orderService;
            _env = env;
            _configuration = configuration;
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
            var checkout = await _userService.GenerateCheckout(userId);
            if (checkout == null || !checkout.Items.Any())
                return RedirectToAction("Index", "Home");
            return View(checkout);
        }

        [HttpPost("/api/order/place")]
        public async Task<IActionResult> PlaceOrder()
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);
            bool isGuest = false;

            if (Request.Cookies.TryGetValue("GuestId", out var guestId))
            {
                isGuest = true;
            }

            var checkoutDto = await _userService.GenerateCheckout(userId);

            string orderid = await _orderService.PlaceOrder(userId, checkoutDto);
            var receipt = new ReceiptDocument
            {
                Id = orderid,
                Checkout = checkoutDto
            };
            var folderPath = Path.Combine("wwwroot", "receipts", userId);
            if (isGuest)
                folderPath = Path.Combine("wwwroot", "receipts", "guests");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, $"{orderid}.pdf");
            receipt.GeneratePdf(filePath);

            return Ok(new { orderid });
        }

        [HttpGet("/receipts/download/{orderId}")]
        public IActionResult Download(string orderId)
        {
            string userId = GetUserIdFromJwtOrCookie(HttpContext);
            bool isGuest = false;

            if (Request.Cookies.TryGetValue("GuestId", out var guestId))
            {
                isGuest = true;
            }

            var filePath = Path.Combine(_env.WebRootPath, "receipts", userId, $"{orderId}.pdf");
            if (isGuest)
                filePath = Path.Combine(_env.WebRootPath, "receipts", "guests", $"{orderId}.pdf");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            return PhysicalFile(filePath, "application/pdf", $"{orderId}.pdf");
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
    }
}
