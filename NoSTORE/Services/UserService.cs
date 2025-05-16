using MongoDB.Bson;
using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;
using System.Numerics;
using System.Threading.Tasks;

namespace NoSTORE.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _user;
        private readonly ProductService _productService;

        public UserService(MongoDbContext dbContext, ProductService productService)
        {
            _user = dbContext.GetCollection<User>("users");
            _productService = productService;
        }
        public async Task<List<User>> GetAllAsync() => await _user.Find(_ => true).ToListAsync();
        public async Task<User> GetUserById(string id) => await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
        public async Task<User> GetUserByEmailAsync(string email) => await _user.Find(u => u.Email == email).FirstOrDefaultAsync();
        public async Task<User> GetUserByPhoneAsync(string phone) => await _user.Find(u => u.Phone == phone).FirstOrDefaultAsync();

        public async Task ChangeQuantityInBasket(string userId, string productId, int quantity)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return;
            var existingItem = user.Basket?.FirstOrDefault(b => b.ProductId == productId) ?? null;
            if (existingItem == null)
                return;

            if (existingItem.Quantity + quantity <= 0)
            {
                await RemoveFromBasket(userId, productId);
                return;
            }
            var product = await _productService.GetByIdAsync(productId);
            if (existingItem.Quantity + quantity > product.Quantity)
                return;


            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(x => x.Basket, b => b.ProductId == productId));
            var update = Builders<User>.Update.Inc("basket.$.quantity", quantity);
            await _user.UpdateOneAsync(filter, update);

        }

        public async Task InsertInBasket(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return;
            var existingItem = user.Basket?.FirstOrDefault(b => b.ProductId == productId) ?? null;
            if (existingItem != null)
                return;

            var update = Builders<User>.Update.Push("basket", new User.BasketItem
            {
                ProductId = productId,
                Quantity = 1,
                IsSelected = true
            });
            await _user.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task SelectBasketChange(string userId, string productId, bool select)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return;
            var existingItem = user.Basket?.FirstOrDefault(b => b.ProductId == productId) ?? null;
            if (existingItem == null)
                return;
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(x => x.Basket, b => b.ProductId == productId));
            var update = Builders<User>.Update.Set("basket.$.selected", select);

            await _user.UpdateOneAsync(filter, update);
        }

        public async Task RemoveFromBasket(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return;
            var existingItem = user.Basket.FirstOrDefault(b => b.ProductId == productId);

            if (existingItem == null)
                return;

            var update = Builders<User>.Update.PullFilter(x => x.Basket, b => b.ProductId == productId);
            await _user.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task InsertInFavorite(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return;
            var existingItem = user.Favorites.FirstOrDefault(fav => fav.ProductId == productId);
            if (existingItem != null)
                return;

            var update = Builders<User>.Update.Push("favorites", new User.FavoriteItem
            {
                ProductId = productId,
                Date = DateTime.UtcNow
            });
            await _user.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task RemoveFromFavorite(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return;
            var existingItem = user.Favorites.FirstOrDefault(fav => fav.ProductId == productId);

            if (existingItem == null)
                return;

            var update = Builders<User>.Update.PullFilter(x => x.Favorites, fav => fav.ProductId == productId);
            var x = await _user.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<bool> UserExistsByEmail(string email)
        {
            var exist = await _user.Find(u => u.Email == email).FirstOrDefaultAsync();
            return exist != null;
        }

        public async Task<bool> UserExistsByPhone(string phone)
        {
            var exist = await _user.Find(u => u.Phone == phone).FirstOrDefaultAsync();
            return exist != null;
        }

        public async Task InsertUser(User user)
        {
            if (string.IsNullOrEmpty(user.RoleId))
            {
                user.RoleId = "67e27e725cce8193928f9f28";
            }
            await _user.InsertOneAsync(user);
        }

    }
}
