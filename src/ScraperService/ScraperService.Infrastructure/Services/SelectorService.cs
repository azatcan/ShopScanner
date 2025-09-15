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

        public SelectorService(ISiteSelectorRepository repository)
        {
            _repository = repository;
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
        }

        public async Task<SiteSelector?> GetSelectorAsync(string siteName)
        {
            return await _repository.GetSelectorBySiteAsync(siteName);
        }
    }

}
