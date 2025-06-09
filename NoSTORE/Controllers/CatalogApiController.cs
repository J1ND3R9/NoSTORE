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
        private readonly FilterService _filterService;
        public CatalogApiController(CategoryService categoryService, ProductService productService, FilterService filterService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _filterService = filterService;
        }
        [HttpGet]
        public async Task<IActionResult> GetRootCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            var response = categories.Select(c => MapToDto(c, depth: 1)).ToList();
            return Ok(response.ToJson());
        }

        // Получаем категорию по slug
        [HttpGet("{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            List<Category> categories = await _categoryService.GetAllAsync();
            Category category = FindCategory(categories, slug);
            if (category == null) return NotFound();

            if (category.HasSubcategories)
            {
                return Ok(new CategoryResponse
                {
                    Name = category.Name,
                    Subcategories = category.Subcategories.Select(sc => MapToDto(sc, depth: 1)).ToList(),
                    IsSubcategory = category.Subcategories != null && category.Subcategories.Count > 0,
                    HasSubcategories = category.Subcategories != null && category.Subcategories.Count > 0,
                }.ToJson());
            }
            else
            {
                var productCategory = await GetProductsAsync(category.Name);
                return Ok(new ProductsResponse
                {
                    CategoryName = category.Name,
                    Products = productCategory.ProductsDto,
                    Filter = productCategory.Filter
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
                ProductsDto = filtered.Select(p => new ProductDto(p)).ToList(),
                Filter = await _filterService.GetFiltersByCategory(category)
            };
        }

        private CategoryDto MapToDto(Category category, int depth = 1)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Image = category.Image,
                HasSubcategories = category.HasSubcategories,
                Subcategories = (depth > 0 && category.HasSubcategories)
                    ? category.Subcategories.Select(sc => MapToDto(sc, depth - 1)).ToList()
                    : null
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
        public bool IsSubcategory { get; set; }
        public string Name { get; set; }
        public List<CategoryDto> Subcategories { get; set; }
        public bool HasSubcategories { get; set; }
    }
    public class ProductsResponse
    {
        public bool IsSubcategory => true;
        public string CategoryName { get; set; }
        public List<ProductDto> Products { get; set; }
        public Filter Filter { get; set; }
    }


}
