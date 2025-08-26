using Microsoft.EntityFrameworkCore;
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
                await EntityPermissionSeeder.SeedEntityPermissionsAsync(dbContext);
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
        public static async Task SeedEntityPermissionsAsync(this UNOPSAppDbContext context)
        {
            await EntityPermissionSeeder.SeedEntityPermissionsAsync(context);
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

        public DataSeeder(UNOPSAppDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Seeding entities...");
                await EntitiesSeeder.SeedEntitiesAsync(_context);
                _logger.LogInformation("Entities seeded successfully.");

                _logger.LogInformation("Seeding entity managers...");
                await EntityManagerSeeder.SeedEntityManagersAsync(_context);
                _logger.LogInformation("Entity managers seeded successfully.");

                _logger.LogInformation("Seeding entity field managers...");
                await EntityManagerSeeder.SeedEntityFieldManagersAsync(_context);
                _logger.LogInformation("Entity field managers seeded successfully.");

                _logger.LogInformation("Seeding document types...");
                await DocumentTypeSeeder.SeedDocumentTypesAsync(_context);
                _logger.LogInformation("Document types seeded successfully.");

                _logger.LogInformation("Seeding AI prompts...");
                await AiPromptSeeder.SeedAiPromptsAsync(_context);
                _logger.LogInformation("AI prompts seeded successfully.");

                _logger.LogInformation("Seeding entity permissions...");
                await EntityPermissionSeeder.SeedEntityPermissionsAsync(_context);
                _logger.LogInformation("Entity permissions seeded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
} 