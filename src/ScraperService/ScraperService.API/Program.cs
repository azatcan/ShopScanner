
using MongoDB.Driver;
using ScraperService.Domain.Application.Services;
using ScraperService.Domain.Repositories;
using ScraperService.Infrastructure.Config;
using ScraperService.Infrastructure.RedisCache;
using ScraperService.Infrastructure.Repositories;
using ScraperService.Infrastructure.Services;

namespace ScraperService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddScoped<IMongoClient>(sp =>
            {
                var connectionString = builder.Configuration.GetConnectionString("MongoDb");
                return new MongoClient(connectionString);
            });

            builder.Services.Configure<RedisConfig>(builder.Configuration.GetSection("Redis"));
            builder.Services.AddSingleton<RedisConnection>();
            builder.Services.AddSingleton<ICacheService, RedisCacheService>();

            builder.Services.AddScoped<ISiteSelectorRepository, SiteSelectorRepository>();
            builder.Services.AddScoped<ISelectorService, SelectorService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
