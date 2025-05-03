using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSTORE.Models
{
    public class Verification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Code")]
        public string Code { get; set; }

        [BsonElement("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("Used")]
        public bool Used { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

    }
}
