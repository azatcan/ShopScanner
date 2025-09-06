using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Domain.Application.Services;
using ProductService.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopScanner.Shared.Dtos;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ProService = ProductService.Infrastructure.Services.ProductService;

namespace ProductService.Infrastructure.Messaging
{
    public class ProductCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;

        public ProductCreatedConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("product_exchange", ExchangeType.Direct, durable: true);
            _channel.QueueDeclare("product_created_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("product_created_queue", "product_exchange", "product.created");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var productDto = JsonSerializer.Deserialize<ProductScrapedDto>(message);

                if (productDto != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IProductService>();

                    var product = new Product
                    {
                        Name = productDto.Name,
                        Price = productDto.Price,
                        Url = productDto.Url,
                        ImageUrl = string.IsNullOrWhiteSpace(productDto.ImageUrl) ? "/images/default.png" : productDto.ImageUrl,
                        CategoryId = productDto.CategoryId,
                        Source = productDto.SourceName,
                        CreatedAt = productDto.ScrapedAt
                    };

                    await service.AddProductAsync(product);
                }
            };

            _channel.BasicConsume("product_created_queue", autoAck: true, consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
