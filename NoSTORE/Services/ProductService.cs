using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;

namespace NoSTORE.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;
        public ProductService(MongoDbContext dbContext)
        {
            _products = dbContext.GetCollection<Product>("products");
        }

        public async Task<List<Product>> GetAllAsync() => await _products.Find(_ => true).ToListAsync();
        public async Task<List<Product>> GetOnlyDiscount() => await _products.Find(p => p.Discount > 0).ToListAsync();
        public async Task<Product> GetByIdAsync(string id) => await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }
}
