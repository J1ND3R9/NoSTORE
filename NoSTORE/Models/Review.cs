using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NoSTORE.Models.DTO;

namespace NoSTORE.Models
{
    public class UserReview
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string AvatarExt { get; set; }
    }
    public class AdditionDto
    {
        public string Date { get; set; }
        public string Text { get; set; }
        public AdditionDto(Addition a)
        {
            Date = a.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            Text = a.Text;
        }
    }
    public class ReviewDto
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string UserId { get; set; }
        public UserReview? User { get; set; }
        public string CreateDate { get; set; }
        public int Rating { get; set; }
        public string UsageTime { get; set; } = string.Empty;
        public string Pluses { get; set; }
        public string Minuses { get; set; }
        public string Comment { get; set; }
        public List<AdditionDto> Additions { get; set; }
        public int Likes { get; set; } = 0;
        public List<string> UsersLikes { get; set; }
        public ReviewDto(Review review)
        {
            Id = review.Id;
            ProductId = review.ProductId;
            UserId = review.UserId;
            CreateDate = review.CreateDate.ToString("yyyy-MM-ddTHH:mm:ssZ"); // ISO 8601
            Rating = review.Rating;
            UsageTime = review.UsageTime;
            Pluses = review.Pluses;
            Minuses = review.Minuses;
            Comment = review.Comment;
            Additions = review.Additions.Select(s => new AdditionDto(s)).ToList();
            Likes = review.Likes;
            UsersLikes = review.UsersLikes;
        }
    }
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("product_id")]
        public string ProductId { get; set; }

        [BsonElement("user_id")]
        public string UserId { get; set; }

        public User User { get; set; }

        [BsonElement("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;


        [BsonElement("rate")]
        public int Rating { get; set; }

        [BsonElement("usage_time")]
        public string UsageTime { get; set; } = string.Empty;

        [BsonElement("pluses")]
        public string Pluses { get; set; }

        [BsonElement("minuses")]
        public string Minuses { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; }

        [BsonElement("photos")]
        public List<string> Photos { get; set; }

        [BsonElement("additions")]
        public List<Addition> Additions { get; set; }

        [BsonElement("likes_count")]
        public int Likes { get; set; } = 0;

        [BsonElement("likes_by")]
        public List<string> UsersLikes { get; set; }

        [BsonElement("comments")]
        public List<Comments> CommentUsers { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class Addition
    {
        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("text")]
        public string Text { get; set; }
    }

    public class Comments
    {
        [BsonElement("user_id")]
        public string UserId { get; set; }

        [BsonElement("comment_date")]
        public DateTime Date { get; set; }

        [BsonElement("comment")]
        public string Text { get; set; }
    }
}
