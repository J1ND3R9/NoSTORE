using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Services;

namespace NoSTORE.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ProductService _productService;

    public HomeController(ILogger<HomeController> logger, ProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        List<Product> products = await _productService.GetAllAsync();
        List<Product> discounts = await _productService.GetOnlyDiscount();
        HomeModel hm = new()
        {
            DiscountProducts = discounts,
            Products = products
        };
        return View(hm);
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
