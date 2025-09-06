using RabbitMQ.Client;
using ScraperService.Domain.Messaging;
using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.Messaging
{
    public class ProductPublisher : IRabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ProductPublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: "product_exchange",
                type: ExchangeType.Direct,
                durable: true);
        }

        public Task PublishAsync(ProductScrapedDto dto)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
            _channel.BasicPublish(
                exchange: "product_exchange",
                routingKey: "product.created",
                basicProperties: null,
                body: body);

            return Task.CompletedTask;
        }
    }
}
