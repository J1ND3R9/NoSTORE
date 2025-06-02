using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Models.DTO;
using NoSTORE.Services;
using System.Runtime.CompilerServices;

namespace NoSTORE.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ReviewService _reviewService;
        private readonly UserService _userService;

        private Product _product;

        public ProductController(ProductService productService, ReviewService reviewService, UserService userService)
        {
            _productService = productService;
            _reviewService = reviewService;
            _userService = userService;
        }

        public async Task<IActionResult> DetailsAsync(string id, string seo)
        {
            var product = await _productService.GetByIdAsync(id);
            _product = product;
            if (product == null)
                return NotFound();

            var reviews = await _reviewService.GetReviewsByProduct(product.Id);

            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
            var users = await _userService.GetUsersByIdsAsync(userIds);
            var userDictionary = users.ToDictionary(u => u.Id, u => u);

            foreach (var review in reviews)
            {
                if (userDictionary.TryGetValue(review.UserId, out var user))
                {
                    review.User = user;
                }
            }

            var dto = new ProductDetails
            {
                Product = product,
                Reviews = reviews
            };

            return View(dto);
        }
    }
}
