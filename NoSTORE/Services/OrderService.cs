using MongoDB.Bson;
using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;
using NoSTORE.Models.DTO;

namespace NoSTORE.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _order;
        private readonly UserService _userService;

        public OrderService(MongoDbContext dbContext, UserService userService)
        {
            _order = dbContext.GetCollection<Order>("orders");
            _userService = userService;
        }

        public async Task<List<Order>> GetAllAsync() => await _order.Find(_ => true).ToListAsync();
        public async Task<List<Order>> GetOrdersByUserId(string userid) => await _order.Find(o => o.UserID == userid).ToListAsync();
        public async Task<string> PlaceOrder(string userid, CheckoutDto checkout)
        {
            var order = new Order
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserID = userid,
                CreateDate = DateTime.UtcNow,
                DevlieryDate = DateTime.UtcNow.AddDays(1),
                Products = checkout.Items.Select(c => new OrderProduct
                {
                    ProductID = c.Product.Id,
                    Quantity = c.Quantity
                }).ToList(),
                Status = 0,
                TotalPrice = checkout.Items.Sum(c => c.TotalPrice)
            };
            await _order.InsertOneAsync(order);
            await _userService.InsertOrder(userid, order.Id);
            return order.Id;
        }
    }
}
