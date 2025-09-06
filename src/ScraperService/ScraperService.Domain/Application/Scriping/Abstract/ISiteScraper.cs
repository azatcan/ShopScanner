using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Application.Scriping.Abstract
{
    public interface ISiteScraper
    {
        Task<List<ProductScrapedDto>> CategoryBasedScrapeAsync(CategoryDto category);
        Task<List<ProductScrapedDto>> ProductScrapeAsync(ProductScrapedDto dto);
    }
}
