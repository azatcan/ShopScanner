using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Application.Scriping.Abstract
{
    public interface ISiteScraperFactory
    {
        ISiteScraper GetScraper(string siteName);
    }
}
