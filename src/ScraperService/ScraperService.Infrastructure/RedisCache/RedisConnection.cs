using Microsoft.Extensions.Options;
using ScraperService.Infrastructure.Config;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.RedisCache
{
    public class RedisConnection
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        public RedisConnection(IOptions<RedisConfig> config)
        {
            _lazyConnection = new Lazy<ConnectionMultiplexer>(
                () => ConnectionMultiplexer.Connect(config.Value.ConnectionString)
            );
        }

        public ConnectionMultiplexer Connection => _lazyConnection.Value;

        public IDatabase GetDatabase() => Connection.GetDatabase();
    }
}
