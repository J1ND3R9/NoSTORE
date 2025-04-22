using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NoSTORE.Models
{
    public class Filter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("properties")]
        public Dictionary<string, List<string>> Properties { get; set; }

        public Dictionary<string, List<string>> PropertiesInDictionary(BsonDocument bson)
        {
            Dictionary<string, List<string>> dict = new();
            JObject root = JObject.Parse(bson.ToJson());
            var properties = root["properties"];
            foreach (var property in properties)
            {
                foreach (var category in property.Children<JProperty>())
                {
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
                                dict[kvp.Name] = strings;
                            }
                        }
                    }
                }
            }
            return dict;
        }

    }
}
