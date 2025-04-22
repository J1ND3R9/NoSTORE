using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Globalization;

namespace NoSTORE.Models
{
    public class ProductCategory
    {
        public string CategoryName { get; set; }
        public List<Product> Products { get; set; }
        public Filter Filter { get; set; }

        public string PluralForm()
        {
            int count = Products.Count;
            int remainder10 = count % 10;
            int remainder100 = count % 100;
                
            if (remainder10 == 1)
                return $"{count} товар";
            if (remainder10 >= 2 && remainder10 <= 4)
                return $"{count} товара";

            return $"{count} товаров";
        }
    }
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("price")]
        public int Price { get; set; }

        public string CorrectPrice()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return Price.ToString("#,0", nfi);
        }

        [BsonElement("discount")]
        public int Discount { get; set; }

        [BsonElement("images")]
        public List<string> Images { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; }

        public string TagsInString()
        {
            string tags = "[";
            for (int i = 0; i < Tags.Count; i++)
            {
                if (i + 1 < Tags.Count)
                    tags += Tags[i] + ", ";
                else
                    tags += Tags[i];
            }
            tags += "]";
            return tags;
        }

        [BsonElement("city_delivery")]
        public List<string> CitiesDelivery { get; set; }

        [BsonElement("price_history")]
        public List<PriceHistory> PriceHistory { get; set; }

        [BsonElement("guarantee")]
        public int Guarantee { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("properties")]
        [BsonExtraElements]
        public BsonDocument Properties { get; set; }

        public string GetFolder()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", "products", Name);
            if (Directory.Exists(path))
                return path;
            return "";
        }

        public bool ImageExist()
        {
            string path = GetFolder();
            if (path != "")
            {
                path += "/" + Images[0];
                if (File.Exists(path))
                    return true;
            }
            else
                CreateFolder();
            return false;
        }

        public void CreateFolder()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", "products", Name);
            Directory.CreateDirectory(path);
        }
    }

    public class PriceHistory
    {
        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("price")]
        public int Price { get; set; }
    }
}
