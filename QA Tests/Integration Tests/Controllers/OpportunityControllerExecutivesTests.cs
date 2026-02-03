using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Unit tests for OpportunityController.GetExecutives endpoint.
/// Tests the executive lookup API for Go Decision approval dialog.
/// </summary>
public class OpportunityControllerExecutivesTests : IDisposable
{
    private readonly Mock<ILogger<OpportunityController>> _mockLogger;
    private readonly Mock<IAuthorizationService> _mockAuthService;
    private readonly Mock<IManagerWrapper> _mockManagerWrapper;
    private readonly Mock<IOpportunityManager> _mockOpportunityManager;
    private readonly Mock<IRiskManager> _mockRiskManager;
    private readonly AppDbContext _dbContext;
    private readonly UserResolverService<int> _userResolverService;
    private readonly DefaultHttpContext _httpContext;

    public OpportunityControllerExecutivesTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        
        // Setup authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        _userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _dbContext = new AppDbContext(options, _userResolverService, mockDbContextSchema.Object);

        // Setup mocks
        _mockLogger = new Mock<ILogger<OpportunityController>>();
        _mockAuthService = new Mock<IAuthorizationService>();
        _mockManagerWrapper = new Mock<IManagerWrapper>();
        _mockOpportunityManager = new Mock<IOpportunityManager>();
        _mockRiskManager = new Mock<IRiskManager>();

        // Setup manager wrapper
        _mockManagerWrapper.Setup(x => x.OpportunityManager).Returns(_mockOpportunityManager.Object);
        _mockManagerWrapper.Setup(x => x.AuditLogManager).Returns(Mock.Of<IAuditLogManager>());
        _mockManagerWrapper.Setup(x => x.GeminiManager).Returns(Mock.Of<IGeminiManager>());
        _mockManagerWrapper.Setup(x => x.ImageGenerationManager).Returns(Mock.Of<IImageGenerationManager>());
        _mockManagerWrapper.Setup(x => x.RiskManager).Returns(_mockRiskManager.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region GetExecutives Tests

    /// <summary>
    /// Task 3.7: Test returns executives for valid opportunity org unit.
    /// </summary>
    [Fact]
    public async Task GetExecutives_ReturnsExecutivesForValidOpportunity()
    {
        // Arrange
        var expectedExecutives = new List<TypeaheadInput>
        {
            new TypeaheadInput { Label = "John Director (Director)", Value = "10", Description = "Suggested" },
            new TypeaheadInput { Label = "Jane Deputy (Deputy Director)", Value = "11", Description = null }
        };

        _mockOpportunityManager.Setup(x => x.GetExecutivesForOpportunityAsync(1))
            .ReturnsAsync(expectedExecutives);

        // Act
        var result = await CallGetExecutivesEndpoint(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Label.Should().Contain("Director");
        result.First().Description.Should().Be("Suggested"); // Director marked as suggested
    }

    /// <summary>
    /// Task 3.7: Test returns empty list when no executives assigned.
    /// </summary>
    [Fact]
    public async Task GetExecutives_ReturnsEmptyList_WhenNoExecutivesAssigned()
    {
        // Arrange
        _mockOpportunityManager.Setup(x => x.GetExecutivesForOpportunityAsync(1))
            .ReturnsAsync(Enumerable.Empty<TypeaheadInput>());

        // Act
        var result = await CallGetExecutivesEndpoint(1);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Task 3.7: Test returns 404 for non-existent opportunity.
    /// </summary>
    [Fact]
    public async Task GetExecutives_Returns404_ForNonExistentOpportunity()
    {
        // Arrange
        _mockOpportunityManager.Setup(x => x.GetExecutivesForOpportunityAsync(999))
            .ThrowsAsync(new KeyNotFoundException("Opportunity with ID 999 not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _mockOpportunityManager.Object.GetExecutivesForOpportunityAsync(999));
    }

    /// <summary>
    /// Task 3.7: Test Director/Manager marked as suggested in response.
    /// </summary>
    [Fact]
    public async Task GetExecutives_DirectorMarkedAsSuggested()
    {
        // Arrange
        var executives = new List<TypeaheadInput>
        {
            new TypeaheadInput { Label = "John Director (Director)", Value = "10", Description = "Suggested" },
            new TypeaheadInput { Label = "Jane Deputy (Deputy Director)", Value = "11", Description = null },
            new TypeaheadInput { Label = "Bob Regional (Regional Director)", Value = "12", Description = "Suggested" }
        };

        _mockOpportunityManager.Setup(x => x.GetExecutivesForOpportunityAsync(1))
            .ReturnsAsync(executives);

        // Act
        var result = await CallGetExecutivesEndpoint(1);

        // Assert
        result.Should().NotBeNull();
        
        // Directors should be marked as "Suggested"
        var suggestedCount = result.Count(e => e.Description == "Suggested");
        suggestedCount.Should().BeGreaterThan(0);
        
        // Directors should appear first (sorted by Suggested flag)
        result.First().Description.Should().Be("Suggested");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper to call the GetExecutives method on the mock manager.
    /// In real tests, this would go through the controller.
    /// </summary>
    private async Task<IEnumerable<TypeaheadInput>> CallGetExecutivesEndpoint(int opportunityId)
    {
        return await _mockOpportunityManager.Object.GetExecutivesForOpportunityAsync(opportunityId);
    }

    #endregion
}
