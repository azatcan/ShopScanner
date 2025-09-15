using ProductService.Domain.Application.Services;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task AddOrUpdateCategoryAsync(Category category)
        {
            var existing = await _repository.GetCategoryByUrlAsync(category.Url);

            if (existing != null)
            {
                existing.Name = category.Name;
                existing.SiteName = category.SiteName;
                existing.Url = category.Url;

                await _repository.UpdateCategoryAsync(existing);
            }
            else
            {
                category.Id = Guid.NewGuid();
                await _repository.AddCategoryAsync(category);
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllCategoriesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _repository.GetCategoryByIdAsync(id);
        }

        public async Task<Category?> GetCategoryByUrlAsync(string url)
        {
            return await _repository.GetCategoryByUrlAsync(url);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            await _repository.DeleteCategoryAsync(id);
        }
    }
}
