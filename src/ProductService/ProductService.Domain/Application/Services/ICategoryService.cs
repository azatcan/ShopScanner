using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Application.Services
{
    public interface ICategoryService
    {
        Task AddOrUpdateCategoryAsync(Category category);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category?> GetCategoryByUrlAsync(string url);
        Task DeleteCategoryAsync(Guid id);
    }
}
