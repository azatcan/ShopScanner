using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(ProductScrapedDto dto);
    }
}
