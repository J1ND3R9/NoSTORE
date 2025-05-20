using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using NoSTORE.Models;
using System.Globalization;
using Unidecode.NET;

namespace NoSTORE.Models.DTO
{
    public class ProductSearchDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string SEOName { get; set; }
        public string Category { get; set; }
        public string Price { get; set; }
        public int Discount { get; set; }
        public string Image { get; set; }

        public ProductSearchDto(Product p)
        {
            Id = p.Id;
            Name = p.Name;
            Category = p.Category;
            Discount = p.Discount;
            Image = p.Images[0];
            SEOName = p.SEOName;
            Price = p.CorrectPrice(p.FinalPrice);
        }
    }
}
