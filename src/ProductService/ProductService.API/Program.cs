
using Microsoft.EntityFrameworkCore;
using Nest;
using ProductService.Domain.Application.Services;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Elasticsearch;
using ProductService.Infrastructure.Messaging;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Services;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace ProductService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefalutConnection")));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "scraper-logs-test",
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    FailureCallback = e => Console.WriteLine("Elasticsearch log gönderilemedi!"),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                    EmitEventFailureHandling.WriteToFailureSink |
                    EmitEventFailureHandling.RaiseCallback
                })
                .CreateLogger();

            Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine("Serilog Error: " + msg));

            builder.Services.AddSingleton<IElasticClient>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return ElasticConfig.CreateElasticClient(config);
            });

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService.Infrastructure.Services.ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddHostedService<ProductCreatedConsumer>();
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
            app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);


            app.Run();
        }
    }
}
