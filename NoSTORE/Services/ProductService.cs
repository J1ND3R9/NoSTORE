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
        public async Task<List<Product>> GetByIdsAsync(List<string> ids)
        {
            var products = await _products.Find(p => ids.Contains(p.Id)).ToListAsync();
            var productDict = products.ToDictionary(p => p.Id);
            return ids.Where(id => productDict.ContainsKey(id)).Select(id => productDict[id]).ToList();
        }

        public async Task<List<Product>> FilterProducts(Dictionary<string, Dictionary<string, List<string>>> filters)
        {
            var builder = Builders<Product>.Filter;
            var mongoFilter = builder.Empty;
            foreach (var dict in filters)
            {
                foreach (var list in dict.Value)
                {
                    var title = dict.Key;
                    var name = list.Key;
                    var values = list.Value;
                    var path = $"properties.{title}.{name}";
                    var filter = builder.In(path, values);
                    mongoFilter &= filter;
                }
            }
            return await _products.Find(mongoFilter).ToListAsync();
        }
    }
}
