using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.Server;

using Lamar.Microsoft.DependencyInjection;

public class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return Host.CreateDefaultBuilder(args)
            .UseLamar()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel((context, options) =>
                {
                    options.ListenAnyIP(7123, listenOptions =>
                    {
                        listenOptions.Protocols = environment is null or "Development"
                            ? HttpProtocols.Http1AndHttp2
                            : HttpProtocols.Http2;
                        if (environment is null or "Development") listenOptions.UseHttps();
                    });
                    options.ListenAnyIP(5159, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1;
                    });
                    options.Limits.MaxRequestBodySize = null;
                    options.Limits.MaxResponseBufferSize = null;
                });

                webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                webBuilder.UseIISIntegration();
                webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureServices(services =>
                {
                    // This is important, the call to AddControllers()
                    // cannot be made before the usage of ConfigureWebHostDefaults

                    // Add Web API services
                    services.AddControllers()
                        .AddApplicationPart(typeof(UNOPSPresentation.AssemblyReference).Assembly)
                        .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly)
                        .AddJsonOptions(options =>
                        {
                            // Configure JSON serialization to use camelCase naming policy
                            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                            // Enable case-insensitive property name matching for deserialization
                            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                            // Add global JsonStringEnumConverter to ensure consistent enum serialization
                            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        });
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAll", builder =>
                        {
                            builder
                                .AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                    });
                    if (environment is null or "Development")
                    {
                        // Expose the WebAPI endpoints to swagger
                        services.AddEndpointsApiExplorer();
                        services.AddSwaggerGen();
                    }
                });
            });
    }

    public static void Main(string[] args)
    {
        var app = CreateHostBuilder(args).Build();
        
        // Data seeding is now triggered manually via API endpoint: POST /api/system-admin/seeding/run
        // Role/permission seeding still happens automatically in Startup.cs
        
        app.Run();
    }
}