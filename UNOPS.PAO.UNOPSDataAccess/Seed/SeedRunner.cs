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

            // Seed data in proper order
            Console.WriteLine("Seeding entities...");
            await EntitiesSeeder.SeedEntitiesAsync(context);
            Console.WriteLine("Entities seeded successfully.");

            Console.WriteLine("Seeding entity managers...");
            await EntityManagerSeeder.SeedEntityManagersAsync(context);
            Console.WriteLine("Entity managers seeded successfully.");

            Console.WriteLine("Seeding entity field managers...");
            await EntityManagerSeeder.SeedEntityFieldManagersAsync(context);
            Console.WriteLine("Entity field managers seeded successfully.");

            Console.WriteLine("Seeding document types...");
            await DocumentTypeSeeder.SeedDocumentTypesAsync(context);
            Console.WriteLine("Document types seeded successfully.");

            Console.WriteLine("Seeding AI prompts...");
            await AiPromptSeeder.SeedAiPromptsAsync(context);
            Console.WriteLine("AI prompts seeded successfully.");

            Console.WriteLine("Seeding entity permissions...");
            await EntityPermissionSeeder.SeedEntityPermissionsAsync(context);
            Console.WriteLine("Entity permissions seeded successfully.");

            Console.WriteLine("Seeding liaison offices...");
            await LiaisonOfficeSeeder.SeedLiaisonOfficesAsync(context);
            Console.WriteLine("Liaison offices seeded successfully.");

            Console.WriteLine("Seeding partner tree...");
            await PartnerTreeSeeder.SeedPartnerTreesAsync(context);
            Console.WriteLine("Partner tree seeded successfully.");

            Console.WriteLine("Seeding partners...");
            await PartnerSeeder.SeedPartnersAsync(context);
            Console.WriteLine("Partners seeded successfully.");

            Console.WriteLine("All configuration data seeding complete!");
        }
    }
} 