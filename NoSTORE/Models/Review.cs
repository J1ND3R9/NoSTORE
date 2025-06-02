using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSTORE.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("product_id")]
        public required string ProductId { get; set; }

        [BsonElement("user_id")]
        public required string UserId { get; set; }

        public User User { get; set; }

        [BsonElement("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [BsonElement("rate")]
        public required int Rating { get; set; }

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
