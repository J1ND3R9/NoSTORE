using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NoSTORE.Models
{
    public class FilterDto
    {
        public string Category { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> Properties { get; set; }

        public FilterDto(Filter f)
        {
            Category = f.Category;
            Properties = f.Properties;
        }
    }
    public class Filter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("properties")]
        public Dictionary<string, Dictionary<string, List<string>>> Properties { get; set; }

        public Dictionary<string, Dictionary<string, List<string>>> PropertiesInDictionary(BsonDocument bson)
        {
            Dictionary<string, Dictionary<string, List<string>>> dict = new();
            JObject root = JObject.Parse(bson.ToJson());
            var properties = root["properties"];
            foreach (var property in properties)
            {
                foreach (var category in property.Children<JProperty>())
                {
                    string parent = category.Name;
                    Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
                    foreach (var item in category.Value)
                    {
                        foreach (var kvp in item.Children<JProperty>())
                        {
                            if (kvp.Name.Contains('!'))
                            {
                                bool isArray = false;
                                List<string> strings = new();
                                foreach (var values in kvp.Value)
                                {
                                    isArray = true;
                                    strings.Add(values.ToString());
                                }
                                if (!isArray)
                                    strings.Add(kvp.Value.ToString());
                                keyValuePairs[kvp.Name] = strings;
                            }
                        }
                    }
                    dict[parent] = keyValuePairs;
                }
            }
            return dict;
        }

    }
}
