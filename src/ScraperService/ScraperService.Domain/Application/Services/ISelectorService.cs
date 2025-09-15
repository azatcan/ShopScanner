using ScraperService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Application.Services
{
    public interface ISelectorService
    {
        Task AddOrUpdateSelectorAsync(SiteSelector selector);
        Task<SiteSelector?> GetSelectorAsync(string siteName);
    }
}
