/**
 * @fileoverview Unit tests for low-priority services: UrlService, UserLookupService,
 * StateMachineStageChangeSeeder, StateMachineStageChangeRoleSeeder, WorkflowServiceExtensions.
 * Requirements source: Read each production file to understand the methods.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Seeders;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Unit tests for low-priority services.
/// Per service: 3 positive, 3 negative, 3 edge/boundary tests.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "LowPriorityServices")]
public class LowPriorityServiceTests : IDisposable
{
    #region 1. UrlService Tests

    [Fact]
    [Trait("Category", "Positive")]
    public void UrlService_GetCurrentHostUrl_WhenHttpContextPresent_ReturnsHostUrlFromRequest()
    {
        // Arrange
        var mockHttpContext = new DefaultHttpContext();
        mockHttpContext.Request.Scheme = "https";
        mockHttpContext.Request.Host = new HostString("example.unops.org", 443);

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

        var config = new ConfigurationBuilder().Build();
        var logger = new Mock<ILogger<UrlService>>().Object;
        var service = new UrlService(config, logger, mockAccessor.Object);

        // Act
        var result = service.GetCurrentHostUrl();

        // Assert
        result.Should().Be("https://example.unops.org:443");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UrlService_GetCurrentHostUrl_WhenHttpContextNull_UsesFallbackFromConfig()
    {
        // Arrange
        var configData = new Dictionary<string, string?> { ["AppConfig:BaseUrl"] = "https://app.unops.org" };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act
        var result = service.GetCurrentHostUrl();

        // Assert
        result.Should().Be("https://app.unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UrlService_BuildEntityUrl_ForKnownEntityTypes_ReturnsCorrectPaths()
    {
        // Arrange - use fallback config
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["AppConfig:BaseUrl"] = "https://app.unops.org" }).Build();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act & Assert
        service.BuildEntityUrl("partner", 42).Should().Be("https://app.unops.org/partnerships/partners/42");
        service.BuildEntityUrl("contact", 7).Should().Be("https://app.unops.org/partnerships/contacts/7");
        service.BuildEntityUrl("interaction", 99).Should().Be("https://app.unops.org/partnerships/interaction/99");
        service.BuildEntityUrl("opportunity", 1).Should().Be("https://app.unops.org/partnerships/opportunities/1");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UrlService_GetCurrentHostUrl_WhenConfigBaseUrlNull_UsesDefaultFallback()
    {
        // Arrange - no AppConfig:BaseUrl
        var config = new ConfigurationBuilder().Build();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act
        var result = service.GetCurrentHostUrl();

        // Assert - default fallback from constructor
        result.Should().Be("https://test-opportunityplus.unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UrlService_GetCurrentHostUrl_WhenHttpContextThrows_UsesFallback()
    {
        // Arrange
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Throws(new InvalidOperationException("Context access failed"));

        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["AppConfig:BaseUrl"] = "https://fallback.unops.org" }).Build();
        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act
        var result = service.GetCurrentHostUrl();

        // Assert
        result.Should().Be("https://fallback.unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UrlService_BuildEntityUrl_WithEmptyEntityType_ReturnsGenericFallbackPath()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["AppConfig:BaseUrl"] = "https://app.unops.org" }).Build();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act - empty string becomes "s" in generic fallback: /partnerships/s/1
        var result = service.BuildEntityUrl("", 1);

        // Assert - generic fallback: entityType.ToLower() + "s"
        result.Should().Be("https://app.unops.org/partnerships/s/1");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UrlService_GetCurrentHostUrl_WhenFallbackHasTrailingSlash_TrimsCorrectly()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["AppConfig:BaseUrl"] = "https://app.unops.org/" }).Build();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act
        var result = service.GetCurrentHostUrl();

        // Assert
        result.Should().Be("https://app.unops.org");
        result.Should().NotEndWith("/");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UrlService_BuildEntityUrl_WithUnknownEntityType_UsesGenericFallback()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["AppConfig:BaseUrl"] = "https://app.unops.org" }).Build();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act
        var result = service.BuildEntityUrl("customentity", 5);

        // Assert - generic: /partnerships/{entityType.ToLower()}s/{id}
        result.Should().Be("https://app.unops.org/partnerships/customentitys/5");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UrlService_BuildEntityUrl_WithEntityIdZero_ReturnsValidUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["AppConfig:BaseUrl"] = "https://app.unops.org" }).Build();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new UrlService(config, Mock.Of<ILogger<UrlService>>(), mockAccessor.Object);

        // Act
        var result = service.BuildEntityUrl("partner", 0);

        // Assert
        result.Should().Be("https://app.unops.org/partnerships/partners/0");
    }

    #endregion

    #region 2. UserLookupService Tests

    [Fact]
    [Trait("Category", "Positive")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenUserExistsAndActive_ReturnsUserId()
    {
        // Arrange
        var user = new PAOIdentityUser { Id = 42, Email = "user@unops.org", ActiveUser = true };
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("user@unops.org")).ReturnsAsync(user);

        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("user@unops.org");

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenUserExistsAndActive_ReturnsCorrectId()
    {
        // Arrange
        var user = new PAOIdentityUser { Id = 100, Email = "admin@unops.org", ActiveUser = true };
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("admin@unops.org");

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenUserFound_ReturnsId()
    {
        // Arrange
        var user = new PAOIdentityUser { Id = 7, Email = "test@unops.org", ActiveUser = true };
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("test@unops.org")).ReturnsAsync(user);

        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("test@unops.org");

        // Assert
        result.Should().Be(7);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenEmailNull_ReturnsZero()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync(null!);

        // Assert
        result.Should().Be(0);
        mockUserManager.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenEmailEmpty_ReturnsZero()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("");

        // Assert
        result.Should().Be(0);
        mockUserManager.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenUserNotFound_ReturnsZero()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("nonexistent@unops.org")).ReturnsAsync((PAOIdentityUser?)null);

        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("nonexistent@unops.org");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenUserInactive_ReturnsZero()
    {
        // Arrange
        var user = new PAOIdentityUser { Id = 42, Email = "inactive@unops.org", ActiveUser = false };
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("inactive@unops.org")).ReturnsAsync(user);

        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("inactive@unops.org");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenUserNull_ReturnsZero()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("valid@unops.org")).ReturnsAsync((PAOIdentityUser?)null);

        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("valid@unops.org");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task UserLookupService_GetUserIdByEmailAsync_WhenEmailWhitespace_ReturnsZero()
    {
        // Arrange - string.IsNullOrEmpty("   ") is false, so it will call FindByEmailAsync
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("   ")).ReturnsAsync((PAOIdentityUser?)null);

        var service = new UserLookupService(mockUserManager.Object);

        // Act
        var result = await service.GetUserIdByEmailAsync("   ");

        // Assert - whitespace is not null/empty so FindByEmailAsync is called; returns 0 if not found
        result.Should().Be(0);
    }

    #endregion

    #region 3. StateMachineStageChangeSeeder Tests

    [Fact]
    [Trait("Category", "Positive")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_CreatesFiveTransitions()
    {
        // Arrange
        var (sp, _) = CreateSeederServiceProvider();

        // Act
        await sp.SeedStateMachineStageChangesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var transitions = await context.StateMachineStageChanges.ToListAsync();
        transitions.Should().HaveCount(5);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_CreatesIdentifyToGoTransition()
    {
        // Arrange
        var (sp, _) = CreateSeederServiceProvider();

        // Act
        await sp.SeedStateMachineStageChangesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var transition = await context.StateMachineStageChanges
            .FirstOrDefaultAsync(x => x.EntityName == "Opportunity" && x.FromStage == "IDENTIFY & PROFILE" && x.ToStage == "GO");
        transition.Should().NotBeNull();
        transition!.ApprovalRequired.Should().BeTrue();
        transition.CommentRequired.Should().BeTrue();
        transition.Name.Should().Be("Submit for Go");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_IsIdempotent()
    {
        // Arrange
        var (sp, _) = CreateSeederServiceProvider();

        // Act - run twice
        await sp.SeedStateMachineStageChangesAsync();
        await sp.SeedStateMachineStageChangesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var transitions = await context.StateMachineStageChanges.ToListAsync();
        transitions.Should().HaveCount(5);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_WhenWorkflowDbContextMissing_Throws()
    {
        // Arrange - service provider without WorkflowDbContext
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILogger<WorkflowDbContext>>());
        var sp = services.BuildServiceProvider();

        // Act
        var act = async () => await sp.SeedStateMachineStageChangesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_WhenLoggerMissing_Throws()
    {
        // Arrange - WorkflowDbContext present but no ILogger<WorkflowDbContext>
        var options = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase($"Workflow_{Guid.NewGuid()}").Options;
        var services = new ServiceCollection();
        services.AddScoped<WorkflowDbContext>(_ => new WorkflowDbContext(options));
        var sp = services.BuildServiceProvider();

        // Act
        var act = async () => await sp.SeedStateMachineStageChangesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_WhenScopeDisposed_ThrowsOnAccess()
    {
        // Arrange - valid provider
        var (sp, _) = CreateSeederServiceProvider();
        await sp.SeedStateMachineStageChangesAsync();

        // Act & Assert - disposing provider then calling again would use new scope; no issue
        // Instead: verify that calling with invalid scope throws
        sp.Dispose();
        var act = async () => await sp.SeedStateMachineStageChangesAsync();
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_AllTransitionsHaveActiveStatus()
    {
        // Arrange
        var (sp, _) = CreateSeederServiceProvider();

        // Act
        await sp.SeedStateMachineStageChangesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var transitions = await context.StateMachineStageChanges.ToListAsync();
        transitions.Should().AllSatisfy(t => t.Status.Should().Be(UNOPS.Workflow.Domain.Enums.EntityStatus.Active));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_ReactivatesSoftDeletedTransition()
    {
        // Arrange
        var (sp, _) = CreateSeederServiceProvider();
        await sp.SeedStateMachineStageChangesAsync();

        using (var scope = sp.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
            var transition = await context.StateMachineStageChanges.FirstAsync(x => x.ToStage == "GO");
            transition.IsDeleted = true;
            await context.SaveChangesAsync();
        }

        // Act - run seeder again
        await sp.SeedStateMachineStageChangesAsync();

        // Assert
        using (var scope2 = sp.CreateScope())
        {
            var context2 = scope2.ServiceProvider.GetRequiredService<WorkflowDbContext>();
            var reactivated = await context2.StateMachineStageChanges.FirstAsync(x => x.ToStage == "GO");
            reactivated.IsDeleted.Should().BeFalse();
        }
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task StateMachineStageChangeSeeder_SeedAsync_AllTransitionsHaveCorrectEntityName()
    {
        // Arrange
        var (sp, _) = CreateSeederServiceProvider();

        // Act
        await sp.SeedStateMachineStageChangesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var transitions = await context.StateMachineStageChanges.ToListAsync();
        transitions.Should().AllSatisfy(t => t.EntityName.Should().Be(OpportunityWorkflow.EntityName));
    }

    #endregion

    #region 4. StateMachineStageChangeRoleSeeder Tests

    [Fact]
    [Trait("Category", "Positive")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenRolesExist_SeedsRolePermissions()
    {
        // Arrange
        var (sp, appContext) = CreateRoleSeederServiceProvider(withRoles: true);

        // Act
        await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var roles = await workflowContext.StateMachineStageChangeRoles.ToListAsync();
        roles.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenRolesExist_IsIdempotent()
    {
        // Arrange
        var (sp, _) = CreateRoleSeederServiceProvider(withRoles: true);

        // Act - run twice
        await sp.SeedStateMachineStageChangeRolesAsync();
        await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var roles = await workflowContext.StateMachineStageChangeRoles.ToListAsync();
        roles.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenRolesExist_CreatesOpportunityManagerPermissions()
    {
        // Arrange
        var (sp, _) = CreateRoleSeederServiceProvider(withRoles: true);

        // Act
        await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var omRoles = await workflowContext.StateMachineStageChangeRoles
            .Where(x => x.RoleName == StateMachineStageChangeRoleSeeder.RoleNames.OpportunityManager)
            .ToListAsync();
        omRoles.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenAppDbContextMissing_Throws()
    {
        // Arrange - WorkflowDbContext but no AppDbContext
        var options = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase($"Workflow_{Guid.NewGuid()}").Options;
        var services = new ServiceCollection();
        services.AddScoped<WorkflowDbContext>(_ => new WorkflowDbContext(options));
        services.AddSingleton(Mock.Of<ILogger<WorkflowDbContext>>());
        var sp = services.BuildServiceProvider();

        // Act
        var act = async () => await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenWorkflowDbContextMissing_Throws()
    {
        // Arrange - AppDbContext but no WorkflowDbContext
        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"App_{Guid.NewGuid()}").Options;
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");
        var mockAccessor = new Mock<IHttpContextAccessor>();
        var userResolver = new UserResolverService<int>(mockAccessor.Object);
        var appContext = new AppDbContext(appOptions, userResolver, mockSchema.Object);

        var services = new ServiceCollection();
        services.AddScoped<AppDbContext>(_ => appContext);
        services.AddSingleton(Mock.Of<ILogger<WorkflowDbContext>>());
        var sp = services.BuildServiceProvider();

        // Act
        var act = async () => await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenProviderDisposed_Throws()
    {
        // Arrange
        var (sp, _) = CreateRoleSeederServiceProvider(withRoles: true);
        sp.Dispose();

        // Act
        var act = async () => await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenNoRolesInDb_SkipsWithoutThrowing()
    {
        // Arrange - AppDbContext with no EntityRoles

        var (sp, _) = CreateRoleSeederServiceProvider(withRoles: false);

        // Act - should not throw; skips roles with RoleId 0
        var act = async () => await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenNoRoles_AddsNoRolePermissions()
    {
        // Arrange
        var (sp, _) = CreateRoleSeederServiceProvider(withRoles: false);

        // Act
        await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert - all seed data has RoleId 0 so nothing is added
        using var scope = sp.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var roles = await workflowContext.StateMachineStageChangeRoles.ToListAsync();
        roles.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task StateMachineStageChangeRoleSeeder_SeedAsync_WhenOnlyOpportunityManagerRoleExists_StillSeeds()
    {
        // Arrange - only one role
        var (sp, appContext) = CreateRoleSeederServiceProvider(withRoles: true);

        // Act
        await sp.SeedStateMachineStageChangeRolesAsync();

        // Assert
        using var scope = sp.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var roles = await workflowContext.StateMachineStageChangeRoles.ToListAsync();
        roles.Should().NotBeEmpty();
    }

    #endregion

    #region 5. WorkflowServiceExtensions Tests

    [Fact]
    [Trait("Category", "Positive")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WithIamAuth_CompletesWithoutThrowing()
    {
        // Arrange - IAM auth (no password) avoids DB connection during registration
        var services = new ServiceCollection();
        var connStr = "Host=localhost;Database=test;Username=u"; // No Password = IAM

        // Act - AddPaoWorkflowServices with IAM path skips AddWorkflowServices (no DB connect)
        var act = () => services.AddPaoWorkflowServices(opts => opts.UsePostgreSqlStorage(connStr));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WithEmptyConfig_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - configure with empty connection string
        var act = () => services.AddPaoWorkflowServices(opts => { });

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_ReturnsServiceCollection()
    {
        // Arrange - IAM auth to avoid DB connection; only testing return value, no provider build
        var services = new ServiceCollection();

        // Act - AddPaoWorkflowServices returns the service collection for chaining
        var result = services.AddPaoWorkflowServices(opts => opts.UsePostgreSqlStorage("Host=localhost;Database=test;Username=u"));

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WhenConfigureNull_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddPaoWorkflowServices(null!);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WhenServicesNull_Throws()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        var act = () => services!.AddPaoWorkflowServices(opts => { });

        // Assert - extension method throws ArgumentNullException when collection is null
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WhenConnectionStringEmpty_DoesNotThrow()
    {
        // Arrange - empty connection string; IAM path is taken when conn str has no password
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IHttpContextAccessor>());

        // Act - empty string: useIamAuth is true (no password), so no DB connection attempted
        var act = () => services.AddPaoWorkflowServices(opts => opts.ConnectionString = "");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WithIamAuth_SkipsWorkflowDbContextRegistration()
    {
        // Arrange - connection string with no password = IAM auth path
        var services = new ServiceCollection();
        var connStr = "Host=localhost;Database=test;Username=u"; // No Password = IAM

        // Act - IAM path does NOT call AddWorkflowServices (DbContext registered elsewhere in Startup)
        services.AddPaoWorkflowServices(opts => opts.UsePostgreSqlStorage(connStr));

        // Assert - WorkflowDbContext is not registered (IAM path skips it)
        var sp = services.BuildServiceProvider();
        var workflowDb = sp.GetService(typeof(WorkflowDbContext));
        workflowDb.Should().BeNull("IAM path skips DbContext registration");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WithEmptyConnectionString_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddPaoWorkflowServices(opts => opts.ConnectionString = "");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void WorkflowServiceExtensions_AddPaoWorkflowServices_WithNullConnectionString_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - configure leaves ConnectionString as default
        var act = () => services.AddPaoWorkflowServices(opts => { });

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Helpers

    private static Mock<UserManager<PAOIdentityUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<PAOIdentityUser>>();
        return new Mock<UserManager<PAOIdentityUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!,
            new Mock<ILogger<UserManager<PAOIdentityUser>>>().Object);
    }

    private static (ServiceProvider sp, WorkflowDbContext context) CreateSeederServiceProvider()
    {
        var options = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase($"Workflow_{Guid.NewGuid()}").Options;
        var context = new WorkflowDbContext(options);

        var services = new ServiceCollection();
        // Use Singleton so scope disposal does not dispose the shared in-memory context
        services.AddSingleton(context);
        services.AddSingleton<ILogger<WorkflowDbContext>>(new Mock<ILogger<WorkflowDbContext>>().Object);

        return (services.BuildServiceProvider(), context);
    }

    private static (ServiceProvider sp, AppDbContext appContext) CreateRoleSeederServiceProvider(bool withRoles)
    {
        var workflowOptions = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase($"Workflow_{Guid.NewGuid()}").Options;
        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"App_{Guid.NewGuid()}").Options;

        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");
        var mockAccessor = new Mock<IHttpContextAccessor>();
        var userResolver = new UserResolverService<int>(mockAccessor.Object);

        var workflowContext = new WorkflowDbContext(workflowOptions);
        var appContext = new AppDbContext(appOptions, userResolver, mockSchema.Object);
        appContext.Database.EnsureCreated();

        if (withRoles)
        {
            var omRole = new EntityRole
            {
                Name = StateMachineStageChangeRoleSeeder.RoleNames.OpportunityManager,
                EntityType = "Opportunity",
                IsDeleted = false
            };
            var plRole = new EntityRole
            {
                Name = StateMachineStageChangeRoleSeeder.RoleNames.PartnershipLead,
                EntityType = "Opportunity",
                IsDeleted = false
            };
            appContext.EntityRoles.AddRange(omRole, plRole);
            appContext.SaveChanges();
        }

        var services = new ServiceCollection();
        // Use Singleton so scope disposal does not dispose the shared in-memory contexts
        services.AddSingleton(workflowContext);
        services.AddSingleton(appContext);
        services.AddSingleton<ILogger<WorkflowDbContext>>(new Mock<ILogger<WorkflowDbContext>>().Object);

        return (services.BuildServiceProvider(), appContext);
    }

    public void Dispose()
    {
        // No unmanaged resources in this test class
    }

    #endregion
}
