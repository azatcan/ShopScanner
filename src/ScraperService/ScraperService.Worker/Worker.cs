using Microsoft.Playwright;
using Polly;
using Polly.Retry;
using ScraperService.Domain.Application.Scriping.Abstract;
using ScraperService.Infrastructure.APIClients;
using ShopScanner.Shared.Dtos;
using System;

namespace ScraperService.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CategoryApiClient _categoryApiClient;
        private readonly AsyncRetryPolicy _retryPolicy;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, CategoryApiClient categoryApiClient)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _categoryApiClient = categoryApiClient;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning("Retry {retryCount} failed: {message}", retryCount, exception.Message);
                    });
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var categories = await _retryPolicy.ExecuteAsync(() =>
                        _categoryApiClient.GetCategoriesAsync(stoppingToken));

                    foreach (var category in categories)
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var scraperFactory = scope.ServiceProvider.GetRequiredService<ISiteScraperFactory>();
                        var scraper = scraperFactory.GetScraper(category.SiteName);

                        var products = await scraper.CategoryBasedScrapeAsync(category);

                        _logger.LogInformation("{count} ürün bulundu: {category}", products.Count, category.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Scraping error: {message}", ex.Message);
                }

                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
        }
    }
}
