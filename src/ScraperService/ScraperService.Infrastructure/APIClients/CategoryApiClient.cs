using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.APIClients
{
    public class CategoryApiClient
    {
        private readonly HttpClient _httpClient;

        public CategoryApiClient(HttpClient httpClient)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            _httpClient = httpClient;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Categories/get", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<List<CategoryDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CategoryApiClient error: {ex.Message}");
                throw;
            }
        }

    }
}
