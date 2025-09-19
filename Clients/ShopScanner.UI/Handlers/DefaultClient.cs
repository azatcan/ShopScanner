using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ShopScanner.UI.Handlers
{
    public class DefaultClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public DefaultClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _contextAccessor = contextAccessor;
        }

        private HttpClient CreateClient(string clientName = "DefaultClient")
        {
            var client = _httpClientFactory.CreateClient(clientName);

            var token = _contextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<T?> GetAsync<T>(string url, string clientName = "DefaultClient")
        {
            var client = CreateClient(clientName);
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, string clientName = "DefaultClient")
        {
            var client = CreateClient(clientName);

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode) return default;

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(responseJson);
        }

        public async Task<bool> PutAsync<TRequest>(string url, TRequest data, string clientName = "DefaultClient")
        {
            var client = CreateClient(clientName);

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string url, string clientName = "DefaultClient")
        {
            var client = CreateClient(clientName);
            var response = await client.DeleteAsync(url);

            return response.IsSuccessStatusCode;
        }
    }
}
