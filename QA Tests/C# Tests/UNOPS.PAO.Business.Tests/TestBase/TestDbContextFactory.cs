using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Factory for creating test-friendly database context instances.
/// Default: Creates UNOPSAppDbContext (returned as AppDbContext) to ensure
/// proper TPH discriminator handling for PostgreSQL.
/// Also supports backward-compatible Create(options) overload for tests
/// that define their own DbContextOptions.
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
    /// </summary>
    public static AppDbContext Create(DbContextOptions<AppDbContext> options)
    {
        var mockHttpContextAccessor = CreateMockHttpContextAccessor();
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);

        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        var context = new AppDbContext(options, userResolverService, mockSchema.Object);

        if (TestEnvironment.UseInMemory)
        {
            context.Database.EnsureCreated();
        }

        return context;
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext with proper TPH discriminator support.
    /// This is the primary factory method - all integration tests should use this
    /// to match the production database schema.
    /// </summary>
    public static UNOPSAppDbContext CreateUNOPS(string? databaseName = null)
    {
        var options = TestEnvironment.CreateUNOPSDbContextOptions(databaseName);
        return CreateUNOPS(options);
    }

    /// <summary>
    /// Creates a UNOPSAppDbContext with the provided options
    /// </summary>
    public static UNOPSAppDbContext CreateUNOPS(DbContextOptions<UNOPSAppDbContext> options)
    {
        var mockHttpContextAccessor = CreateMockHttpContextAccessor();
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);

        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        var context = new UNOPSAppDbContext(options, userResolverService, mockSchema.Object);

        if (TestEnvironment.UseInMemory)
        {
            context.Database.EnsureCreated();
        }

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
    /// Creates a mock HttpContextAccessor that simulates an authenticated user
    /// </summary>
    internal static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);
        mockHttpContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        return mockHttpContextAccessor;
    }
}
