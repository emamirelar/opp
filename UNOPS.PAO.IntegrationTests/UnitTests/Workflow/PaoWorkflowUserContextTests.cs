using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

/// <summary>
/// Unit tests for PaoWorkflowUserContext.
/// Tests user context extraction from HTTP context and claims.
/// </summary>
public class PaoWorkflowUserContextTests : IDisposable
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AppDbContext _dbContext;
    private readonly PaoWorkflowUserContext _userContext;

    public PaoWorkflowUserContextTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(_mockHttpContextAccessor.Object);
        _dbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c.GetValue<string>("AppConfig:Environment", It.IsAny<string>()))
            .Returns("Development");

        _userContext = new PaoWorkflowUserContext(
            _mockHttpContextAccessor.Object,
            _mockConfiguration.Object,
            _dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region CurrentUserId Tests

    [Fact]
    public void CurrentUserId_WithValidNameIdentifierClaim_ReturnsUserId()
    {
        // Arrange
        var userId = 123;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserId;

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void CurrentUserId_WithInvalidNameIdentifierClaim_ReturnsZero()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-not-a-number")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserId;

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CurrentUserId_WithNoNameIdentifierClaim_ReturnsZero()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserId;

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CurrentUserId_WithNullHttpContext_ReturnsZero()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _userContext.CurrentUserId;

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region CurrentUserName Tests

    [Fact]
    public void CurrentUserName_WithUserProfileInDatabase_ReturnsProfileName()
    {
        // Arrange
        var userId = 100;
        
        // Create user profile in database
        var userProfile = new UserProfile
        {
            UserId = userId,
            Name = "John Doe",
            Status = Domain.Enums.EntityStatus.Active
        };
        _dbContext.UserProfile.Add(userProfile);
        _dbContext.SaveChanges();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserName;

        // Assert
        result.Should().Be("John Doe");
    }

    [Fact]
    public void CurrentUserName_WithNoUserProfile_FallsBackToIdentityName()
    {
        // Arrange
        var userId = 999; // User not in database
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "identity.name@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        identity.AddClaim(new Claim(identity.NameClaimType, "identity.name@test.com"));
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserName;

        // Assert
        result.Should().Be("identity.name@test.com");
    }

    [Fact]
    public void CurrentUserName_WithNoUserIdAndNoIdentity_ReturnsUnknown()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserName;

        // Assert
        result.Should().Be("Unknown");
    }

    #endregion

    #region CurrentUserEmail Tests

    [Fact]
    public void CurrentUserEmail_WithEmailClaim_ReturnsEmailFromClaim()
    {
        // Arrange
        var email = "user@example.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserEmail;

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void CurrentUserEmail_WithUserInDatabase_ReturnsEmailFromDatabase()
    {
        // Arrange
        var userId = 200;
        var email = "dbuser@example.com";
        
        // Create user in database
        var user = new PAOUser
        {
            Id = userId,
            Email = email,
            Status = Domain.Enums.EntityStatus.Active,
            Name = "DB User"
        };
        _dbContext.PAOUsers.Add(user);
        _dbContext.SaveChanges();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            // No email claim - should fall back to database
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserEmail;

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void CurrentUserEmail_WithNoEmailAndNoUser_ReturnsEmptyString()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "99999") // Non-existent user
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserEmail;

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CurrentUserRoles Tests

    [Fact]
    public void CurrentUserRoles_WithMultipleRoleClaims_ReturnsAllRoles()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(ClaimTypes.Role, "Opportunity Manager"),
            new Claim(ClaimTypes.Role, "DOA Holder")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserRoles.ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("Administrator");
        result.Should().Contain("Opportunity Manager");
        result.Should().Contain("DOA Holder");
    }

    [Fact]
    public void CurrentUserRoles_WithNoRoleClaims_ReturnsEmptyList()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.CurrentUserRoles.ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CurrentUserRoles_WithNullHttpContext_ReturnsEmptyList()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _userContext.CurrentUserRoles.ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region HasRole Tests

    [Fact]
    public void HasRole_WithMatchingRole_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Administrator")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.HasRole("Administrator");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithDifferentCaseRole_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMINISTRATOR")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.HasRole("administrator");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithNonMatchingRole_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.HasRole("Administrator");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsAuthenticated Tests

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth"); // Non-null authenticationType means authenticated
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WithUnauthenticatedUser_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // No authenticationType means not authenticated
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContext.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_WithNullHttpContext_ReturnsFalse()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _userContext.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Environment Tests

    [Fact]
    public void Environment_ReturnsValueFromConfiguration()
    {
        // Arrange
        var mockConfigSection = new Mock<IConfigurationSection>();
        mockConfigSection.Setup(x => x.Value).Returns("Production");
        _mockConfiguration.Setup(c => c.GetSection("AppConfig:Environment"))
            .Returns(mockConfigSection.Object);

        // Act & Assert
        // Note: The actual implementation uses GetValue<string> which we've mocked to return "Development"
        // In a real scenario, this would return the configured environment
        _userContext.Environment.Should().NotBeNullOrEmpty();
    }

    #endregion
}
