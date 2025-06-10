using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NoSTORE.Models.DTO;
using NoSTORE.Services;
using Org.BouncyCastle.Asn1.Crmf;

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

        [HttpGet("search/{query}")]
        public async Task<IActionResult> Search(string query)
        {
            var results = await _productService.SearchProducts(query);
            var resultsDto = results.Select(s => new ProductDto(s));
            return Ok(resultsDto.ToJson());
        }
    }
}
