using ScraperService.Domain.Application.Scriping.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.Scriping.Concrete
{
    public class SiteScraperFactory : ISiteScraperFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SiteScraperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISiteScraper GetScraper(string siteName)
        {
            return siteName switch
            {
                "Trendyol" => _serviceProvider.GetRequiredService<TrendyolScraper>(),
                "HepsiBurada" => _serviceProvider.GetRequiredService<HepsiBuradaScraper>(),
                _ => throw new NotImplementedException($"Scraper for {siteName} not implemented")
            };
        }
    }
}
