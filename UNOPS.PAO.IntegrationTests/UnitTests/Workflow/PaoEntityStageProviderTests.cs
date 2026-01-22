using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

/// <summary>
/// Unit tests for PaoEntityStageProvider.
/// Tests entity stage retrieval and update operations.
/// </summary>
public class PaoEntityStageProviderTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly PaoEntityStageProvider _stageProvider;

    public PaoEntityStageProviderTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _dbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);

        _stageProvider = new PaoEntityStageProvider(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region GetCurrentStageAsync Tests

    [Fact]
    public async Task GetCurrentStageAsync_WithValidOpportunity_ReturnsStage()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _stageProvider.GetCurrentStageAsync("opportunity", "1");

        // Assert
        result.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task GetCurrentStageAsync_WithDeletedOpportunity_ReturnsNull()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 2,
            Name = "Deleted Opportunity",
            Description = "Test Description",
            Stage = "GO",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _stageProvider.GetCurrentStageAsync("Opportunity", "2");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentStageAsync_WithNonExistentOpportunity_ReturnsNull()
    {
        // Act
        var result = await _stageProvider.GetCurrentStageAsync("Opportunity", "99999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentStageAsync_WithInvalidEntityId_ReturnsNull()
    {
        // Act
        var result = await _stageProvider.GetCurrentStageAsync("Opportunity", "invalid-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentStageAsync_WithUnsupportedEntityType_ReturnsNull()
    {
        // Act
        var result = await _stageProvider.GetCurrentStageAsync("unsupported-entity", "1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentStageAsync_CaseInsensitiveEntityName_ReturnsStage()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 10,
            Name = "Case Test Opportunity",
            Description = "Test Description",
            Stage = "NO GO",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act - Test that provider handles case-insensitive entity names
        var resultLower = await _stageProvider.GetCurrentStageAsync("opportunity", "10");
        var resultUpper = await _stageProvider.GetCurrentStageAsync("OPPORTUNITY", "10");
        var resultMixed = await _stageProvider.GetCurrentStageAsync("Opportunity", "10");

        // Assert - All should return the same result
        resultLower.Should().Be("NO GO");
        resultUpper.Should().Be("NO GO");
        resultMixed.Should().Be("NO GO");
    }

    #endregion

    #region UpdateStageAsync Tests

    [Fact]
    public async Task UpdateStageAsync_WithValidOpportunity_UpdatesStageAndAuditFields()
    {
        // Arrange
        var userId = 123;
        
        // Set up HttpContext with user claim so UserResolverService returns correct userId
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        
        // Create a new DbContext with the user context set up
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        var testDbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);
        var testStageProvider = new PaoEntityStageProvider(testDbContext);
        
        var opportunity = new Opportunity
        {
            Id = 3,
            Name = "Update Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        testDbContext.Opportunities.Add(opportunity);
        await testDbContext.SaveChangesAsync();
        testDbContext.Entry(opportunity).State = EntityState.Detached;

        // Act
        var result = await testStageProvider.UpdateStageAsync("opportunity", "3", "GO", userId);

        // Assert
        result.Should().BeTrue();

        var updatedOpportunity = await testDbContext.Opportunities.FindAsync(3);
        updatedOpportunity!.Stage.Should().Be("GO");
        updatedOpportunity.LastModifiedBy.Should().Be(userId);
        updatedOpportunity.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        testDbContext.Dispose();
    }

    [Fact]
    public async Task UpdateStageAsync_WithDeletedOpportunity_ReturnsFalse()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 4,
            Name = "Deleted Update Test",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _stageProvider.UpdateStageAsync("Opportunity", "4", "GO", 123);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStageAsync_WithNonExistentOpportunity_ReturnsFalse()
    {
        // Act
        var result = await _stageProvider.UpdateStageAsync("Opportunity", "99999", "GO", 123);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStageAsync_WithInvalidEntityId_ReturnsFalse()
    {
        // Act
        var result = await _stageProvider.UpdateStageAsync("opportunity", "not-a-number", "GO", 123);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStageAsync_WithUnsupportedEntityType_ReturnsFalse()
    {
        // Act
        var result = await _stageProvider.UpdateStageAsync("unsupported", "1", "GO", 123);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsEntityValidAsync Tests

    [Fact]
    public async Task IsEntityValidAsync_WithValidOpportunity_ReturnsTrue()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 5,
            Name = "Valid Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _stageProvider.IsEntityValidAsync("Opportunity", "5");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEntityValidAsync_WithDeletedOpportunity_ReturnsFalse()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 6,
            Name = "Deleted Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _stageProvider.IsEntityValidAsync("Opportunity", "6");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEntityValidAsync_WithNonExistentOpportunity_ReturnsFalse()
    {
        // Act
        var result = await _stageProvider.IsEntityValidAsync("opportunity", "99999");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEntityValidAsync_WithInvalidEntityId_ReturnsFalse()
    {
        // Act
        var result = await _stageProvider.IsEntityValidAsync("Opportunity", "invalid");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEntityValidAsync_WithUnsupportedEntityType_ReturnsFalse()
    {
        // Act
        var result = await _stageProvider.IsEntityValidAsync("unsupported", "1");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetEntityDisplayNameAsync Tests

    [Fact]
    public async Task GetEntityDisplayNameAsync_WithValidOpportunity_ReturnsName()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 7,
            Name = "Display Name Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _stageProvider.GetEntityDisplayNameAsync("opportunity", "7");

        // Assert
        result.Should().Be("Display Name Test Opportunity");
    }

    [Fact]
    public async Task GetEntityDisplayNameAsync_WithNonExistentOpportunity_ReturnsDefaultName()
    {
        // Act
        var result = await _stageProvider.GetEntityDisplayNameAsync("Opportunity", "99999");

        // Assert
        result.Should().Be("Unknown Opportunity");
    }

    [Fact]
    public async Task GetEntityDisplayNameAsync_WithInvalidEntityId_ReturnsUnknown()
    {
        // Act
        var result = await _stageProvider.GetEntityDisplayNameAsync("Opportunity", "invalid");

        // Assert
        result.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetEntityDisplayNameAsync_WithUnsupportedEntityType_ReturnsUnknown()
    {
        // Act
        var result = await _stageProvider.GetEntityDisplayNameAsync("unsupported", "1");

        // Assert
        result.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetEntityDisplayNameAsync_WithDeletedOpportunity_StillReturnsName()
    {
        // Arrange - Deleted entities still have names that can be retrieved
        var opportunity = new Opportunity
        {
            Id = 8,
            Name = "Deleted Opportunity Name",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = true
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _stageProvider.GetEntityDisplayNameAsync("opportunity", "8");

        // Assert
        result.Should().Be("Deleted Opportunity Name");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task WorkflowStageTransition_EndToEnd_CompletesSuccessfully()
    {
        // Arrange - Create a new opportunity
        var opportunity = new Opportunity
        {
            Id = 100,
            Name = "End-to-End Test",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();
        _dbContext.Entry(opportunity).State = EntityState.Detached;

        // Act 1 - Verify entity is valid
        var isValid = await _stageProvider.IsEntityValidAsync("Opportunity", "100");
        isValid.Should().BeTrue();

        // Act 2 - Get current stage
        var currentStage = await _stageProvider.GetCurrentStageAsync("opportunity", "100");
        currentStage.Should().Be("IDENTIFY & PROFILE");

        // Act 3 - Get display name
        var displayName = await _stageProvider.GetEntityDisplayNameAsync("Opportunity", "100");
        displayName.Should().Be("End-to-End Test");

        // Act 4 - Update stage to GO
        var updateResult = await _stageProvider.UpdateStageAsync("Opportunity", "100", "GO", 1);
        updateResult.Should().BeTrue();

        // Act 5 - Verify new stage
        var newStage = await _stageProvider.GetCurrentStageAsync("opportunity", "100");
        newStage.Should().Be("GO");
    }

    #endregion
}
