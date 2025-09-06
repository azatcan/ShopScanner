using Microsoft.EntityFrameworkCore;
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

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
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
    }
}
