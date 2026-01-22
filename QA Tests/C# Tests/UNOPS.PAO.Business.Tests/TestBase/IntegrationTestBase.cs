using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for integration tests that test managers with real service layers
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected AppDbContext Context { get; private set; }
    protected IMapper Mapper { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();

        Context = TestDbContextFactory.Create();
        services.AddSingleton(Context);

        // Configure AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains("UNOPS.PAO") == true));
        });
        Mapper = mapperConfig.CreateMapper();
        services.AddSingleton(Mapper);

        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Save changes to in-memory database
    /// </summary>
    protected async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Clear all entities from database
    /// </summary>
    protected void ClearDatabase()
    {
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Seed test data for integration tests
    /// </summary>
    protected virtual async Task SeedTestDataAsync()
    {
        // Override in derived classes to seed specific test data
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
