using Microsoft.Playwright;
using ScraperService.Domain.Application.Scriping.Abstract;
using ScraperService.Domain.Messaging;
using ShopScanner.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Domain.Application.Scriping.Concrete
{
    public class TrendyolScraper : ISiteScraper
    {

        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public TrendyolScraper(IRabbitMqPublisher rabbitMqPublisher)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public async Task<List<ProductScrapedDto>> CategoryBasedScrapeAsync(CategoryDto category)
        {
            var products = new List<ProductScrapedDto>();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Timeout = 60000
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
                Locale = "tr-TR"
            });

            var page = await context.NewPageAsync();
            await page.GotoAsync(category.Url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForTimeoutAsync(2500);

            for (int i = 0; i < 3; i++)
            {
                await page.EvaluateAsync("window.scrollBy(0, document.body.scrollHeight)");
                await page.WaitForTimeoutAsync(1500);
            }

            var productLinks = await page.QuerySelectorAllAsync(".p-card-wrppr a");
            var productUrls = new List<string>();
            foreach (var link in productLinks)
            {
                var url = await link.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(url))
                    productUrls.Add("https://www.trendyol.com" + url);
            }

            foreach (var productUrl in productUrls)
            {
                await page.GotoAsync(productUrl);
                await page.WaitForTimeoutAsync(5500);

                var name = await page.InnerTextAsync("h1.product-title", new() { Timeout = 60000 });
                var priceEl = await page.QuerySelectorAsync(".price.normal-price .discounted");

                decimal price = 0;

                if (priceEl != null)
                {
                    var priceText = await priceEl.InnerTextAsync();
                    decimal.TryParse(
                        priceText.Replace("TL", "")
                                 .Replace(".", "")
                                 .Replace(",", ".")
                                 .Trim(),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out price
                    );
                }
                else
                {
                    var normalPriceEl = await page.QuerySelectorAsync(".price.normal-price .price-container");
                    if (normalPriceEl != null)
                    {
                        var priceText = await normalPriceEl.InnerTextAsync();
                        decimal.TryParse(
                            priceText.Replace("TL", "")
                                     .Replace(".", "")
                                     .Replace(",", ".")
                                     .Trim(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out price
                        );
                    }
                }


                var imageEl = await page.QuerySelectorAsync("img[data-testid='image']");
                string imageUrl = "";

                if (imageEl != null)
                {
                    imageUrl = await imageEl.GetAttributeAsync("src")
                               ?? await imageEl.GetAttributeAsync("data-src")
                               ?? "";
                }

                if (string.IsNullOrWhiteSpace(imageUrl))
                    imageUrl = "/images/default.png";

                var categoryMapping = new Dictionary<Guid, Guid>
{
    { new Guid("bc46325a-3e86-4090-86ff-372d8773c146"), new Guid("bc46325a-3e86-4090-86ff-372d8773c146") },
    { new Guid("7a756072-b4cf-4a2b-895f-a80791befe0c"), new Guid("7a756072-b4cf-4a2b-895f-a80791befe0c") }
};

                Guid categoryIdMapped = categoryMapping.ContainsKey(category.Id)
                    ? categoryMapping[category.Id]
                    : new Guid("bc46325a-3e86-4090-86ff-372d8773c146");

                var productDto = new ProductScrapedDto
                {
                    Name = name,
                    Price = price,
                    Url = productUrl,
                    ImageUrl = imageUrl ?? "/images/default.png",
                    CategoryId = categoryIdMapped,
                    SourceName = "Trendyol",
                    ScrapedAt = DateTime.UtcNow,
                };

                await _rabbitMqPublisher.PublishAsync(productDto);
                products.Add(productDto);
            }

            await browser.CloseAsync();
            return products;
        }


        public async Task<List<ProductScrapedDto>> ProductScrapeAsync(ProductScrapedDto dto)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(dto.Url);
            await page.WaitForTimeoutAsync(2000);

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
