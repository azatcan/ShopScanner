
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
             {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "AuthService",
                    ValidAudience = "MicroservicesApp",
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("super_secret_key_for_jwt_12345"))
                };
            });

            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

            builder.Services.AddOcelot();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            await app.UseOcelot();
            app.MapControllers();

            app.Run();
        }
    }
}
