using Microsoft.Extensions.Options;
using ScraperService.Domain.Application.Services;
using ScraperService.Infrastructure.Config;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.RedisCache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;

        public RedisCacheService(RedisConnection redisConnection)
        {
            _db = redisConnection.GetDatabase();
        }

        public async Task SetHashAsync<T>(string hashKey, string field, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.HashSetAsync(hashKey, field, json);

            if (expiry.HasValue)
                await _db.KeyExpireAsync(hashKey, expiry);
        }

        public async Task<T?> GetHashAsync<T>(string hashKey, string field)
        {
            var value = await _db.HashGetAsync(hashKey, field);
            if (value.IsNullOrEmpty) return default;

            return JsonSerializer.Deserialize<T>(value!);
        }
    }
}
