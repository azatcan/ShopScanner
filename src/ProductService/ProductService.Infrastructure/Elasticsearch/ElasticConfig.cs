using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Elasticsearch
{
    public static class ElasticConfig
    {
        public static IElasticClient CreateElasticClient(IConfiguration configuration)
        {
            var uri = configuration["ElasticSearch:Uri"];
            var defaultIndex = configuration["ElasticSearch:DefaultIndex"];

            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("ElasticSearch Uri boş olamaz!");

            var settings = new ConnectionSettings(new Uri(uri))
                .DefaultIndex(defaultIndex ?? "default-index")
                .PrettyJson()
                .DisableDirectStreaming()
                .EnableApiVersioningHeader();

            return new ElasticClient(settings);
        }
    }
}
