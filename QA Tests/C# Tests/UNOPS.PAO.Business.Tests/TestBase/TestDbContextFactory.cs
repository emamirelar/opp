using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Factory for creating test-friendly AppDbContext instances
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an AppDbContext with in-memory database for testing
    /// </summary>
    public static AppDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? $"TestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return Create(options);
    }

    /// <summary>
    /// Creates an AppDbContext with the provided options
    /// </summary>
    public static AppDbContext Create(DbContextOptions<AppDbContext> options)
    {
        // Create a mock HttpContextAccessor that returns a user with ID 1
        var mockHttpContextAccessor = CreateMockHttpContextAccessor();
        
        // Create UserResolverService with the mock HttpContextAccessor
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);

        // Create mock schema
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        return new AppDbContext(options, userResolverService, mockSchema.Object);
    }

    /// <summary>
    /// Creates DbContextOptions for sharing across contexts (useful for concurrency tests)
    /// </summary>
    public static DbContextOptions<AppDbContext> CreateOptions(string? databaseName = null)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? $"TestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    /// <summary>
    /// Creates a mock HttpContextAccessor that simulates an authenticated user
    /// </summary>
    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor()
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
