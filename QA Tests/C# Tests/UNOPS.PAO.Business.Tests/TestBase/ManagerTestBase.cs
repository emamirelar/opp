using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for all manager unit tests providing common setup and utilities.
/// Provides helpers for creating prerequisite entities (e.g., partner for FK constraints).
/// </summary>
public abstract class ManagerTestBase : IDisposable
{
    protected AppDbContext Context { get; private set; }
    protected Mock<IMapper> MockMapper { get; private set; }
    protected IMapper Mapper => MockMapper.Object;

    /// <summary>Tracks entity IDs for cleanup on PostgreSQL.</summary>
    private readonly List<Func<Task>> _cleanupActions = new();

    protected ManagerTestBase()
    {
        Context = TestDbContextFactory.Create();
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
    /// Register a cleanup action to run on Dispose (removes test data from PostgreSQL).
    /// </summary>
    protected void RegisterCleanup(Func<Task> cleanupAction)
    {
        _cleanupActions.Add(cleanupAction);
    }

    /// <summary>
    /// Remove test entities by executing raw SQL DELETE.
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
            catch { /* Best-effort cleanup */ }
        });
    }

    /// <summary>
    /// Clear all entities from database
    /// </summary>
    protected void ClearDatabase()
    {
        if (TestEnvironment.UseInMemory)
        {
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
        }
    }

    public void Dispose()
    {
        // Run cleanup actions (child tables first, parent tables last)
        for (int i = _cleanupActions.Count - 1; i >= 0; i--)
        {
            try { _cleanupActions[i]().GetAwaiter().GetResult(); }
            catch { /* Best-effort cleanup */ }
        }
        _cleanupActions.Clear();
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
