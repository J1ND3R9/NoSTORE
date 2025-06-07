using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NoSTORE.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;
        private readonly ProductService _productService;
        private readonly UserService _userService;

        public ReviewController(ReviewService reviewService, ProductService productService, UserService userService)
        {
            _reviewService = reviewService;
            _productService = productService;
            _userService = userService;
        }

        public async Task<IActionResult> Create(string productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                return RedirectToAction("Index", "Home");
            var model = new ReviewViewModel
            {
                ProductId = productId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return RedirectToAction("Details", "Product", new { id = model.ProductId });
        }
    }

    public class ReviewViewModel
    {
        public string ProductId { get; set; }

        [Required(ErrorMessage = "Введите текст отзыва")]
        [StringLength(1000, ErrorMessage = "Отзыв не может быть длиннее 1000 символов")]
        public string Text { get; set; }
    }
}
