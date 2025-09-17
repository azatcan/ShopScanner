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
    public class HepsiBuradaScraper : ISiteScraper
    {
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly BrowserPool _browserPool;
        private readonly ISelectorService _selectorService;

        public HepsiBuradaScraper(IRabbitMqPublisher rabbitMqPublisher, BrowserPool browserPool, ISelectorService selectorService)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
            _browserPool = browserPool;
            _selectorService = selectorService;
        }

        public async Task<List<ProductScrapedDto>> CategoryBasedScrapeAsync(CategoryDto category)
        {
            var selectorConfig = await _selectorService.GetSelectorAsync("HepsiBurada");

            string productLinkSelector = selectorConfig.Selectors["ProductLink"];
            string titleSelector = selectorConfig.Selectors["Title"];
            string priceSelector = selectorConfig.Selectors["PriceDefault"];
            string priceDiscountedSelector = selectorConfig.Selectors["PriceDiscounted"];
            string priceNormalSelector = selectorConfig.Selectors["PriceNormal"];
            string priceCheckoutSelector = selectorConfig.Selectors["PriceCheckout"];

            string imageSelector = selectorConfig.Selectors["Image"];

            var products = new ConcurrentBag<ProductScrapedDto>(); 
            var semaphore = new SemaphoreSlim(5); 

            var page = await _browserPool.GetPageAsync();
            await page.GotoAsync(category.Url, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 180000 });
            await page.WaitForTimeoutAsync(2500);

            for (int i = 0; i < 10; i++)
            {
                await page.EvaluateAsync("window.scrollBy(0, document.body.scrollHeight)");
                await page.WaitForTimeoutAsync(3000);
            }

            await page.WaitForSelectorAsync(productLinkSelector, new PageWaitForSelectorOptions { Timeout = 120000 });
            var productLinks = await page.QuerySelectorAllAsync(productLinkSelector);
            var productUrls = new List<string>();
            foreach (var link in productLinks)
            {
                var href = await link.GetAttributeAsync("href");
                if (string.IsNullOrEmpty(href))
                    continue;

                href = href.Trim();

                if (href.StartsWith("http://") || href.StartsWith("https://"))
                    productUrls.Add(href);
                else if (href.StartsWith("//")) 
                    productUrls.Add("https:" + href);
                else
                    productUrls.Add("https://www.hepsiburada.com" + href);
            }

            await page.Context.CloseAsync(); 

            var tasks = productUrls.Select(async originalUrl =>
            {
                await semaphore.WaitAsync();
                var productPage = await _browserPool.GetPageAsync();

                try
                {
                    await Task.Delay(new Random().Next(1000, 3000));
                    var currentUrl = originalUrl;

                    while (currentUrl.Contains("adservice.hepsiburada.com"))
                    {
                        var uri = new Uri(currentUrl);
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        var redirectUrl = query["redirect"] ?? query["Redirect"];
                        if (string.IsNullOrEmpty(redirectUrl))
                            break;

                        currentUrl = System.Web.HttpUtility.UrlDecode(redirectUrl);
                    }

                    await productPage.GotoAsync(currentUrl, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 180000 });
                    await productPage.WaitForTimeoutAsync(3000);

                    var name = await productPage.InnerTextAsync(titleSelector, new() { Timeout = 60000 });

                    decimal price = 0;
                    try
                    {
                        var priceEl = await productPage.QuerySelectorAsync(priceSelector)
                                ?? await productPage.QuerySelectorAsync(priceDiscountedSelector)
                                ?? await productPage.QuerySelectorAsync(priceNormalSelector)
                                ?? await productPage.QuerySelectorAsync(priceCheckoutSelector);

                        if (priceEl != null)
                        {
                            var priceText = await priceEl.InnerTextAsync();
                            priceText = priceText.Replace("TL", "").Trim().Replace(".", "").Replace(",", ".");
                            decimal.TryParse(priceText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price);
                        }
                    }
                    catch
                    {
                        price = 0; 
                    }

                    var imageEl = await productPage.QuerySelectorAsync(imageSelector);
                    var imageUrl = imageEl != null ? await imageEl.GetAttributeAsync("src") : "/images/default.png";

                    var productDto = new ProductScrapedDto
                    {
                        Name = name,
                        Price = price,
                        Url = currentUrl,
                        ImageUrl = imageUrl ?? "/images/default.png",
                        CategoryId = category.Id,
                        SourceName = "HepsiBurada",
                        ScrapedAt = DateTime.UtcNow,
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

        public Task<List<ProductScrapedDto>> ProductScrapeAsync(ProductScrapedDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
