using Microsoft.AspNetCore.Mvc;
using NoSTORE.Services;

namespace NoSTORE.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> DetailsAsync(string id, string seo)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            return View(product);
        }
    }
}
