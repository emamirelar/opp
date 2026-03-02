using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for integration tests that test managers against PostgreSQL.
/// Uses UNOPSAppDbContext to match production schema with TPH discriminators.
/// 
/// PostgreSQL isolation: Each test runs inside a database transaction that is
/// rolled back on Dispose. This ensures tests never pollute the shared database
/// and always see a consistent snapshot of existing data.
/// 
/// Two-phase user resolution: A temporary context resolves the real test user ID
/// from the database, then the main context is constructed with this ID baked into
/// the ClaimsPrincipal so that AuditableDbContext caches the correct value.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected UNOPSAppDbContext Context { get; private set; }
    protected IMapper Mapper { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }

    /// <summary>
    /// The ID of the test user in the AspNetUsers table.
    /// Use this instead of hardcoding CreatedBy = 1 in test entities.
    /// </summary>
    protected int TestUserId { get; private set; }

    /// <summary>
    /// A default partner created inside the test transaction for use by tests
    /// that create contacts or other entities requiring a valid PartnerId FK.
    /// Rolled back automatically with the transaction on Dispose.
    /// </summary>
    protected int DefaultTestPartnerId { get; private set; }

    /// <summary>
    /// Transaction used to isolate test data on PostgreSQL.
    /// Rolled back on Dispose so test data never reaches the real database.
    /// </summary>
    private IDbContextTransaction? _transaction;

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();

        // Phase 1: Resolve (or create) the test user using a temporary context.
        // This runs OUTSIDE the test transaction so the user persists across tests.
        if (TestEnvironment.UsePostgreSQL)
        {
            using var tempContext = TestDbContextFactory.CreateUNOPS();
            TestUserId = TestDataHelper.GetOrCreateTestUser(tempContext, "integrationtest@unops.org");
        }
        else
        {
            TestUserId = 1;
        }

        // Phase 2: Create the MAIN context with the correct user ID in claims,
        // so AuditableDbContext caches the right _currentUserId from the start.
        Context = TestDbContextFactory.CreateUNOPSWithUserId(TestUserId);

        // PostgreSQL: begin a transaction for test isolation.
        // All SaveChanges calls within the test write to the DB inside this transaction,
        // visible to subsequent queries on the same connection, but rolled back on Dispose.
        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = Context.Database.BeginTransaction();
        }

        // Phase 3: Create a shared test partner inside the transaction.
        // This partner is available to all tests and gets rolled back on Dispose.
        var testPartner = new UNOPSPartner
        {
            Name = "Default Integration Test Partner",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Partners.Add(testPartner);
        Context.SaveChanges();
        DefaultTestPartnerId = testPartner.Id;

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
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await Context.SaveChangesAsync();
        return partner.Id;
    }

    /// <summary>
    /// Register a cleanup action to run on Dispose.
    /// NOTE: With transaction rollback, explicit cleanup is rarely needed.
    /// Kept for backward compatibility.
    /// </summary>
    protected void RegisterCleanup(Func<Task> cleanupAction)
    {
        // With transaction rollback, cleanup is automatic. No-op for PostgreSQL.
    }

    /// <summary>
    /// Register table cleanup (no-op with transaction rollback pattern).
    /// Kept for backward compatibility — transaction rollback handles cleanup.
    /// </summary>
    protected void RegisterTableCleanup(string tableName, string whereClause)
    {
        // With transaction rollback, all test data is automatically rolled back.
    }

    /// <summary>
    /// Clear all entities from database.
    /// For InMemory/SQLite: drops and recreates the schema.
    /// For PostgreSQL: no-op — transaction rollback provides isolation.
    /// </summary>
    protected void ClearDatabase()
    {
        if (TestEnvironment.UseInMemory)
        {
            try
            {
                Context.Database.EnsureDeleted();
                Context.Database.EnsureCreated();
            }
            catch { /* SQLite connection may already be closed during concurrent test runs */ }
        }
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
        // PostgreSQL: roll back the transaction to discard all test data
        if (_transaction != null)
        {
            try { _transaction.Rollback(); }
            catch { /* Context may already be disposed */ }
            _transaction.Dispose();
            _transaction = null;
        }

        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
