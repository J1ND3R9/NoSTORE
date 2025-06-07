using MongoDB.Bson;
using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;
using NoSTORE.Models.DTO;
using NoSTORE.Models.ViewModels;
using System.Numerics;
using System.Threading.Tasks;
using static NoSTORE.Models.User;

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
        public async Task<User> GetUserById(string id) => await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
        public async Task<List<User>> GetUsersByIdsAsync(List<string> ids)
        {
            return await _user.Find(u => ids.Contains(u.Id)).ToListAsync();
        }
        public async Task<User> GetUserByEmailAsync(string email) => await _user.Find(u => u.Email == email).FirstOrDefaultAsync();
        public async Task<User> GetUserByPhoneAsync(string phone) => await _user.Find(u => u.Phone == phone).FirstOrDefaultAsync();
        public async Task<bool> UserIsAdmin(string id)
        {
            List<string> roles = new List<string>
            {
                "67e27e725cce8193928f9f29",
                "67e27e725cce8193928f9f2b"
            };
            var user = await GetUserById(id);
            return roles.Contains(user.RoleId);

        }
        public async Task DeleteUserAsync(string id) => await _user.DeleteOneAsync(u => u.Id == id);

        public async Task ChangeEmail(string id, string newEmail)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return;
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.Email, newEmail);
            await _user.UpdateOneAsync(filter, update);
        }

        public async Task ChangeNickname(string id, string newNickname)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return;
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.Nickname, newNickname);
            await _user.UpdateOneAsync(filter, update);
        }

        public async Task ChangePhone(string id, string newPhone)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return;
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.Phone, newPhone);
            await _user.UpdateOneAsync(filter, update);
        }

        public async Task ChangeAvatar(string id, string extension)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return;
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.AvatarExtension, extension);
            await _user.UpdateOneAsync(filter, update);
        }

        public async Task ChangePassword(string id, string passHash)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return;
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Set(u => u.PasswordHash, passHash);
            await _user.UpdateOneAsync(filter, update);
        }

        public async Task InsertOrder(string userId, string id)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return;

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Push("orders", id);
            var updatePull = Builders<User>.Update.PullFilter(x => x.Basket, b => b.IsSelected == true);
            await _user.UpdateOneAsync(filter, update);
            await _user.UpdateOneAsync(filter, updatePull);
        }

        public async Task<string> GetAvatarExtension(string id)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return "";
            return user.AvatarExtension ?? "";
        }

        public async Task<string> GetNickname(string id)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return "";
            return user.Nickname;
        }

        public async Task<string> GetRole(string id)
        {
            var user = await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
                return "";
            return user.RoleId;
        }

        public async Task<CheckoutDto?> GenerateCheckout(string userId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return null;
            CheckoutDto? checkout = new();
            var items = user.Basket.Where(b => b.IsSelected ?? false).ToList();
            checkout.User = user;
            checkout.Items = new();
            foreach (var item in items)
            {
                var product = new ProductDto(await _productService.GetByIdAsync(item.ProductId));
                var cart = new CartViewModel
                {
                    Product = product,
                    IsSelected = item.IsSelected ?? false,
                    Quantity = item.Quantity,
                };
                checkout.Items.Add(cart);
            }
            return checkout;
        }

        public async Task InsertCompare(string userId, string productId)
        {
            var item = await _productService.GetByIdAsync(productId);
            if (item == null)
                return;
            var user = await GetUserById(userId);
            if (user == null)
                return;
            var existingCategory = user.Compares?.FirstOrDefault(i => i.Category == item.Category) ?? null;
            if (existingCategory != null)
            {
                var itemIsExist = existingCategory.ProductIds.Contains(item.Id);
                if (itemIsExist)
                    return;
                var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(x => x.Compares, b => b.Category == item.Category));
                var update = Builders<User>.Update.Push("compares.$.product_ids", item.Id);
                await _user.UpdateOneAsync(filter, update);
            }
            else
            {
                var update = Builders<User>.Update.Push("compares", new CompareItem
                {
                    Category = item.Category,
                    ProductIds = new List<string> { item.Id }
                });
                await _user.UpdateOneAsync(u => u.Id == userId, update);
            }
        }

        public async Task RemoveCompare(string userId, string productId)
        {
            var item = await _productService.GetByIdAsync(productId);
            if (item == null) return;

            var user = await GetUserById(userId);
            if (user == null) return;

            var existingCategory = user.Compares?.FirstOrDefault(i => i.Category == item.Category) ?? null;
            if (existingCategory == null) return;

            var itemIsExist = existingCategory.ProductIds.Contains(item.Id);
            if (!itemIsExist) return;

            var filter = Builders<User>.Filter.And(
               Builders<User>.Filter.Eq(u => u.Id, userId),
               Builders<User>.Filter.ElemMatch(x => x.Compares, b => b.Category == item.Category));
            var update = Builders<User>.Update.Pull("compares.$.product_ids", item.Id);
            await _user.UpdateOneAsync(filter, update);

            // Повторно проходимся по User и проверяем на пустой массив
            user = await GetUserById(userId);
            if (user == null) return;
            var category = user.Compares?.FirstOrDefault(i => i.Category == item.Category) ?? null;
            if (category != null && category.ProductIds.Count == 0)
            {
                var removeFilter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var removeUpdate = Builders<User>.Update.PullFilter(u => u.Compares, Builders<CompareItem>.Filter.Eq(c => c.Category, item.Category));

                await _user.UpdateOneAsync(removeFilter, removeUpdate);
            }

        }

        public async Task<CartDto> ChangeQuantityInBasket(string userId, string productId, int quantity)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return null;
            var existingItem = user.Basket?.FirstOrDefault(b => b.ProductId == productId) ?? null;
            if (existingItem == null)
                return null;
            var product = await _productService.GetByIdAsync(productId);
            var newQuantity = existingItem.Quantity + quantity;
            if (newQuantity <= 0)
            {
                await RemoveFromBasket(userId, productId);
                return new CartDto
                {
                    IsSelected = true,
                    Product = new ProductDto(product),
                    Quantity = 0,
                };
            }
            if (newQuantity > product.Quantity)
                return null;


            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(x => x.Basket, b => b.ProductId == productId));
            var update = Builders<User>.Update.Inc("basket.$.quantity", quantity);
            await _user.UpdateOneAsync(filter, update);

            return new CartDto
            {
                IsSelected = true,
                Product = new ProductDto(product),
                Quantity = newQuantity,
            };

        }

        public async Task<CartDto> InsertInBasket(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return null;
            var existingItem = user.Basket?.FirstOrDefault(b => b.ProductId == productId) ?? null;
            if (existingItem != null)
                return new CartDto
                {
                    IsSelected = true,
                    Product = new ProductDto(await _productService.GetByIdAsync(productId)),
                    Quantity = 1
                };

            var update = Builders<User>.Update.Push("basket", new BasketItem
            {
                ProductId = productId,
                Quantity = 1,
                IsSelected = true
            });
            await _user.UpdateOneAsync(u => u.Id == userId, update);
            return new CartDto
            {
                IsSelected = true,
                Product = new ProductDto(await _productService.GetByIdAsync(productId)),
                Quantity = 1
            };
        }

        public async Task<CartDto> SelectBasketChange(string userId, string productId, bool select)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return null;
            var existingItem = user.Basket?.FirstOrDefault(b => b.ProductId == productId) ?? null;
            if (existingItem == null)
                return null;
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(x => x.Basket, b => b.ProductId == productId));
            var update = Builders<User>.Update.Set("basket.$.selected", select);

            await _user.UpdateOneAsync(filter, update);
            return new CartDto
            {
                IsSelected = select,
                Product = new ProductDto(await _productService.GetByIdAsync(productId)),
                Quantity = existingItem.Quantity
            };
        }

        public async Task<CartDto> RemoveFromBasket(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return null;
            var existingItem = user.Basket.FirstOrDefault(b => b.ProductId == productId);

            if (existingItem == null)
                return null;

            var update = Builders<User>.Update.PullFilter(x => x.Basket, b => b.ProductId == productId);
            await _user.UpdateOneAsync(u => u.Id == userId, update);
            return new CartDto
            {
                Product = new ProductDto(await _productService.GetByIdAsync(productId))
            };
        }

        public async Task<FavoriteDto> InsertInFavorite(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return null;
            var existingItem = user.Favorites.FirstOrDefault(fav => fav == productId);
            if (existingItem != null)
                return null;

            var update = Builders<User>.Update.Push("favorites", productId);
            await _user.UpdateOneAsync(u => u.Id == userId, update);
            return new FavoriteDto
            {
                Product = new ProductDto(await _productService.GetByIdAsync(productId)),
                ActionType = "Added"
            };
        }

        public async Task<FavoriteDto> RemoveFromFavorite(string userId, string productId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return null;
            var existingItem = user.Favorites.FirstOrDefault(fav => fav == productId);

            if (existingItem == null)
                return null;

            var update = Builders<User>.Update.PullFilter(x => x.Favorites, fav => fav == productId);
            await _user.UpdateOneAsync(u => u.Id == userId, update);
            return new FavoriteDto
            {
                Product = new ProductDto(await _productService.GetByIdAsync(productId)),
                ActionType = "Removed"
            };
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
            user.IsGuest = false;
            await _user.InsertOneAsync(user);
        }

        public async Task<User> CreateGuest()
        {
            User user = new();
            user.Id = ObjectId.GenerateNewId().ToString();
            user.RoleId = "67e27e725cce8193928f9f28";
            user.IsGuest = true;
            await _user.InsertOneAsync(user);
            return user;
        }
    }
}
