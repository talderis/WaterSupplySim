using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WaterSupplySimulator.Data;
using WaterSupplySimulator.Services;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace WaterSupplySimulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add configuration for rate limiting
            builder.Configuration.AddJsonFile("appsettings.RateLimit.json", optional: true, reloadOnChange: true);

            // Add services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WaterSupplySimulator API",
                    Version = "v1",
                    Description = "API for managing water supply system sensors, pumps, and alerts"
                });
                // Enable JWT authentication in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field. Example: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // CORS: csak a szükséges domainek engedélyezése
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy
                        .WithOrigins("https://localhost:5173", "https://localhost:5174") // mindkét fejlesztői frontend origin engedélyezése
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                );
            });

            // JWT Authentication
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes("SuperSecretKey12345_ChangeMe_1234567890")) // legalább 32 karakter!
                    };
                });

            // Register repositories
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register AutoMapper
            builder.Services.AddAutoMapper(typeof(Program));

            // Register background service
            builder.Services.AddHostedService<SensorDataService>();

            // Register DbContext with SQL Server and EnableRetryOnFailure
            builder.Services.AddDbContext<WaterSystemContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                ));

            // Rate limiting
            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            builder.Services.AddInMemoryRateLimiting();

            // API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            var app = builder.Build();

            // Use global exception middleware
            app.UseMiddleware<Middleware.ExceptionMiddleware>();

            // Use rate limiting middleware
            app.UseIpRateLimiting();

            // HTTPS enforcement (already present)
            app.UseHttpsRedirection();

            // CORS policy alkalmazása
            app.UseCors("AllowFrontend");

            // Authentication & Authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Redirect root to Swagger UI
            app.MapGet("/", ctx =>
            {
                ctx.Response.Redirect("/swagger");
                return Task.CompletedTask;
            });

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WaterSupplySimulator API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.Run();
        }
    }
}