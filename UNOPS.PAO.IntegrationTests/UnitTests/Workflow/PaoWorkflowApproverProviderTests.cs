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
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Domain.Entities;
using Xunit;
using WorkflowEntityStatus = UNOPS.Workflow.Domain.Enums.EntityStatus;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

/// <summary>
/// Unit tests for PaoWorkflowApproverProvider.
/// Tests approver resolution and permission checks for workflow transitions.
/// </summary>
public class PaoWorkflowApproverProviderTests : IDisposable
{
    private readonly AppDbContext _appDbContext;
    private readonly WorkflowDbContext _workflowDbContext;
    private readonly PaoWorkflowApproverProvider _approverProvider;

    public PaoWorkflowApproverProviderTests()
    {
        // Setup in-memory database for AppDbContext
        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _appDbContext = new AppDbContext(appOptions, userResolverService, mockDbContextSchema.Object);

        // Setup in-memory database for WorkflowDbContext
        var workflowOptions = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _workflowDbContext = new WorkflowDbContext(workflowOptions, "workflow");

        _approverProvider = new PaoWorkflowApproverProvider(_appDbContext, _workflowDbContext);
    }

    public void Dispose()
    {
        _appDbContext.Dispose();
        _workflowDbContext.Dispose();
    }

    #region Test Data Setup

    private async Task SeedTestDataAsync()
    {
        // Create entity role
        var doaHolderRole = new EntityRole
        {
            Id = 1,
            EntityType = "Opportunity", // Required property
            Name = "DOA Holder",
            Code = "DOA_HOLDER",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _appDbContext.EntityRoles.Add(doaHolderRole);

        var opportunityManagerRole = new EntityRole
        {
            Id = 2,
            EntityType = "Opportunity", // Required property
            Name = "Opportunity Manager",
            Code = "OPP_MANAGER",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _appDbContext.EntityRoles.Add(opportunityManagerRole);

        // Create users
        var approverUser = new PAOUser
        {
            Id = 100,
            Email = "approver@test.com",
            IsInternal = true
            // Note: Name is computed from UserProfile, not settable
        };
        _appDbContext.PAOUsers.Add(approverUser);

        var approverUserProfile = new UserProfile
        {
            Id = 100,
            UserId = 100,
            FirstName = "John",
            LastName = "Approver",
            Status = EntityStatus.Active,
            IsDeleted = false
            // Note: Name is a computed property (FirstName + LastName)
        };
        _appDbContext.UserProfile.Add(approverUserProfile);

        var triggerUser = new PAOUser
        {
            Id = 101,
            Email = "trigger@test.com",
            IsInternal = true
            // Note: Name is computed from UserProfile, not settable
        };
        _appDbContext.PAOUsers.Add(triggerUser);

        var triggerUserProfile = new UserProfile
        {
            Id = 101,
            UserId = 101,
            FirstName = "Jane",
            LastName = "Trigger",
            Status = EntityStatus.Active,
            IsDeleted = false
            // Note: Name is a computed property (FirstName + LastName)
        };
        _appDbContext.UserProfile.Add(triggerUserProfile);

        // Create opportunity
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        _appDbContext.Opportunities.Add(opportunity);

        // Create stakeholders (link users to opportunity with roles)
        var approverStakeholder = new OpportunityStakeholder
        {
            Id = 1,
            OpportunityId = 1,
            UserId = 100,
            EntityRoleId = 1, // DOA Holder
            IsInternal = true
            // Note: OpportunityStakeholder does not have Status, Name, or IsDeleted properties
        };
        _appDbContext.Set<OpportunityStakeholder>().Add(approverStakeholder);

        var triggerStakeholder = new OpportunityStakeholder
        {
            Id = 2,
            OpportunityId = 1,
            UserId = 101,
            EntityRoleId = 2, // Opportunity Manager
            IsInternal = true
            // Note: OpportunityStakeholder does not have Status, Name, or IsDeleted properties
        };
        _appDbContext.Set<OpportunityStakeholder>().Add(triggerStakeholder);

        await _appDbContext.SaveChangesAsync();

        // Seed workflow stage change roles
        var approverRoleConfig = new StateMachineStageChangeRole
        {
            Id = 1,
            EntityType = "Opportunity",
            FromStage = "IDENTIFY & PROFILE",
            ToStage = "GO",
            RoleId = 1,
            RoleName = "DOA Holder",
            CanApprove = true,
            CanTrigger = false,
            Status = WorkflowEntityStatus.Active,
            IsDeleted = false,
            Name = "DOA Holder Approval Config"
        };
        _workflowDbContext.StateMachineStageChangeRoles.Add(approverRoleConfig);

        var triggerRoleConfig = new StateMachineStageChangeRole
        {
            Id = 2,
            EntityType = "Opportunity",
            FromStage = "IDENTIFY & PROFILE",
            ToStage = "GO",
            RoleId = 2,
            RoleName = "Opportunity Manager",
            CanApprove = false,
            CanTrigger = true,
            Status = WorkflowEntityStatus.Active,
            IsDeleted = false,
            Name = "Opportunity Manager Trigger Config"
        };
        _workflowDbContext.StateMachineStageChangeRoles.Add(triggerRoleConfig);

        await _workflowDbContext.SaveChangesAsync();
    }

    #endregion

    #region GetApproversAsync Tests

    [Fact]
    public async Task GetApproversAsync_WithConfiguredApprovers_ReturnsApproverList()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _approverProvider.GetApproversAsync(
            "Opportunity", 1, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(100);
        result.First().Role.Should().Be("DOA Holder");
    }

    [Fact]
    public async Task GetApproversAsync_WithNoConfiguredRoles_ReturnsEmptyList()
    {
        // Arrange - No seed data

        // Act
        var result = await _approverProvider.GetApproversAsync(
            "Opportunity", 1, "IDENTIFY & PROFILE", "UNCONFIGURED_STAGE");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApproversAsync_WithUnsupportedEntityType_ReturnsEmptyList()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _approverProvider.GetApproversAsync(
            "unsupported", 1, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetApprovalConfigurationAsync Tests

    [Fact]
    public async Task GetApprovalConfigurationAsync_WithConfiguredApprovers_ReturnsConfiguration()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _approverProvider.GetApprovalConfigurationAsync(
            "Opportunity", 1, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().NotBeNull();
        result!.Value.roles.Should().Contain("DOA Holder");
        result!.Value.approvals.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetApprovalConfigurationAsync_WithNoConfiguredRoles_ReturnsNull()
    {
        // Arrange - No seed data

        // Act
        var result = await _approverProvider.GetApprovalConfigurationAsync(
            "Opportunity", 1, "UNCONFIGURED_FROM", "UNCONFIGURED_TO");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetTriggerConfigurationAsync Tests

    [Fact]
    public async Task GetTriggerConfigurationAsync_WithConfiguredTriggers_ReturnsConfiguration()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _approverProvider.GetTriggerConfigurationAsync(
            "Opportunity", 1, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().NotBeNull();
        result!.Value.roles.Should().Contain("Opportunity Manager");
        result!.Value.triggers.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTriggerConfigurationAsync_WithNoConfiguredRoles_ReturnsNull()
    {
        // Arrange - No seed data

        // Act
        var result = await _approverProvider.GetTriggerConfigurationAsync(
            "Opportunity", 1, "UNCONFIGURED_FROM", "UNCONFIGURED_TO");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CanUserApproveAsync Tests

    [Fact]
    public async Task CanUserApproveAsync_WithAuthorizedApprover_ReturnsTrue()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _approverProvider.CanUserApproveAsync(
            "Opportunity", 1, 100, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserApproveAsync_WithUnauthorizedUser_ReturnsFalse()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act - User 999 is not a stakeholder
        var result = await _approverProvider.CanUserApproveAsync(
            "Opportunity", 1, 999, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUserApproveAsync_WithTriggerUserNotApprover_ReturnsFalse()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act - User 101 has trigger role, not approve role
        var result = await _approverProvider.CanUserApproveAsync(
            "Opportunity", 1, 101, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUserApproveAsync_WithUnconfiguredTransition_ReturnsFalse()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _approverProvider.CanUserApproveAsync(
            "Opportunity", 1, 100, "GO", "UNCONFIGURED_STAGE");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUserApproveAsync_WithDifferentEntityId_ReturnsFalse()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act - Opportunity 999 doesn't have user 100 as stakeholder
        var result = await _approverProvider.CanUserApproveAsync(
            "Opportunity", 999, 100, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task GetApproversAsync_WithDeletedStageChangeRole_ExcludesFromResult()
    {
        // Arrange
        var deletedRoleConfig = new StateMachineStageChangeRole
        {
            Id = 10,
            EntityType = "Opportunity",
            FromStage = "IDENTIFY & PROFILE",
            ToStage = "NO GO",
            RoleId = 1,
            RoleName = "DOA Holder",
            CanApprove = true,
            CanTrigger = false,
            Status = WorkflowEntityStatus.Active,
            IsDeleted = true, // Deleted
            Name = "Deleted Config"
        };
        _workflowDbContext.StateMachineStageChangeRoles.Add(deletedRoleConfig);
        await _workflowDbContext.SaveChangesAsync();

        // Act
        var result = await _approverProvider.GetApproversAsync(
            "Opportunity", 1, "IDENTIFY & PROFILE", "NO GO");

        // Assert - Should not return the deleted config
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetApproversAsync_CaseInsensitiveEntityName_Works()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act - Use different case (test case-insensitive comparison)
        // Note: Seed data uses "Opportunity", but provider should handle case-insensitive matching
        var resultLower = await _approverProvider.GetApproversAsync(
            "opportunity", 1, "IDENTIFY & PROFILE", "GO");
        var resultUpper = await _approverProvider.GetApproversAsync(
            "OPPORTUNITY", 1, "IDENTIFY & PROFILE", "GO");
        var resultMixed = await _approverProvider.GetApproversAsync(
            "Opportunity", 1, "IDENTIFY & PROFILE", "GO");

        // Assert - All should return the same results
        resultLower.Should().HaveCount(resultUpper.Count);
        resultUpper.Should().HaveCount(resultMixed.Count);
    }

    [Fact]
    public async Task GetApproversAsync_WithMultipleApprovers_ReturnsAll()
    {
        // Arrange
        await SeedTestDataAsync();

        // Add another approver for the same transition
        var secondApproverUser = new PAOUser
        {
            Id = 200,
            Email = "approver2@test.com",
            IsInternal = true
            // Note: Name is computed from UserProfile, not settable
        };
        _appDbContext.PAOUsers.Add(secondApproverUser);

        var secondApproverProfile = new UserProfile
        {
            Id = 200,
            UserId = 200,
            FirstName = "Second",
            LastName = "Approver",
            Status = EntityStatus.Active,
            IsDeleted = false
            // Note: Name is a computed property (FirstName + LastName)
        };
        _appDbContext.UserProfile.Add(secondApproverProfile);

        var secondApproverStakeholder = new OpportunityStakeholder
        {
            Id = 10,
            OpportunityId = 1,
            UserId = 200,
            EntityRoleId = 1, // DOA Holder
            IsInternal = true
            // Note: OpportunityStakeholder does not have Status, Name, or IsDeleted properties
        };
        _appDbContext.Set<OpportunityStakeholder>().Add(secondApproverStakeholder);
        await _appDbContext.SaveChangesAsync();

        // Act
        var result = await _approverProvider.GetApproversAsync(
            "Opportunity", 1, "IDENTIFY & PROFILE", "GO");

        // Assert
        result.Should().HaveCount(2);
        result.Select(a => a.UserId).Should().Contain(100);
        result.Select(a => a.UserId).Should().Contain(200);
    }

    #endregion
}
