using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using NoSTORE.Models;
using NoSTORE.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoSTORE.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ILogger<CatalogController> _logger;
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;
        private readonly FilterService _filterService;

        private List<Category> _categories = new();

        public CatalogController(ILogger<CatalogController> logger, CategoryService categoryService, ProductService productService, FilterService filterService)
        {
            _logger = logger;
            _categoryService = categoryService;
            _productService = productService;
            _filterService = filterService;
        }

        public async Task<Filter> UpdateFilters(Product product, string category)
        {
            Filter? filters = await _filterService.GetFiltersByCategory(category);
            var propertiesFilters = filters.Properties;
            var propertiesProduct = filters.PropertiesInDictionary(product.Properties);

            if (propertiesFilters == propertiesProduct)
                return new();



        }
        public async Task InsertFiltersAsync(List<Product> products, string category)
        {
            Filter filter = new();
            filter.Category = category;
            foreach (var product in products)
            {
                if (await FiltersExist(category))
                    filter = await UpdateFilters(product, category);
                else
                    filter.Properties = filter.PropertiesInDictionary(product.Properties);

                if (!String.IsNullOrEmpty(filter.Category))
                    await _filterService.InsertDocument(filter);
            }
        }

        public async Task CreateFilters(List<Product> products, string category)
        {
            if (products.Count == 0)
                return;
            await InsertFiltersAsync(products, category);
        }

        public async Task<bool> FiltersExist(string category)
        {
            Filter? filters = await _filterService.GetFiltersByCategory(category);
            return filters != null;
        }

        public async Task<IActionResult> IndexAsync()
        {
            _categories = await _categoryService.GetAllAsync();
            return View(_categories);
        }

        public async Task<IActionResult> DetailsAsync(string slug)
        {
            var categories = await _categoryService.GetAllAsync();
            var category = FindCategoryBySlug(categories, slug);

            if (category == null)
            {
                return NotFound();
            }

            if (category.HasSubcategories)
            {
                return View("Index", category.Subcategories);
            }
            var products = await GetProductsAsync(category.Name);
            await CreateFilters(products.Products, category.Name);
            products.Filter = await _filterService.GetFiltersByCategory(category.Name);
            return View("Products", products);
        }

        private async Task<ProductCategory> GetProductsAsync(string category)
        {
            List<Product> products = await _productService.GetAllAsync();
            List<Product> filteredProducts = products.Where(p => p.Category == category).ToList();
            ProductCategory productCategory = new ProductCategory
            {
                CategoryName = category,
                Products = filteredProducts
            };
            return productCategory;
        }

        private Category FindCategoryBySlug(List<Category> categories, string slug)
        {
            foreach (var category in categories)
            {
                if (category.Slug == slug)
                {
                    return category;
                }

                if (category.HasSubcategories)
                {
                    var found = FindCategoryBySlug(category.Subcategories, slug);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }
    }
}
