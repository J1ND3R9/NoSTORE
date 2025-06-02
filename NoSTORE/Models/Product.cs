using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using NoSTORE.Models.DTO;
using System.Globalization;
using Unidecode.NET;

namespace NoSTORE.Models
{
    public class HomeModel
    {
        public List<Product> Products { get; set; }
        public List<Product> DiscountProducts { get; set; }
    }
    public class ProductCategory
    {
        public string CategoryName { get; set; }
        public List<Product> Products { get; set; }
        public List<ProductDto> ProductsDto { get; set; }
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

        public string SEOName =>
            Name.Unidecode().ToLower()
                .Replace(' ', '-')
                .Replace(".", "")
                .Replace(",", "")
                .Replace("\'", "")
                .Replace("\"", "");

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("price")]
        public int Price { get; set; }
        public int FinalPrice => Price - (int)(Price * ((double)Discount / 100));

        public string CorrectPrice(int price)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return price.ToString("#,0 ₽", nfi);
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
        
        [BsonElement("quantity")]
        public int Quantity { get; set; }
        public string QuantityString()
        {
            if (Quantity < 100)
                return Quantity.ToString() + " шт.";
            if (Quantity > 999)
                return ">1000 шт.";
            int first = Convert.ToInt32(Quantity.ToString().Substring(0, 1)) * 100;
            string x = "";
            x = ">" + first.ToString() + " шт.";
            return x;
        }

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("properties")]
        [BsonExtraElements]
        public BsonDocument Properties { get; set; }

        [BsonElement("reviews")]
        public List<string> Reviews { get; set; } = new List<string>();

        public Dictionary<string, List<Dictionary<string, string>>> PropertiesDict()
        {
            JObject obj = JObject.Parse(Properties.ToJson());
            Dictionary<string, List<Dictionary<string, string>>> kvp = new();
            var properties = obj["properties"];
            foreach (var property in properties)
            {
                foreach (var p in property.Children<JProperty>())
                {
                    List<Dictionary<string, string>> str = new();
                    foreach (var x in p.Value)
                    {
                        foreach (var y in x.Children<JProperty>())
                        {
                            Dictionary<string, string> values = new();
                            values[y.Name] = y.Value.ToString();
                            str.Add(values);
                        }
                    }
                    kvp[p.Name] = str;
                }
            }
            return kvp;
        }

        public string Manufracturer()
        {
            JObject obj = JObject.Parse(Properties.ToJson());
            var properties = obj["properties"];
            foreach (var property in properties)
            {
                foreach (var param in property.Children<JProperty>())
                {
                    foreach (var keys in param.Value)
                    {
                        if (keys["Производитель!"] != null)
                        {
                            return keys["Производитель!"].ToString();
                        }
                    }
                }
            }
            return "null";
        }

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
