using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for concurrency tests with multi-threaded utilities.
/// For PostgreSQL: all threads share the same real database via TestDbContextFactory.
/// For SQLite: all threads share the same in-memory connection via UNOPS options.
/// </summary>
public abstract class ConcurrencyTestBase : IDisposable
{
    protected DbContextOptions<UNOPSAppDbContext> DbOptions { get; private set; }
    private readonly string _databaseName;

    /// <summary>Tracks cleanup actions for PostgreSQL test data isolation.</summary>
    private readonly List<Func<Task>> _cleanupActions = new();

    protected ConcurrencyTestBase()
    {
        _databaseName = $"ConcurrencyTest_{Guid.NewGuid()}";
        DbOptions = TestDbContextFactory.CreateUNOPSOptions(_databaseName);
    }

    /// <summary>
    /// Create a new context for each thread (important for concurrency).
    /// Uses CreateUNOPS(options) to ensure shared database and SQLite compatibility.
    /// </summary>
    protected AppDbContext CreateContext()
    {
        return TestDbContextFactory.CreateUNOPS(DbOptions);
    }

    /// <summary>
    /// Creates a test partner in the database and returns its auto-generated ID.
    /// </summary>
    protected async Task<int> CreateTestPartnerAsync(string name = "Concurrency Test Partner")
    {
        using var context = CreateContext();
        var partner = new UNOPSPartner
        {
            Name = name,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await context.Partners.AddAsync(partner);
        await context.SaveChangesAsync();
        RegisterTableCleanup("Partners", $"\"Id\" = {partner.Id}");
        return partner.Id;
    }

    protected void RegisterCleanup(Func<Task> cleanupAction) => _cleanupActions.Add(cleanupAction);

    protected void RegisterTableCleanup(string tableName, string whereClause)
    {
        if (!TestEnvironment.UsePostgreSQL) return;

        _cleanupActions.Add(async () =>
        {
            try
            {
                using var ctx = TestDbContextFactory.Create();
                await ctx.Database.ExecuteSqlAsync($"DELETE FROM public.\"{tableName}\" WHERE {whereClause}");
            }
            catch { /* Best-effort cleanup */ }
        });
    }

    /// <summary>
    /// Execute operations concurrently
    /// </summary>
    protected async Task<ConcurrentBag<T>> ExecuteConcurrentlyAsync<T>(
        int threadCount,
        Func<int, Task<T>> operation)
    {
        var results = new ConcurrentBag<T>();
        var tasks = new List<Task>();

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var result = await operation(index);
                results.Add(result);
            }));
        }

        await Task.WhenAll(tasks);
        return results;
    }

    /// <summary>
    /// Execute operations concurrently (void operations)
    /// </summary>
    protected async Task ExecuteConcurrentlyAsync(
        int threadCount,
        Func<int, Task> operation)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () => await operation(index)));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Execute operations concurrently and collect exceptions
    /// </summary>
    protected async Task<(ConcurrentBag<T> Results, ConcurrentBag<Exception> Exceptions)> 
        ExecuteConcurrentlyWithExceptionsAsync<T>(
            int threadCount,
            Func<int, Task<T>> operation)
    {
        var results = new ConcurrentBag<T>();
        var exceptions = new ConcurrentBag<Exception>();
        var tasks = new List<Task>();

        for (int i = 0; i < threadCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var result = await operation(index);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        await Task.WhenAll(tasks);
        return (results, exceptions);
    }

    /// <summary>
    /// Verify no deadlocks by setting timeout
    /// </summary>
    protected async Task<bool> ExecuteWithTimeoutAsync(
        Func<Task> operation,
        int timeoutMs = 5000)
    {
        var task = operation();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMs));
        return completedTask == task;
    }

    /// <summary>
    /// Seed shared data before concurrent tests
    /// </summary>
    protected virtual async Task SeedSharedDataAsync()
    {
        using var context = CreateContext();
        // Override in derived classes
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        // Run cleanup actions for PostgreSQL
        for (int i = _cleanupActions.Count - 1; i >= 0; i--)
        {
            try { _cleanupActions[i]().GetAwaiter().GetResult(); }
            catch { /* Best-effort cleanup */ }
        }
        _cleanupActions.Clear();

        if (TestEnvironment.UseInMemory)
        {
            try
            {
                using var context = CreateContext();
                context.Database.EnsureDeleted();
            }
            catch
            {
                // Best effort cleanup
            }
        }
        GC.SuppressFinalize(this);
    }
}
