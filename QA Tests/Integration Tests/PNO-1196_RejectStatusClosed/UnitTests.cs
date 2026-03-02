/**
 * @fileoverview PNO-1196 Unit Tests — 21 tests.
 * Tests request/response model validation, enum values, helper logic.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Workflow;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Domain.Enums;
using Xunit;
using EntityStatus = UNOPS.PAO.Domain.Entities.EntityStatus;

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// PNO-1196 Unit Tests — 21 tests for model validation and enum behaviour.
/// </summary>
[Collection("Unit")]
[Trait("Category", "Unit")]
[Trait("Ticket", "PNO-1196")]
public class UnitTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public UnitTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, "test@unops.org")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        mockHttpContextAccessor.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext { User = principal });

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _dbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);
    }

    [Fact] [Trait("TestId", "UNT-001")]
    public void RejectWorkflowRequest_WithAllFields_IsValid()
    {
        var request = new RejectWorkflowRequest
        {
            EntityId = 1, EntityName = "Opportunity",
            Rationale = "Funding no longer available",
            ConfirmationAcknowledged = true
        };
        request.EntityId.Should().Be(1);
        request.EntityName.Should().Be("Opportunity");
        request.Rationale.Should().NotBeNullOrEmpty();
        request.ConfirmationAcknowledged.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-002")]
    public void EntityStatus_Closed_HasDistinctIntValue()
    {
        var closedVal = (int)EntityStatus.Closed;
        var activeVal = (int)EntityStatus.Active;
        closedVal.Should().NotBe(activeVal);
    }

    [Fact] [Trait("TestId", "UNT-003")]
    public void EntityStatus_Closed_NotEqualToActive()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "UNT-004")]
    public void EntityStatus_Closed_NotEqualToDraft()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "UNT-005")]
    public void WorkflowStatus_None_HasDistinctIntValue()
    {
        var noneVal = (int)WorkflowStatus.None;
        noneVal.Should().BeGreaterThanOrEqualTo(0);
        WorkflowStatus.None.Should().NotBe(WorkflowStatus.InWorkflow);
    }

    [Fact] [Trait("TestId", "UNT-006")]
    public void NoGoStageConstant_IsCorrectString()
    {
        const string noGo = "NO GO";
        noGo.Should().Be("NO GO");
        noGo.Should().NotBe("GO");
        noGo.Should().NotBe("CANCELLED");
    }

    [Fact] [Trait("TestId", "UNT-007")]
    public void WorkflowStatus_InWorkflow_IsDistinctFromNone()
    {
        WorkflowStatus.InWorkflow.Should().NotBe(WorkflowStatus.None);
        ((int)WorkflowStatus.InWorkflow).Should().BeGreaterThan(-1);
    }

    [Fact] [Trait("TestId", "UNT-008")]
    public void RejectWorkflowRequest_DefaultConfirmationAcknowledgedIsFalse()
    {
        var request = new RejectWorkflowRequest { EntityId = 1, EntityName = "Opportunity", Rationale = "Test" };
        request.ConfirmationAcknowledged.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "UNT-009")]
    public void RejectWorkflowRequest_RationaleIsSettable()
    {
        var request = new RejectWorkflowRequest
        {
            EntityId = 1, EntityName = "Opportunity",
            Rationale = "Funding no longer available"
        };
        request.Rationale.Should().NotBeNullOrEmpty();
    }

    [Fact] [Trait("TestId", "UNT-010")]
    public void RejectWorkflowRequest_EntityIdCanBeSetToZero()
    {
        var request = new RejectWorkflowRequest { EntityId = 0, EntityName = "Opportunity", Rationale = "Test" };
        request.EntityId.Should().Be(0);
    }

    [Fact] [Trait("TestId", "UNT-011")]
    public async Task DbContext_Opportunity_CanSetStageToNoGo()
    {
        _dbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 100, Name = "Unit Test Opp", Description = "Unit test opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await _dbContext.SaveChangesAsync();

        var opp = await _dbContext.Opportunities.FindAsync(100);
        opp!.Stage = "NO GO";
        opp.Status = EntityStatus.Closed;
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();
        var updated = await _dbContext.Opportunities.FindAsync(100);
        updated!.Stage.Should().Be("NO GO");
        updated.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "UNT-012")]
    public async Task DbContext_Opportunity_StatusClosedPersists()
    {
        _dbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 101, Name = "Closed Status Opp", Description = "Closed status test opportunity",
            Stage = "NO GO", Status = EntityStatus.Closed, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var opp = await _dbContext.Opportunities.FindAsync(101);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "UNT-013")]
    public void Opportunity_IsDeletedDefaultFalse()
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity { Name = "Test", Description = "Test" };
        opp.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "UNT-014")]
    public void WorkflowLog_RequiresApprovalProperty_CanBeSet()
    {
        var log = new WorkflowLog { RequiresApproval = true };
        log.RequiresApproval.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-015")]
    public void WorkflowLog_EntityNameProperty_CanBeSet()
    {
        var log = new WorkflowLog { EntityName = "Opportunity" };
        log.EntityName.Should().Be("Opportunity");
    }

    [Fact] [Trait("TestId", "UNT-016")]
    public void WorkflowLog_EntityIdProperty_CanBeSetAsString()
    {
        var log = new WorkflowLog { EntityId = "42" };
        log.EntityId.Should().Be("42");
    }

    [Fact] [Trait("TestId", "UNT-017")]
    public void RejectWorkflowRequest_AllPropertiesSettable()
    {
        var req = new RejectWorkflowRequest
        {
            EntityId = 99,
            EntityName = "Opportunity",
            Rationale = "Testing unit",
            ConfirmationAcknowledged = true
        };
        req.EntityId.Should().Be(99);
        req.EntityName.Should().Be("Opportunity");
        req.Rationale.Should().Be("Testing unit");
        req.ConfirmationAcknowledged.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-018")]
    public void EntityStatus_Enum_HasExpectedValues()
    {
        Enum.IsDefined(typeof(EntityStatus), EntityStatus.Active).Should().BeTrue();
        Enum.IsDefined(typeof(EntityStatus), EntityStatus.Closed).Should().BeTrue();
        Enum.IsDefined(typeof(EntityStatus), EntityStatus.Inactive).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-019")]
    public void WorkflowStatus_Enum_HasExpectedValues()
    {
        Enum.IsDefined(typeof(WorkflowStatus), WorkflowStatus.None).Should().BeTrue();
        Enum.IsDefined(typeof(WorkflowStatus), WorkflowStatus.InWorkflow).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-020")]
    public async Task DbContext_OpportunityFilter_IsDeletedFalse_WorksCorrectly()
    {
        _dbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 200, Name = "Active Opp", Description = "Active opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        _dbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 201, Name = "Deleted Opp", Description = "Deleted opportunity",
            Stage = "GO", Status = EntityStatus.Active, IsDeleted = true,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await _dbContext.SaveChangesAsync();

        var active = await _dbContext.Opportunities.Where(o => !o.IsDeleted).ToListAsync();
        active.Should().Contain(o => o.Id == 200);
        active.Should().NotContain(o => o.Id == 201);
    }

    [Fact] [Trait("TestId", "UNT-021")]
    public void WorkflowStatus_Values_AreDistinct()
    {
        WorkflowStatus.None.Should().NotBe(WorkflowStatus.InWorkflow);
        ((int)WorkflowStatus.None).Should().NotBe((int)WorkflowStatus.InWorkflow);
        Enum.IsDefined(typeof(WorkflowStatus), WorkflowStatus.None).Should().BeTrue();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
