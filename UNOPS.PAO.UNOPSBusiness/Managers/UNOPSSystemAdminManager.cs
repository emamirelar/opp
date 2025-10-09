namespace UNOPS.PAO.UNOPSBusiness.Managers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Seed;
using UNOPS.PAO.Utilities.Interfaces;


public class UNOPSSystemAdminManager : ISystemAdminManager
{
    private readonly UNOPSAppDbContext unopsAppDbContext;
    private readonly IConfiguration configuration;
    private readonly IServiceProvider serviceProvider;

    public UNOPSSystemAdminManager(UNOPSAppDbContext unopsAppDbContext, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        this.unopsAppDbContext = unopsAppDbContext;
        this.configuration = configuration;
        this.serviceProvider = serviceProvider;
    }

    public async Task RunMigrations()
    {
        await unopsAppDbContext.Database.MigrateAsync();
    }

    public async Task RunSeeding()
    {
        // Execute seeding using the same GenericSeedRunner as startup
        await GenericSeedRunner.ExecuteConfiguredSeedsAsync(unopsAppDbContext, serviceProvider, configuration);
    }

    public async Task RunSpecificSeeder(string seederName)
    {
        // Execute a specific seeder by name
        await GenericSeedRunner.ExecuteSpecificSeederAsync(unopsAppDbContext, serviceProvider, configuration, seederName);
    }

    public async Task TruncateSeedScripts()
    {
        Console.WriteLine("🗑️  Truncating SeedScripts table...");
        await unopsAppDbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE public.\"SeedScripts\" RESTART IDENTITY;");
        Console.WriteLine("✅ SeedScripts table truncated successfully");
    }

    public async Task DeleteSeedScript(string scriptName)
    {
        Console.WriteLine($"🗑️  Deleting seed script: {scriptName}");
        var script = await unopsAppDbContext.SeedScripts.FirstOrDefaultAsync(s => s.ScriptName == scriptName);
        if (script != null)
        {
            unopsAppDbContext.SeedScripts.Remove(script);
            await unopsAppDbContext.SaveChangesAsync();
            Console.WriteLine($"✅ Seed script '{scriptName}' deleted successfully");
        }
        else
        {
            Console.WriteLine($"⚠️  Seed script '{scriptName}' not found");
        }
    }
}