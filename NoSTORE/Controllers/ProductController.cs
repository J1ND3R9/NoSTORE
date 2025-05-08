using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Services;
using System.Runtime.CompilerServices;

namespace NoSTORE.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private Product _product;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> DetailsAsync(string id, string seo)
        {
            var product = await _productService.GetByIdAsync(id);
            _product = product;
            if (product == null)
                return NotFound();
            return View(product);
        }
    }
}
