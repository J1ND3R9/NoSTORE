using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using NoSTORE.Models;
using NoSTORE.Models.DTO;
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
            Filter? filter = await _filterService.GetFiltersByCategory(category);

            var propertiesFilters = filter.Properties;
            var propertiesProduct = filter.PropertiesInDictionary(product.Properties);

            bool containsAll = propertiesProduct.All(group =>
            propertiesFilters.TryGetValue(group.Key, out var existingGroup) &&
            group.Value.All(prop =>
            existingGroup.TryGetValue(prop.Key, out var existingValues) &&
            prop.Value.All(v => existingValues.Contains(v))));

            if (containsAll)
                return;
            var filterUpdate = Builders<Filter>.Filter.Eq("category", category);
            foreach (var group in propertiesProduct)
            {
                string groupKey = group.Key;
                foreach (var property in group.Value)
                {
                    string propertyKey = property.Key;
                    List<string> valuesToAdd = property.Value.Select(s => s.Replace('ё', 'е')).ToList();
                    var update = Builders<Filter>.Update.AddToSetEach($"properties.{groupKey}.{propertyKey}", valuesToAdd);

                    await _filterService.UpdateDocument(filterUpdate, update);
                }
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
            List<ProductDto> dto = filteredProducts.Select(p => new ProductDto(p)).ToList();
            ProductCategory productCategory = new ProductCategory
            {
                CategoryName = category,
                Products = filteredProducts,
                ProductsDto = dto
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

        [HttpPost]
        public async Task<IActionResult> GetFilteredProducts([FromBody] FilterRequest FR)
        {
            var products = await _productService.FilterProducts(FR.Category, FR.Dictionary, FR.MinPrice, FR.MaxPrice);
            var dto = products.Select(p => new ProductDto(p)).ToList();
            if (FR.Sort != null)
                dto = Sorting(dto, FR.Sort);
            return PartialView("_ProductsPartial", dto);
        }

        public List<ProductDto> Sorting(List<ProductDto> products, string type)
        {
            var sorted = new List<ProductDto>();
            switch (type)
            {
                case "cheap":
                    products.Sort((p1, p2) => p1.Price.CompareTo(p2.Price));
                    break;
                case "expensive":
                    products.Sort((p1, p2) => p2.Price.CompareTo(p1.Price));
                    break;
                case "new":
                    products.Reverse();
                    break;
                case "rating":
                    products.Sort((p1, p2) => p2.Rating.CompareTo(p1.Rating));
                    break;
            }
            sorted = products;
            return sorted;
        }
    }

    public class FilterRequest
    {
        public string Category { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> Dictionary { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string? Sort { get; set; }
    }
}
