using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
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

        public async Task UpdateFilters(Product product, string category)
        {
            Filter? filters = await _filterService.GetFiltersByCategory(category);
            var propertiesFilters = filters.Properties;
            var propertiesProduct = filters.PropertiesInDictionary(product.Properties);

            bool containsAll = propertiesProduct.All(pair =>
            propertiesFilters.ContainsKey(pair.Key) && pair.Value.All(value => propertiesFilters[pair.Key].Contains(value)));

            if (containsAll)
                return;

            var filterUpdate1 = Builders<Filter>.Filter.Eq("category", category);
            var filterUpdate2 = Builders<Filter>.Update.Push("", "");

            foreach (var kvp in propertiesProduct)
            {
                if (propertiesFilters.ContainsKey(kvp.Key))
                {
                    propertiesFilters[kvp.Key] = propertiesFilters[kvp.Key].Union(kvp.Value).ToList();
                }
                else
                {
                    propertiesFilters[kvp.Key] = kvp.Value;
                }
                filterUpdate2 = Builders<Filter>.Update.AddToSetEach($"properties.{kvp.Key}", kvp.Value);
                await _filterService.UpdateDocument(filterUpdate1, filterUpdate2);
            }
            
        }
        public async Task InsertFiltersAsync(List<Product> products, string category)
        {
            Filter filter = new();
            filter.Category = category;
            foreach (var product in products)
            {
                if (await FiltersExist(category))
                {
                    await UpdateFilters(product, category);
                    continue;
                }
                filter.Properties = filter.PropertiesInDictionary(product.Properties);
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
