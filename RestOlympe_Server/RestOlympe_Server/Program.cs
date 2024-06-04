using Microsoft.EntityFrameworkCore;
using RestOlympe_Server.Data;
using RestOlympe_Server.Hubs;
using RestOlympe_Server.Models.DTO;
using RestOlympe_Server.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestOlympe_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSignalR();
            builder.Services.AddControllers();
            builder.Services.AddScoped<OsmApiService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .SetIsOriginAllowed((host) => true)
                           .AllowCredentials();
                }));

            #region Database

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString), ServiceLifetime.Scoped, ServiceLifetime.Scoped
            );

            #endregion Database

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

            app.MapHub<RestoHub>("/restohub");

            app.UseCors("CorsPolicy");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

                db?.Database.Migrate();
            }

            app.Run();
        }
    }
}
