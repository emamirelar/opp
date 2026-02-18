using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using UNOPS.PAO.DataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for all manager unit tests providing common setup and utilities
/// </summary>
public abstract class ManagerTestBase : IDisposable
{
    protected AppDbContext Context { get; private set; }
    protected Mock<IMapper> MockMapper { get; private set; }
    protected IMapper Mapper => MockMapper.Object;

    protected ManagerTestBase()
    {
        Context = TestDbContextFactory.Create();
        MockMapper = new Mock<IMapper>();
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

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
