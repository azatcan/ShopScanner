using Microsoft.Playwright;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Infrastructure.Scriping
{
    public class BrowserPool : IAsyncDisposable
    {
        private readonly IPlaywright _playwright;
        private readonly IBrowser _browser;
        private readonly ConcurrentBag<IBrowserContext> _browserContexts = new();

        public BrowserPool()
        {
            _playwright = Microsoft.Playwright.Playwright.CreateAsync().GetAwaiter().GetResult();
            _browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true, 
                Timeout = 60000
            }).GetAwaiter().GetResult();
        }

        public async Task<IPage> GetPageAsync()
        {
            var context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
                Locale = "tr-TR"
            });

            _browserContexts.Add(context);
            return await context.NewPageAsync();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var ctx in _browserContexts)
                await ctx.CloseAsync();

            await _browser.CloseAsync();
            _playwright.Dispose();
        }
    }
}
