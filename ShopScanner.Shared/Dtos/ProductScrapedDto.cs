using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScanner.Shared.Dtos
{
    public class ProductScrapedDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Url { get; set; }
        public string SourceName { get; set; }
        public string ImageUrl { get; set; }
        public DateTime ScrapedAt { get; set; }
        public Guid CategoryId { get; set; }
    }
}
