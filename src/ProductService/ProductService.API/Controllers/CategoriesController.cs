using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Domain.Application.Services;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Services;
using System.Threading.Tasks;

namespace ProductService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public CategoriesController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get() 
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var results = await _productService.SearchProductsAsync(keyword);
            return Ok(results);
        }
    }
}
