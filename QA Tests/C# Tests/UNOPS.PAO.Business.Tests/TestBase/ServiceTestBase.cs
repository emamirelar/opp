using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using AutoMapper;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for all service unit tests providing common setup and utilities.
/// 
/// PostgreSQL isolation: Each test runs inside a database transaction that is
/// rolled back on Dispose, ensuring no test data reaches the shared database.
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected UNOPSAppDbContext Context { get; private set; }
    protected Mock<IMapper> MockMapper { get; private set; }
    protected IMapper Mapper => MockMapper.Object;

    private IDbContextTransaction? _transaction;

    protected ServiceTestBase()
    {
        Context = (UNOPSAppDbContext)TestDbContextFactory.Create();
        MockMapper = new Mock<IMapper>();

        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = Context.Database.BeginTransaction();
        }
    }

    /// <summary>
    /// Save changes to database
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

