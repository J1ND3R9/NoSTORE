using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSTORE.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("create_date")]
        public DateTime CreateDate { get; set; }

        [BsonElement("userid")]
        public string UserID { get; set; }

        [BsonElement("delivery_date")]
        public DateTime DevlieryDate { get; set; }

        [BsonElement("products")]
        public List<OrderProduct> Products { get; set; }

        [BsonElement("total_price")]
        public int TotalPrice { get; set; }

        [BsonElement("status")]
        public byte Status { get; set; }
    }

    public class OrderProduct
    {
        [BsonElement("product_id")]
        public string ProductID { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}
