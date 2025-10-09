using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class SeedExtensions
    {
        /// <summary>
        /// Seeds the entity permissions data on application startup.
        /// Call this method from your Startup.cs or Program.cs
        /// </summary>
        public static async Task SeedEntityPermissionsOnStartupAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<UNOPSAppDbContext>();
                var configuration = services.GetRequiredService<IConfiguration>();
                // Now handled by generic configurable system
                await GenericSeedRunner.ExecuteConfiguredSeedsAsync(dbContext, services, configuration);
            }
            catch (Exception ex)
            {
                // Log any errors that occur during seeding
                var logger = services.GetRequiredService<ILogger<DataSeeder>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        /// <summary>
        /// Seeds the entity permissions if they don't exist yet.
        /// This method can be called from controllers or services as needed.
        /// </summary>
        public static async Task SeedEntityPermissionsAsync(this UNOPSAppDbContext context, IServiceProvider? serviceProvider = null, IConfiguration? configuration = null)
        {
            // Now handled by generic configurable system
            await GenericSeedRunner.ExecuteConfiguredSeedsAsync(context, serviceProvider, configuration);
        }

        /// <summary>
        /// Add this to configure services to register a hosted service that will seed data on startup
        /// </summary>
        public static IServiceCollection AddDataSeeding(this IServiceCollection services)
        {
            services.AddTransient<DataSeeder>();
            services.AddHostedService<DataSeedingService>();
            return services;
        }
    }

    // Hosted service for seeding data
    public class DataSeedingService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DataSeedingService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedDataAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    // Helper class for seeding data
    public class DataSeeder
    {
        private readonly UNOPSAppDbContext _context;
        private readonly ILogger<DataSeeder> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public DataSeeder(UNOPSAppDbContext context, ILogger<DataSeeder> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                // ALL configuration seeding is now done via generic configurable system!
                _logger.LogInformation("Running all configured seed steps...");
                await GenericSeedRunner.ExecuteConfiguredSeedsAsync(_context, _serviceProvider, _configuration);
                _logger.LogInformation("All seed steps completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
} 