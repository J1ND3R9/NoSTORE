using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSTORE.Models
{
    public class OrderDto
    {
        public string Id { get; set; }
        public string CreateDate { get; set; }
        public int TotalPrice { get; set; }
        public string Status { get; set; }

        public OrderDto(Order order)
        {
            Id = order.Id;
            CreateDate = order.CreateDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            TotalPrice = order.TotalPrice;
            Dictionary<byte, string> dict = new()
    {
        {0, "В обработке"},
        {1, "Принят"},
        {2, "Сборка заказа"},
        {3, "В пути"},
        {4, "Ожидает получения"},
        {5, "Получен"},
        {10, "Отменён"}
    };
            Status = dict[order.Status];
        }
    }
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
