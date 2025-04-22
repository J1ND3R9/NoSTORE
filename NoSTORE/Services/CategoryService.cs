using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;

namespace NoSTORE.Services
{
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _category;

        public CategoryService(MongoDbContext dbContext)
        {
            _category = dbContext.GetCollection<Category>("categories");
        }

        public async Task<List<Category>> GetAllAsync() => await _category.Find(_ => true).ToListAsync();
        public List<Category> GetAll() => _category.Find(_ => true).ToList();
    }
}
