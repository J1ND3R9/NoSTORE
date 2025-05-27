using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSTORE.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("nickname")]
        public string Nickname { get; set; }

        [BsonElement("avatar_ext")]
        public string? AvatarExtension { get; set; }

        [BsonElement("password")]
        public string PasswordHash { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("registration_date")]
        public DateTime RegistrationDate { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoleId { get; set; }

        [BsonElement("favorites")]
        public List<string> Favorites { get; set; } = new();

        [BsonElement("basket")]
        public List<BasketItem> Basket { get; set; } = new();

        [BsonElement("compares")]
        public List<CompareItem> Compares { get; set; } = new();

        [BsonElement("pcs")]
        public List<string> PCs { get; set; } = new();

        [BsonElement("orders")]
        public List<string> Orders { get; set; } = new();

        [BsonElement("reviews")]
        public List<string> Reviews { get; set; } = new();


        public class BasketItem
        {

            [BsonElement("product_id")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string ProductId { get; set; }

            [BsonElement("quantity")]
            public int Quantity { get; set; }

            [BsonElement("selected")]
            public bool? IsSelected { get; set; } = true;

        }

        public class CompareItem
        {
            [BsonElement("category")]
            public string Category { get; set; }

            [BsonElement("product_ids")]
            public List<string> ProductIds { get; set; }
        }
    }
}
