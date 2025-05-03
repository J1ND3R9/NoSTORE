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

        [BsonElement("avatar")]
        public string Avatar { get; set; }

        [BsonElement("password")]
        public string PasswordHash { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoleId { get; set; }

        [BsonElement("favorites")]
        public List<string> Favorties { get; set; }

        [BsonElement("basket")]
        public List<BasketItem> Basket { get; set; }

        [BsonElement("pcs")]
        public List<string> PCs { get; set; }

        [BsonElement("orders")]
        public List<string> Orders { get; set; }

        [BsonElement("reviews")]
        public List<string> Reviews { get; set; }


        public class BasketItem{

            [BsonElement("product_id")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string ProductId { get; set; }

            [BsonElement("quantity")]
            public int Quantity { get; set; }
            
        }


    }
}
