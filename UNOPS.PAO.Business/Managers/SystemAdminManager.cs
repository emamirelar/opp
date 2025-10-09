namespace UNOPS.PAO.Business.Managers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;


public class SystemAdminManager : ISystemAdminManager
{
    private readonly AppDbContext appDbContext;
    private readonly IConfiguration configuration;
    private readonly IServiceProvider serviceProvider;

    public SystemAdminManager(AppDbContext appDbContext, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        this.appDbContext = appDbContext;
        this.configuration = configuration;
        this.serviceProvider = serviceProvider;
    }

    public async Task RunMigrations()
    {
        await appDbContext.Database.MigrateAsync();
    }

    public async Task RunSeeding()
    {
        // Note: This base class doesn't have seeding logic
        // Override in derived class (UNOPSSystemAdminManager) for actual implementation
        await Task.CompletedTask;
    }

    public async Task RunSpecificSeeder(string seederName)
    {
        // Note: This base class doesn't have seeding logic
        // Override in derived class (UNOPSSystemAdminManager) for actual implementation
        await Task.CompletedTask;
    }

    public async Task TruncateSeedScripts()
    {
        // Note: This base class doesn't have seeding logic
        // Override in derived class (UNOPSSystemAdminManager) for actual implementation
        await Task.CompletedTask;
    }

    public async Task DeleteSeedScript(string scriptName)
    {
        // Note: This base class doesn't have seeding logic
        // Override in derived class (UNOPSSystemAdminManager) for actual implementation
        await Task.CompletedTask;
    }
}