using MongoDB.Bson;
using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;

namespace NoSTORE.Services
{
    public class FilterService
    {
        private readonly IMongoCollection<Filter> _filters;

        public FilterService(MongoDbContext dbContext)
        {
            _filters = dbContext.GetCollection<Filter>("filters");
        }

        public async Task<List<Filter>> GetAllAsync() => await _filters.Find(_ => true).ToListAsync();
        public async Task<Filter> GetFiltersByCategory(string category)
        {
            var filters = await GetAllAsync();
            return filters.FirstOrDefault(f => f.Category == category);
        }

        public async Task InsertDocument(Filter filter) => await _filters.InsertOneAsync(filter);

        public async Task UpdateDocument(FilterDefinition<Filter> filter, UpdateDefinition<Filter> update) => await _filters.UpdateOneAsync(filter, update);
    }
}
