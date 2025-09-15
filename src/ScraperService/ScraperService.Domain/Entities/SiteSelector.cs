using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Entities
{
    public class SiteSelector
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        public string SiteName { get; set; } 

        public Dictionary<string, string> Selectors { get; set; } 

        public DateTime UpdatedAt { get; set; } 
    }
}
