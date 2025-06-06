﻿using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NoSTORE.Data;
using NoSTORE.Models;
using System.Web.Mvc;

namespace NoSTORE.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;
        public ProductService(MongoDbContext dbContext)
        {
            _products = dbContext.GetCollection<Product>("products");
            var indexKeys = Builders<Product>.IndexKeys.Text(x => x.Name)
                .Text(x => x.Description)
                .Text(x => x.Tags);
            var indexOptions = new CreateIndexOptions { DefaultLanguage = "russian" };
            _products.Indexes.CreateOne(new CreateIndexModel<Product>(indexKeys, indexOptions));
        }

        public async Task<List<Product>> GetAllAsync() => await _products.Find(_ => true).ToListAsync();
        public async Task<List<Product>> GetOnlyDiscount() => await _products.Find(p => p.Discount > 0).ToListAsync();
        public async Task<List<Product>> SearchProducts(string query)
        {
            var filter = Builders<Product>.Filter.Text(query);
            return await _products.Find(filter).ToListAsync();
        }
        public async Task<Product> GetByIdAsync(string id) => await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        public async Task<List<Product>> GetByIdsAsync(List<string> ids)
        {
            var products = await _products.Find(p => ids.Contains(p.Id)).ToListAsync();
            var productDict = products.ToDictionary(p => p.Id);
            return ids.Where(id => productDict.ContainsKey(id)).Select(id => productDict[id]).ToList();
        }

        public async Task<List<Product>> FilterProducts(string category, Dictionary<string, Dictionary<string, List<string>>> filters, int? min, int? max)
        {
            var builder = Builders<Product>.Filter;
            var mongoFilter = builder.Eq(f => f.Category, category);
            foreach (var dict in filters)
            {
                foreach (var list in dict.Value)
                {
                    var title = dict.Key;
                    var name = list.Key;
                    var values = list.Value;
                    var path = $"properties.{title}.{name}";
                    var filter = builder.In(path, values);
                    mongoFilter &= filter;
                }
            }
            if (min != null || max != null)
            {
                var filter1 = builder.Gte("price", min == null ? 0 : min);
                var filter2 = builder.Lte("price", max == null ? 99999999 : max);
                mongoFilter &= filter1;
                mongoFilter &= filter2;
            }
            return await _products.Find(mongoFilter).ToListAsync();
        }
    }
}
