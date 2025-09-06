using ScraperService.Domain.Application.Scriping.Abstract;
using ScraperService.Domain.Application.Scriping.Concrete;
using ScraperService.Domain.Messaging;
using ScraperService.Infrastructure.Messaging;

namespace ScraperService.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices((hostContext, services) =>
                 {
                     // RabbitMQ publisher ekle
                     services.AddSingleton<IRabbitMqPublisher, ProductPublisher>();

                     // Trendyol scraper ekle
                     services.AddSingleton<ISiteScraper, TrendyolScraper>();

                     // Worker servisini ekle
                     services.AddHostedService<Worker>();
                 })
                 .Build();

            await host.RunAsync();
        }
    }
}