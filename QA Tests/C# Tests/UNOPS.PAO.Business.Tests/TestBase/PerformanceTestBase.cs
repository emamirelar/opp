using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for performance tests with timing utilities
/// </summary>
public abstract class PerformanceTestBase : IDisposable
{
    protected AppDbContext Context { get; private set; }
    protected Stopwatch Stopwatch { get; private set; }

    // Performance thresholds (in milliseconds)
    protected const int FastOperationThreshold = 100;
    protected const int NormalOperationThreshold = 500;
    protected const int SlowOperationThreshold = 1000;
    protected const int BulkOperationThreshold = 2000;

    /// <summary>Tracks cleanup actions for PostgreSQL test data isolation.</summary>
    private readonly List<Func<Task>> _cleanupActions = new();

    protected PerformanceTestBase()
    {
        Context = TestDbContextFactory.Create();
        Stopwatch = new Stopwatch();
    }

    /// <summary>
    /// Creates a test partner in the database and returns its auto-generated ID.
    /// </summary>
    protected async Task<int> CreateTestPartnerAsync(string name = "Perf Test Partner")
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

    protected void RegisterCleanup(Func<Task> cleanupAction) => _cleanupActions.Add(cleanupAction);

    protected void RegisterTableCleanup(string tableName, string whereClause)
    {
        _cleanupActions.Add(async () =>
        {
            try { await Context.Database.ExecuteSqlRawAsync($"DELETE FROM public.\"{tableName}\" WHERE {whereClause}"); }
            catch { /* Best-effort cleanup */ }
        });
    }

    /// <summary>
    /// Execute and measure operation time
    /// </summary>
    protected async Task<(T Result, long ElapsedMs)> MeasureAsync<T>(Func<Task<T>> operation)
    {
        Stopwatch.Restart();
        var result = await operation();
        Stopwatch.Stop();
        return (result, Stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Execute and measure operation time (void)
    /// </summary>
    protected async Task<long> MeasureAsync(Func<Task> operation)
    {
        Stopwatch.Restart();
        await operation();
        Stopwatch.Stop();
        return Stopwatch.ElapsedMilliseconds;
    }

    /// <summary>
    /// Execute synchronous operation and measure time
    /// </summary>
    protected (T Result, long ElapsedMs) Measure<T>(Func<T> operation)
    {
        Stopwatch.Restart();
        var result = operation();
        Stopwatch.Stop();
        return (result, Stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Run operation multiple times and get average time
    /// </summary>
    protected async Task<double> MeasureAverageAsync(Func<Task> operation, int iterations = 10)
    {
        var times = new List<long>();
        
        // Warm up
        await operation();
        
        for (int i = 0; i < iterations; i++)
        {
            Stopwatch.Restart();
            await operation();
            Stopwatch.Stop();
            times.Add(Stopwatch.ElapsedMilliseconds);
        }

        return times.Average();
    }

    /// <summary>
    /// Seed large dataset for performance testing
    /// </summary>
    protected virtual async Task SeedLargeDatasetAsync(int count)
    {
        // Override in derived classes
        await Task.CompletedTask;
    }

    protected async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();

    public void Dispose()
    {
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
