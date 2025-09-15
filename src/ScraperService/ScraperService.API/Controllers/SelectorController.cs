using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScraperService.Domain.Application.Services;
using ScraperService.Domain.Entities;
using ScraperService.Domain.Repositories;

namespace ScraperService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelectorController : ControllerBase
    {
        private readonly ISelectorService _selectorService;

        public SelectorController(ISelectorService selectorService)
        {
            _selectorService = selectorService;
        }

        [HttpPost("addOrUpdate")]
        public async Task<IActionResult> AddOrUpdateSelector([FromBody] SiteSelector selector)
        {
            if (selector == null || string.IsNullOrEmpty(selector.SiteName))
                return BadRequest("Selector or SiteName cannot be null.");

            var existing = await _selectorService.GetSelectorAsync(selector.SiteName);

            if (existing != null)
            {
                selector.Id = existing.Id;
            }
            else
            {
                selector.Id = null;
            }

            await _selectorService.AddOrUpdateSelectorAsync(selector);
            return Ok(selector);
        }

        [HttpGet("{siteName}")]
        public async Task<IActionResult> GetSelector(string siteName)
        {
            var selector = await _selectorService.GetSelectorAsync(siteName);
            if (selector == null)
                return NotFound();
            return Ok(selector);
        }
    }
}
