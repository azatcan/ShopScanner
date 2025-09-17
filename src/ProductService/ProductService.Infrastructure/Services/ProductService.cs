using Microsoft.EntityFrameworkCore;
using Nest;
using ProductService.Domain.Application.DTOs;
using ProductService.Domain.Application.Services;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IElasticClient _elasticClient;

        public ProductService(IProductRepository repository, IElasticClient elasticClient)
        {
            _repository = repository;
            _elasticClient = elasticClient;
        }

        public async Task AddProductAsync(Product product)
        {
            var existing = await _repository.GetProductsBySourceAsync(product.Source);
            if (existing.Any(p => p.Url == product.Url))
                return; 

            await _repository.AddProductAsync(product);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _repository.GetAllProductsAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _repository.GetProductByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductsBySourceAsync(string sourceName)
        {
            return await _repository.GetProductsBySourceAsync(sourceName);
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal min, decimal max)
        {
            var allProducts = await _repository.GetAllProductsAsync();
            return allProducts.Where(p => p.Price >= min && p.Price <= max);
        }

        public async Task<IEnumerable<ProductIndex>> SearchProductsAsync(string keyword)
        {
            var response = await _elasticClient.SearchAsync<ProductIndex>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                         .Fields(f => f
                            .Field(p => p.Name)
                            )
                        .Query(keyword)
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            return response.Documents;
        }

        public async Task AddOrUpdateProductAsync(Product product)
        {
            var existing = await _repository.GetProductByUrlAsync(product.Url);

            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.ImageUrl = product.ImageUrl;
                existing.CategoryId = product.CategoryId;
                existing.Source = product.Source;
                //existing.UpdatedAt = DateTime.UtcNow; 

                await _repository.UpdateProductAsync(existing);

                var productIndex = new ProductIndex
                {
                    Name = existing.Name,
                    Price = existing.Price,
                    Category = existing.Category?.Name, 
                };

                await _elasticClient.IndexDocumentAsync(productIndex);
            }
            else
            {
                product.CreatedAt = DateTime.UtcNow;
                await _repository.AddProductAsync(product);

                var productIndex = new ProductIndex
                {
                    Name = product.Name,
                    Price = product.Price,
                    Category = product.Category?.Name,
                };

                await _elasticClient.IndexDocumentAsync(productIndex);
            }
        }
    }
}
