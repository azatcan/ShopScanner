using MongoDB.Driver;
using ScraperService.Domain.Application.Scriping.Abstract;
using ScraperService.Domain.Application.Services;
using ScraperService.Domain.Messaging;
using ScraperService.Domain.Repositories;
using ScraperService.Infrastructure.APIClients;
using ScraperService.Infrastructure.Messaging;
using ScraperService.Infrastructure.Repositories;
using ScraperService.Infrastructure.Scriping;
using ScraperService.Infrastructure.Scriping.Concrete;
using ScraperService.Infrastructure.Services;

namespace ScraperService.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices((hostContext, services) =>
                 {
                     services.AddSingleton<IRabbitMqPublisher, ProductPublisher>();

                     services.AddSingleton<BrowserPool>();
                     services.AddTransient<HepsiBuradaScraper>();
                     services.AddTransient<TrendyolScraper>();

                     services.AddScoped<ISiteScraperFactory, SiteScraperFactory>();
                     services.AddScoped<ISelectorService, SelectorService>();
                     services.AddScoped<ISiteSelectorRepository, SiteSelectorRepository>();

                     services.AddSingleton<IMongoClient>(sp =>
                     {
                         var connectionString = hostContext.Configuration.GetConnectionString("MongoDb");
                         return new MongoClient(connectionString);
                     });

                     services.AddHttpClient<CategoryApiClient>(client =>
                     {
                         client.BaseAddress = new Uri("https://localhost:7059");
                         client.Timeout = TimeSpan.FromMinutes(2);
                     });

                     // Worker servisini ekle
                     services.AddHostedService<Worker>();
                 })
                 .Build();



            await host.RunAsync();
        }
    }
}