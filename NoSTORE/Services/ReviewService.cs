using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;

namespace NoSTORE.Services
{
    public class ReviewService
    {
        private readonly IMongoCollection<Review> _review;

        public ReviewService(MongoDbContext dbContext)
        {
            _review = dbContext.GetCollection<Review>("reviews");
        }

        public async Task<List<Review>> GetAllAsync() => await _review.Find(_ => true).ToListAsync();
        public async Task<Review> GetReviewById(string id) => await _review.Find(r => r.Id == id).FirstOrDefaultAsync();
        public async Task<List<Review>> GetReviewsByProduct(string productid) => await _review.Find(r => r.ProductId == productid).ToListAsync();
        public async Task<List<Review>> GetUserReviews(string userid) => await _review.Find(r => r.UserId == userid).ToListAsync();
    }
}
