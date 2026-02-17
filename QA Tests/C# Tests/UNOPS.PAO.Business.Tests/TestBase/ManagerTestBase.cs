using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using AutoMapper;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for all manager unit tests providing common setup and utilities.
/// Provides helpers for creating prerequisite entities (e.g., partner for FK constraints).
/// 
/// PostgreSQL isolation: Each test runs inside a database transaction that is
/// rolled back on Dispose, ensuring no test data reaches the shared database.
/// Uses two-phase user resolution to ensure AuditableDbContext caches a valid user ID.
/// </summary>
public abstract class ManagerTestBase : IDisposable
{
    protected UNOPSAppDbContext Context { get; private set; }
    protected Mock<IMapper> MockMapper { get; private set; }
    protected IMapper Mapper => MockMapper.Object;
    protected int TestUserId { get; private set; }

    private IDbContextTransaction? _transaction;

    protected ManagerTestBase()
    {
        if (TestEnvironment.UsePostgreSQL)
        {
            // Phase 1: Resolve (or create) the test user using a temporary context
            using var tempContext = TestDbContextFactory.CreateUNOPS();
            TestUserId = TestDataHelper.GetOrCreateTestUser(tempContext, "managertest@unops.org");

            // Phase 2: Create main context with correct user ID in claims
            Context = TestDbContextFactory.CreateUNOPSWithUserId(TestUserId);
            _transaction = Context.Database.BeginTransaction();
        }
        else
        {
            TestUserId = 1;
            Context = (UNOPSAppDbContext)TestDbContextFactory.Create();
        }

        MockMapper = new Mock<IMapper>();
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
    protected async Task<int> CreateTestPartnerAsync(string name = "Test Partner")
    {
        var partner = new UNOPSPartner
        {
            Name = name,
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await Context.SaveChangesAsync();
        return partner.Id;
    }

    /// <summary>
    /// Register a cleanup action (no-op with transaction rollback).
    /// </summary>
    protected void RegisterCleanup(Func<Task> cleanupAction) { }

    /// <summary>
    /// Register table cleanup (no-op with transaction rollback).
    /// </summary>
    protected void RegisterTableCleanup(string tableName, string whereClause) { }

    /// <summary>
    /// Clear all entities from database
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

    public void Dispose()
    {
        if (_transaction != null)
        {
            try { _transaction.Rollback(); }
            catch { }
            _transaction.Dispose();
            _transaction = null;
        }
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
