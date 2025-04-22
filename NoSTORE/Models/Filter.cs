using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSTORE.Models
{
    public class Filter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("properties")]
        [BsonExtraElements]
        public BsonDocument Properties { get; set; }
    }
}
