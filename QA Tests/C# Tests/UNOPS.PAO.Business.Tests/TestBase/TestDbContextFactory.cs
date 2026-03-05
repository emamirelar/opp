using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Factory for creating test-friendly database context instances.
/// Default: Creates UNOPSAppDbContext (returned as AppDbContext) to ensure
/// proper TPH discriminator handling for PostgreSQL.
/// For SQLite: Uses SqliteTestAppDbContext which removes ExcludeFromMigrations()
/// on Identity tables so EnsureCreated() creates them.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an AppDbContext using the test environment configuration.
    /// Returns a UNOPSAppDbContext (which inherits from AppDbContext) to ensure
    /// proper TPH discriminator column handling with PostgreSQL.
    /// </summary>
    public static AppDbContext Create(string? databaseName = null)
    {
        return CreateUNOPS(databaseName);
    }

    /// <summary>
    /// Creates an AppDbContext with the provided AppDbContext options.
    /// Backward-compatible overload for tests that define their own options
    /// (e.g., custom InMemory databases for unit-level isolation).
    /// NOTE: This respects the caller's options as-is. Tests that need SQLite-aware
    /// context creation should use Create() or CreateUNOPS() instead.
    /// </summary>
    public static AppDbContext Create(DbContextOptions<AppDbContext> options)
    {
        var mockHttpContextAccessor = CreateMockHttpContextAccessor();
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);

        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        var context = new AppDbContext(options, userResolverService, mockSchema.Object);
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext with proper TPH discriminator support.
    /// This is the primary factory method - all integration tests should use this
    /// to match the production database schema.
    /// For SQLite: creates SqliteTestAppDbContext which includes Identity tables.
    /// </summary>
    public static UNOPSAppDbContext CreateUNOPS(string? databaseName = null)
    {
        var options = TestEnvironment.CreateUNOPSDbContextOptions(databaseName);
        return CreateUNOPS(options);
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext with the provided options.
    /// For SQLite mode, uses SqliteTestAppDbContext subclass to ensure
    /// Identity tables (AspNetUsers, AspNetUserRoles) are created by EnsureCreated().
    /// </summary>
    public static UNOPSAppDbContext CreateUNOPS(DbContextOptions<UNOPSAppDbContext> options)
    {
        var mockHttpContextAccessor = CreateMockHttpContextAccessor();
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);

        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        UNOPSAppDbContext context;

        if (TestEnvironment.UseSQLite)
        {
            // SQLite mode: use subclass that re-maps Identity tables without
            // ExcludeFromMigrations() so EnsureCreated() creates them.
            context = new SqliteTestAppDbContext(options, userResolverService, mockSchema.Object);
        }
        else
        {
            context = new UNOPSAppDbContext(options, userResolverService, mockSchema.Object);
        }

        if (TestEnvironment.UseInMemory)
        {
            context.Database.EnsureCreated();
        }

        return context;
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext with custom dependencies (user service, schema).
    /// Use this when tests need specific user claims or HttpContext configurations.
    /// For SQLite mode, returns SqliteTestAppDbContext to handle Identity table creation.
    /// </summary>
    public static UNOPSAppDbContext CreateUNOPS(
        DbContextOptions<UNOPSAppDbContext> options,
        UserResolverService<int> userResolverService,
        IDbContextSchema schema)
    {
        UNOPSAppDbContext context;

        if (TestEnvironment.UseSQLite)
        {
            context = new SqliteTestAppDbContext(options, userResolverService, schema);
        }
        else
        {
            context = new UNOPSAppDbContext(options, userResolverService, schema);
        }

        if (TestEnvironment.UseInMemory)
        {
            context.Database.EnsureCreated();
        }

        return context;
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext backed by SQLite in-memory, ignoring TestEnvironment.UsePostgreSQL.
    /// Used as a fallback when PostgreSQL is configured but unreachable (Cloud SQL IAM auth failure, proxy down).
    /// </summary>
    public static AppDbContext CreateFallbackSqlite()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = OFF;";
        cmd.ExecuteNonQuery();

        var builder = new DbContextOptionsBuilder<UNOPSAppDbContext>();
        builder.UseSqlite(connection);
        var options = builder.Options;

        var mockAccessor = CreateMockHttpContextAccessor();
        var userResolver = new UserResolverService<int>(mockAccessor.Object);
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        var context = new SqliteTestAppDbContext(options, userResolver, mockSchema.Object);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates DbContextOptions for AppDbContext (backward compatibility).
    /// </summary>
    public static DbContextOptions<AppDbContext> CreateOptions(string? databaseName = null)
    {
        return TestEnvironment.CreateAppDbContextOptions(databaseName);
    }

    /// <summary>
    /// Creates DbContextOptions for UNOPSAppDbContext.
    /// </summary>
    public static DbContextOptions<UNOPSAppDbContext> CreateUNOPSOptions(string? databaseName = null)
    {
        return TestEnvironment.CreateUNOPSDbContextOptions(databaseName);
    }

    /// <summary>
    /// Creates an AppDbContext with the specified user ID baked into the claims.
    /// Convenience wrapper that returns AppDbContext type for backward compatibility.
    /// </summary>
    public static AppDbContext CreateWithUserId(int userId, string? databaseName = null)
    {
        return CreateUNOPSWithUserId(userId, databaseName);
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext with the specified user ID baked into the claims.
    /// Use this when you need the AuditableDbContext to cache a specific user ID.
    /// </summary>
    public static UNOPSAppDbContext CreateUNOPSWithUserId(int userId, string? databaseName = null)
    {
        var options = TestEnvironment.CreateUNOPSDbContextOptions(databaseName);
        return CreateUNOPSWithUserId(options, userId);
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext with the specified user ID baked into the claims.
    /// </summary>
    public static UNOPSAppDbContext CreateUNOPSWithUserId(DbContextOptions<UNOPSAppDbContext> options, int userId)
    {
        var mockAccessor = CreateMockHttpContextAccessor(userId.ToString());
        var userResolver = new UserResolverService<int>(mockAccessor.Object, null);
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        return CreateUNOPS(options, userResolver, mockSchema.Object);
    }

    /// <summary>
    /// Creates a mock HttpContextAccessor that simulates an authenticated user.
    /// </summary>
    internal static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string userId = "1")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = claimsPrincipal;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        return mockHttpContextAccessor;
    }
}

/// <summary>
/// Test-specific subclass of UNOPSAppDbContext for SQLite compatibility.
/// Re-maps Identity tables (AspNetUsers, AspNetUserRoles) WITHOUT ExcludeFromMigrations()
/// so that EnsureCreated() creates them in the SQLite in-memory database.
/// In production, these tables are managed by the separate PAOIdentityDbContext.
/// </summary>
internal sealed class SqliteTestAppDbContext : UNOPSAppDbContext
{
    public SqliteTestAppDbContext(
        DbContextOptions<UNOPSAppDbContext> options,
        UserResolverService<int> userService,
        IDbContextSchema schema)
        : base(options, userService, schema)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQLite does not support schemas. Remove the default "public" schema
        // set by AppDbContext.OnModelCreating() to avoid table-name mismatches.
        modelBuilder.HasDefaultSchema(null);

        // EnsureCreated() respects ExcludeFromMigrations and skips excluded tables.
        // The base context marks Identity tables with ExcludeFromMigrations(true),
        // so we must explicitly set ExcludeFromMigrations(false) to force creation.
        // Simply calling .ToTable("name") without the delegate does NOT clear the flag.
        modelBuilder.Entity<PAOUser>()
            .ToTable("AspNetUsers", t => t.ExcludeFromMigrations(false));

        modelBuilder.Entity<IdentityUserRole<int>>()
            .ToTable("AspNetUserRoles", t => t.ExcludeFromMigrations(false))
            .HasKey(ur => new { ur.UserId, ur.RoleId });
    }
}
