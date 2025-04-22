using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSTORE.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("slug")]
        public string Slug { get; set; }

        [BsonElement("subcategories")]
        public List<Category> Subcategories { get; set; } = new List<Category>();

        public string Image => Slug + ".png";

        public bool HasSubcategories => Subcategories.Any() && Subcategories != null;
    }
}
