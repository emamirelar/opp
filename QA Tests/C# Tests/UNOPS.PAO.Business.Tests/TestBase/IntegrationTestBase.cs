using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for integration tests that test managers against PostgreSQL.
/// Uses UNOPSAppDbContext to match production schema with TPH discriminators.
/// Test data is cleaned up after each test to maintain database isolation.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected AppDbContext Context { get; private set; }
    protected IMapper Mapper { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }

    /// <summary>
    /// Tracks entity IDs added during the test for cleanup.
    /// Key: DbSet accessor lambda, Value: list of entity IDs to remove.
    /// </summary>
    private readonly List<Func<Task>> _cleanupActions = new();

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
    /// Save changes to database
    /// </summary>
    protected async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a test partner in the database and returns its auto-generated ID.
    /// Required as a parent for Contact and other entities with FK constraints.
    /// </summary>
    protected async Task<int> CreateTestPartnerAsync(string name = "Integration Test Partner")
    {
        var partner = new UNOPS.PAO.UNOPSDomain.Entities.UNOPSPartner
        {
            Name = name,
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await Context.SaveChangesAsync();
        RegisterTableCleanup("Partners", $"\"Id\" = {partner.Id}");
        return partner.Id;
    }

    /// <summary>
    /// Register a cleanup action to run on Dispose.
    /// Use this to remove test data from the PostgreSQL database.
    /// </summary>
    protected void RegisterCleanup(Func<Task> cleanupAction)
    {
        _cleanupActions.Add(cleanupAction);
    }

    /// <summary>
    /// Remove test entities by executing raw SQL DELETE.
    /// This is more reliable than EF tracking for cleanup.
    /// </summary>
    protected void RegisterTableCleanup(string tableName, string whereClause)
    {
        _cleanupActions.Add(async () =>
        {
            try
            {
                await Context.Database.ExecuteSqlRawAsync(
                    $"DELETE FROM public.\"{tableName}\" WHERE {whereClause}");
            }
            catch
            {
                // Best-effort cleanup - don't fail the test
            }
        });
    }

    /// <summary>
    /// Clear all entities from database.
    /// For InMemory: drops and recreates the schema.
    /// For PostgreSQL: no-op — real database schema is managed by migrations.
    /// </summary>
    protected void ClearDatabase()
    {
        if (TestEnvironment.UseInMemory)
        {
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
        }
        // PostgreSQL: do NOT call EnsureDeleted/EnsureCreated on a real database
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
        // Run cleanup actions to remove test data from PostgreSQL
        foreach (var cleanup in _cleanupActions)
        {
            try { cleanup().GetAwaiter().GetResult(); }
            catch { /* Best-effort cleanup */ }
        }
        _cleanupActions.Clear();

        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
