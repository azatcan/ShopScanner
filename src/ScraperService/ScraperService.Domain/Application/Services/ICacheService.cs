using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Application.Services
{
    public interface ICacheService
    {
        Task SetHashAsync<T>(string hashKey, string field, T value, TimeSpan? expiry = null);
        Task<T?> GetHashAsync<T>(string hashKey, string field);
    }
}
