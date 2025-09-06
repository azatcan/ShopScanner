using Microsoft.Playwright;
using ScraperService.Domain.Application.Scriping.Abstract;
using ShopScanner.Shared.Dtos;

namespace ScraperService.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISiteScraper _scraper;

        public Worker(ILogger<Worker> logger, ISiteScraper scraper)
        {
            _logger = logger;
            _scraper = scraper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Scraper çalýþýyor: {time}", DateTimeOffset.Now);

                // Örnek: kategorileri çekip scrape et
                var categories = new List<CategoryDto>
            {
                new CategoryDto {  Id = Guid.Parse("bc46325a-3e86-4090-86ff-372d8773c146"), Url = "https://www.trendyol.com/laptop" },
                new CategoryDto { Id = Guid.Parse("7a756072-b4cf-4a2b-895f-a80791befe0c"), Url = "https://www.trendyol.com/telefon" }
            };

                foreach (var category in categories)
                {
                    await _scraper.CategoryBasedScrapeAsync(category);
                }

                // 2 saatte bir çalýþacak
                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
        }
    }
}
