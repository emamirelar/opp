using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for concurrency tests with multi-threaded utilities
/// </summary>
public abstract class ConcurrencyTestBase : IDisposable
{
    protected DbContextOptions<AppDbContext> DbOptions { get; private set; }
    private readonly string _databaseName;

    protected ConcurrencyTestBase()
    {
        _databaseName = $"ConcurrencyTest_{Guid.NewGuid()}";
        DbOptions = TestDbContextFactory.CreateOptions(_databaseName);
    }

    /// <summary>
    /// Create a new context for each thread (important for concurrency)
    /// </summary>
    protected AppDbContext CreateContext()
    {
        return TestDbContextFactory.Create(DbOptions);
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
        using var context = CreateContext();
        context.Database.EnsureDeleted();
        GC.SuppressFinalize(this);
    }
}
