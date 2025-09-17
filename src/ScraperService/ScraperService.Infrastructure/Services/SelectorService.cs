using ScraperService.Domain.Application.Services;
using ScraperService.Domain.Entities;
using ScraperService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.Services
{
    public class SelectorService : ISelectorService
    {
        private readonly ISiteSelectorRepository _repository;
        private readonly ICacheService _cache;
        private const string HashKey = "site_selectors";

        public SelectorService(ISiteSelectorRepository repository, ICacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task AddOrUpdateSelectorAsync(SiteSelector selector)
        {
            var existing = await _repository.GetSelectorBySiteAsync(selector.SiteName);

            if (existing != null)
            {
                selector.Id = existing.Id; 
                await _repository.UpdateAsync(selector);
            }
            else
            {
                await _repository.InsertAsync(selector);
            }

            await _cache.SetHashAsync(HashKey, selector.SiteName, selector, TimeSpan.FromHours(1));
        }

        public async Task<SiteSelector?> GetSelectorAsync(string siteName)
        {
            var cached = await _cache.GetHashAsync<SiteSelector>(HashKey, siteName);
            if (cached != null)
                return cached;

            var fromDb = await _repository.GetSelectorBySiteAsync(siteName);
            if (fromDb != null)
            {
                await _cache.SetHashAsync(HashKey, siteName, fromDb, TimeSpan.FromHours(1));
            }

            return fromDb;
        }
    }

}
