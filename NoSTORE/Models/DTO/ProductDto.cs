﻿using Unidecode.NET;

namespace NoSTORE.Models.DTO
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SEOName =>
            Name.Unidecode().ToLower()
                .Replace(' ', '-')
                .Replace(".", "")
                .Replace(",", "")
                .Replace("\'", "")
                .Replace("\"", "");
        public string Tags { get; set; }
        public int Price { get; set; }
        public int FinalPrice { get; set; }
        public int Discount { get; set; }
        public string Image { get; set; }
        public List<string>? Images { get; set; }
        public int Quantity { get; set; }
        public double Rating { get; set; }
        public string FinalPriceString { get; set; }
        public List<ReviewDto> Reviews { get; set; }
        public string Description { get; set; }
        public Dictionary<string, List<Dictionary<string, string>>> Properties { get; set; }

        public ProductDto() { }

        public ProductDto(Product p)
        {
            Id = p.Id;
            Name = p.Name;
            Price = p.Price;
            FinalPrice = p.FinalPrice;
            Discount = p.Discount;
            Image = p.Images[0];
            Quantity = p.Quantity;
            Tags = p.TagsInString();
            Rating = p.Rating;
            FinalPriceString = p.CorrectPrice(p.FinalPrice);
            Description = p.Description;
            Properties = p.PropertiesDict();
            Images = p.Images;
        }
    }
}
