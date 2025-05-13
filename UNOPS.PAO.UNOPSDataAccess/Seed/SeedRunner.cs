using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
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

            // Using null as UserResolverService since we don't need it for seeding
            using var context = new UNOPSAppDbContext(
                (DbContextOptions<UNOPSAppDbContext>)options, 
                null, 
                dbContextSchema);

            // Seed data
            Console.WriteLine("Seeding entity permissions...");
            await EntityPermissionSeeder.SeedEntityPermissionsAsync(context);
            Console.WriteLine("Seeding complete!");
        }
    }
} 