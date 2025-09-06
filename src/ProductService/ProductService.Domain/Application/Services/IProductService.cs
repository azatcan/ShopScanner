using ProductService.Domain.Entities;
using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Application.Services
{
    public interface IProductService
    {
        Task AddProductAsync(Product product);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetProductsBySourceAsync(string sourceName);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal min, decimal max);
    }
}
