using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSTORE.Models
{
    public class Role
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

    }
}
