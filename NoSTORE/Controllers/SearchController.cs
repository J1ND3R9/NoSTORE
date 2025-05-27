using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models.DTO;
using NoSTORE.Services;

namespace NoSTORE.Controllers
{
    public class SearchController : Controller
    {
        private readonly ProductService _productService;
        public SearchController(ProductService productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> Index(string q)
        {
            var results = await _productService.SearchProducts(q);
            var resultsDto = results.Select(s => new ProductDto(s));
            return View(resultsDto);
        }
    }
}
