using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Base class for performance tests with timing utilities.
/// PostgreSQL isolation via transaction rollback.
/// </summary>
public abstract class PerformanceTestBase : IDisposable
{
    protected UNOPSAppDbContext Context { get; private set; }
    protected Stopwatch Stopwatch { get; private set; }

    /// <summary>
    /// Test user ID used for CreatedBy/LastModifiedBy when seeding entities.
    /// Must exist in AspNetUsers — call EnsureTestUserAsync() before seeding when using PostgreSQL.
    /// </summary>
    protected const int TestUserId = 1;

    // Performance thresholds (in milliseconds, scaled for CI via ScaleThreshold)
    protected static readonly int FastOperationThreshold = ScaleThreshold(100);
    protected static readonly int NormalOperationThreshold = ScaleThreshold(500);
    protected static readonly int SlowOperationThreshold = ScaleThreshold(1000);
    protected static readonly int BulkOperationThreshold = ScaleThreshold(2000);

    /// <summary>
    /// CI/shared environments are typically slower than local dev machines due to constrained
    /// CPU, memory, and I/O. This multiplier is applied to SLA thresholds via ScaleThreshold()
    /// to prevent false failures in CI while keeping strict thresholds locally.
    /// Detected via GITHUB_ACTIONS, CI, or TF_BUILD environment variables.
    /// </summary>
    protected static bool IsCiEnvironment { get; } =
        Environment.GetEnvironmentVariable("CI") != null ||
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != null ||
        Environment.GetEnvironmentVariable("TF_BUILD") != null;

    private static readonly double _ciMultiplier = IsCiEnvironment ? 2.5 : 1.0;

    /// <summary>
    /// Scales a local SLA threshold for the current environment.
    /// Returns the original value locally, 2.5x in CI.
    /// </summary>
    protected static int ScaleThreshold(int localThresholdMs) =>
        (int)(localThresholdMs * _ciMultiplier);

    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Whether the PostgreSQL database is actually reachable.
    /// TestEnvironment.UsePostgreSQL can be true (config exists) but the database
    /// may still be unreachable (Cloud SQL IAM auth failure, proxy not running, etc.).
    /// Tests that require PostgreSQL should check this before proceeding.
    /// </summary>
    protected bool IsPostgresReachable { get; private set; }

    protected PerformanceTestBase()
    {
        Stopwatch = new Stopwatch();

        if (TestEnvironment.UsePostgreSQL)
        {
            try
            {
                Context = (UNOPSAppDbContext)TestDbContextFactory.Create();
                _transaction = Context.Database.BeginTransaction();
                IsPostgresReachable = true;
            }
            catch (Exception ex)
            {
                IsPostgresReachable = false;
                Context = (UNOPSAppDbContext)TestDbContextFactory.CreateFallbackSqlite();
                TestEnvironment.EnsureCleanDatabase(Context);
                Console.WriteLine(
                    $"[QA-102] PostgreSQL unreachable — falling back to SQLite. " +
                    $"Error: {ex.GetType().Name}: {ex.Message}. " +
                    $"Check: (1) Cloud SQL proxy running? (2) gcloud token fresh? (3) IAM user has GRANTs?");
            }
        }
        else
        {
            Context = (UNOPSAppDbContext)TestDbContextFactory.Create();
            IsPostgresReachable = false;
            if (IsCiEnvironment)
            {
                Console.WriteLine(
                    "[QA-102] Running in CI without PostgreSQL — using InMemory/SQLite fallback. " +
                    "Performance thresholds scaled by 2.5x.");
            }
        }
    }

    /// <summary>
    /// Ensures a minimal test user (Id=1) exists in AspNetUsers.
    /// Required before inserting entities with CreatedBy FK (e.g. Opportunities).
    /// No-op for SQLite (FK enforcement disabled).
    /// </summary>
    protected async Task EnsureTestUserAsync()
    {
        if (!TestEnvironment.UsePostgreSQL)
            return;

        await Context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"AspNetUsers\" (\"Id\", \"Email\", \"NormalizedEmail\", \"UserName\", \"NormalizedUserName\", " +
            "\"EmailConfirmed\", \"PasswordHash\", \"SecurityStamp\", \"ConcurrencyStamp\", " +
            "\"PhoneNumberConfirmed\", \"TwoFactorEnabled\", \"LockoutEnabled\", \"AccessFailedCount\", \"IsInternal\") " +
            "SELECT 1, 'perf@test.local', 'PERF@TEST.LOCAL', 'perf@test.local', 'PERF@TEST.LOCAL', " +
            "true, 'x', 'x', 'x', false, false, true, 0, true " +
            "WHERE NOT EXISTS (SELECT 1 FROM \"AspNetUsers\" WHERE \"Id\" = 1)");
    }

    /// <summary>
    /// Creates a test partner in the database and returns its auto-generated ID.
    /// </summary>
    protected async Task<int> CreateTestPartnerAsync(string name = "Perf Test Partner")
    {
        await EnsureTestUserAsync();
        var partner = new UNOPSPartner
        {
            Name = name,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await Context.SaveChangesAsync();
        return partner.Id;
    }

    protected void RegisterCleanup(Func<Task> cleanupAction) { }
    protected void RegisterTableCleanup(string tableName, string whereClause) { }

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
