using Microsoft.Playwright;
using ScraperService.Domain.Application.Scriping.Abstract;
using ScraperService.Domain.Application.Services;
using ScraperService.Domain.Messaging;
using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.Scriping.Concrete
{
    public class TrendyolScraper : ISiteScraper
    {

        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly BrowserPool _browserPool;
        private readonly ISelectorService _selectorService;

        public TrendyolScraper(IRabbitMqPublisher rabbitMqPublisher, BrowserPool browserPool, ISelectorService selectorService)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
            _browserPool = browserPool;
            _selectorService = selectorService;
        }

        public async Task<List<ProductScrapedDto>> CategoryBasedScrapeAsync(CategoryDto category)
        {
            var selectorConfig = await _selectorService.GetSelectorAsync("Trendyol");

            string productLinkSelector = selectorConfig.Selectors["ProductLink"];
            string titleSelector = selectorConfig.Selectors["Title"];
            string priceSelector = selectorConfig.Selectors["Price"];
            string imageSelector = selectorConfig.Selectors["Image"];

            var products = new ConcurrentBag<ProductScrapedDto>();
            var semaphore = new SemaphoreSlim(5); 

            var page = await _browserPool.GetPageAsync();
            await page.GotoAsync(category.Url, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 180000 });
            await page.WaitForTimeoutAsync(2500);

            for (int i = 0; i < 5; i++)
            {
                await page.EvaluateAsync("window.scrollBy(0, document.body.scrollHeight)");
                await page.WaitForTimeoutAsync(1500);
            }

            var productLinks = await page.QuerySelectorAllAsync(productLinkSelector);
            var productUrls = new List<string>();
            foreach (var link in productLinks)
            {
                var url = await link.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(url))
                    productUrls.Add("https://www.trendyol.com" + url);
            }

            await page.Context.CloseAsync();

            var tasks = productUrls.Select(async productUrl =>
            {
                await semaphore.WaitAsync();
                var productPage = await _browserPool.GetPageAsync();

                try
                {
                    await productPage.GotoAsync(productUrl, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.Load,
                        Timeout = 180000
                    });
                    await productPage.WaitForTimeoutAsync(3000);

                    var name = await productPage.InnerTextAsync(titleSelector, new() { Timeout = 60000 });

                    var priceEl = await productPage.QuerySelectorAsync(priceSelector);
                    decimal price = 0;

                    if (priceEl != null)
                    {
                        var priceText = await priceEl.InnerTextAsync();
                        decimal.TryParse(
                            priceText.Replace("TL", "").Replace(".", "").Replace(",", ".").Trim(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out price
                        );
                    }

                    var imageEl = await productPage.QuerySelectorAsync(imageSelector);
                    var imageUrl = imageEl != null
                        ? await imageEl.GetAttributeAsync("src") ?? await imageEl.GetAttributeAsync("data-src") ?? ""
                        : "/images/default.png";

                    if (string.IsNullOrWhiteSpace(imageUrl))
                        imageUrl = "/images/default.png";

                    var productDto = new ProductScrapedDto
                    {
                        Name = name,
                        Price = price,
                        Url = productUrl,
                        ImageUrl = imageUrl,
                        CategoryId = category.Id,
                        SourceName = "Trendyol",
                        ScrapedAt = DateTime.UtcNow
                    };

                    await _rabbitMqPublisher.PublishAsync(productDto);
                    products.Add(productDto);
                }
                finally
                {
                    await productPage.Context.CloseAsync();
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            return products.ToList();
        }


        public async Task<List<ProductScrapedDto>> ProductScrapeAsync(ProductScrapedDto dto)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(dto.Url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 120000
            });

            var nameEl = await page.QuerySelectorAsync("h1.product-title");
            var priceEl = await page.QuerySelectorAsync(".price.normal-price .discounted");
            var imageEl = await page.QuerySelectorAsync("img._carouselImage_abb7111");

            string name = nameEl != null ? await nameEl.InnerTextAsync() : "Bilinmeyen Ürün";
            string priceText = priceEl != null ? await priceEl.InnerTextAsync() : "0";
            decimal.TryParse(priceText.Replace("TL", "").Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);
            string imageUrl = imageEl != null ? await imageEl.GetAttributeAsync("src") : "";

            var productDto = new ProductScrapedDto
            {
                Name = name,
                Price = price,
                Url = dto.Url,
                ImageUrl = imageUrl ?? "/images/default.png",
                SourceName = "Trendyol",
                ScrapedAt = DateTime.UtcNow,
                CategoryId = dto.CategoryId
            };

            await _rabbitMqPublisher.PublishAsync(productDto);

            await browser.CloseAsync();

            return new List<ProductScrapedDto> { productDto };
        }
    }
}
