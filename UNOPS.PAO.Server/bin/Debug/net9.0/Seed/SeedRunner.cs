using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public class SeedRunner
    {
        public static async Task Main(string[] args)
        {
            // Build configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Get connection string
            string connectionString = configuration.GetConnectionString("DbContext") 
                ?? throw new InvalidOperationException("Connection string 'DbContext' not found.");
            
            string schema = configuration.GetConnectionString("DbSchema") ?? "public";

            // Create DbContext
            var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            // Create DbContextSchema
            var dbContextSchema = new DbContextSchema(schema);

            // Create a dummy UserResolverService for seeding (we don't need real user resolution during seeding)
            var dummyUserResolver = new UserResolverService<int>(null!, null!);
            using var context = new UNOPSAppDbContext(
                (DbContextOptions<UNOPSAppDbContext>)options, 
                dummyUserResolver, 
                dbContextSchema);

            // Seed data using new generic configuration-driven system
            Console.WriteLine("Running all configured seed steps...");
            // Note: serviceProvider is null here - seeders that require it won't work in this standalone context
            await GenericSeedRunner.ExecuteConfiguredSeedsAsync(context, serviceProvider: null, configuration);
            Console.WriteLine("All seed steps completed successfully.");

            Console.WriteLine("All configuration data seeding complete!");
        }
    }
} 