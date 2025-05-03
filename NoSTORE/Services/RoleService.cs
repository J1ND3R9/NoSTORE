using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;

namespace NoSTORE.Services
{
    public class RoleService
    {
        private readonly IMongoCollection<Role> _roles;

        public RoleService(MongoDbContext dbContext)
        {
            _roles = dbContext.GetCollection<Role>("roles");
        }

        public Task<List<Role>> GetAllAsync() => _roles.Find(_ => true).ToListAsync();
    }
}
