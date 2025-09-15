using MongoDB.Bson;
using MongoDB.Driver;
using ScraperService.Domain.Entities;
using ScraperService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.Repositories
{
    public class SiteSelectorRepository : ISiteSelectorRepository
    {
        private readonly IMongoCollection<SiteSelector> _collection;

        public SiteSelectorRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("ScraperDb"); 
            _collection = database.GetCollection<SiteSelector>("SiteSelectors"); 
        }

        public async Task<SiteSelector?> GetSelectorBySiteAsync(string siteName)
        {
            return await _collection.Find(s => s.SiteName == siteName)
                                    .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(SiteSelector selector)
        {
            selector.UpdatedAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(selector.Id))
                selector.Id = ObjectId.GenerateNewId().ToString();
            await _collection.InsertOneAsync(selector);
        }

        public async Task UpdateAsync(SiteSelector selector)
        {
            selector.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<SiteSelector>.Filter.Eq(s => s.Id, selector.Id);
            await _collection.ReplaceOneAsync(filter, selector);
        }
    }
}
