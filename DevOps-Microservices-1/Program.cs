
using Microsoft.OpenApi.Models;

namespace DevOps_Microservices_1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API v1",
                    Version = "v1",
                    Description = "API documentation for my .NET 10 application"
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // Native .NET 10 mapping
                app.MapOpenApi();

                // Enable Swashbuckle Swagger generation and UI
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    // Points to the generated OpenAPI JSON document
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

                    // Optional: To serve Swagger UI at the app's root (http://localhost:<port>/), uncomment below
                    // options.RoutePrefix = string.Empty; 
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
