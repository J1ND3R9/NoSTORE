using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models.DTO;
using NoSTORE.Models;
using NoSTORE.Services;
using MongoDB.Bson;

namespace NoSTORE.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogApiController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;
        public CatalogApiController(CategoryService categoryService, ProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetRootCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            var response = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Image = c.Image,
                HasSubcategories = c.HasSubcategories,
                Subcategories = c.Subcategories.Select(sc => new CategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Slug = sc.Slug,
                    Image = sc.Image,
                    HasSubcategories = false,
                    Subcategories = null
                }).ToList()
            });

            return Ok(response.ToJson());
        }

        // Получаем категорию по slug
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var categories = await _categoryService.GetAllAsync();
            var category = FindCategory(categories, slug);

            if (category == null) return NotFound();

            if (category.HasSubcategories)
            {
                return Ok(new CategoryResponse
                {
                    Name = category.Name,
                    Subcategories = category.Subcategories.Select(sc => new CategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Slug = sc.Slug,
                        Image = sc.Image,
                        HasSubcategories = sc.HasSubcategories,
                        Subcategories = null
                    }).ToList()
                }.ToJson());
            }
            else
            {
                var productCategory = await GetProductsAsync(category.Name);
                return Ok(new ProductsResponse
                {
                    CategoryName = category.Name,
                    Products = productCategory.ProductsDto
                }.ToJson());
            }
        }

        private Category FindCategory(List<Category> categories, string slug)
        {
            foreach (var category in categories)
            {
                if (category.Slug == slug)
                    return category;

                if (category.HasSubcategories)
                {
                    var found = category.Subcategories
                        .FirstOrDefault(sc => sc.Slug == slug);

                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        private async Task<ProductCategory> GetProductsAsync(string category)
        {
            var products = await _productService.GetAllAsync();
            var filtered = products.Where(p => p.Category == category).ToList();

            return new ProductCategory
            {
                CategoryName = category,
                Products = filtered,
                ProductsDto = filtered.Select(p => new ProductDto(p)).ToList()
            };
        }
    }
    public class CategoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }
        public bool HasSubcategories { get; set; }
        public List<CategoryDto>? Subcategories { get; set; }
    }
    public class CategoryResponse
    {
        public bool IsSubcategory => Subcategories != null && Subcategories.Count > 0;
        public string Name { get; set; }
        public List<CategoryDto> Subcategories { get; set; }
    }
    public class ProductsResponse
    {
        public bool IsSubcategory => true;
        public string CategoryName { get; set; }
        public List<ProductDto> Products { get; set; }
    }
}
