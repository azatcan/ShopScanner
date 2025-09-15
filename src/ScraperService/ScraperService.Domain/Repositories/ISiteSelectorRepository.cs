using ScraperService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Repositories
{
    public interface ISiteSelectorRepository
    {
        Task<SiteSelector?> GetSelectorBySiteAsync(string siteName);
        Task InsertAsync(SiteSelector selector);
        Task UpdateAsync(SiteSelector selector);
    }
}
