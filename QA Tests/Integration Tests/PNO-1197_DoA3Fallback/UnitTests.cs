/**
 * @fileoverview PNO-1197 Unit Tests: DoA Level 3 Fallback in Submit Validation.
 * Tests DoA model validation, submit request model, and DoA logic validation.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using UNOPS.Workflow.Models.Requirements;
using Xunit;
using Facing = UNOPS.Workflow.Models.Facing;

namespace UNOPS.PAO.IntegrationTests.PNO1197;

/// <summary>
/// PNO-1197 Unit tests: DoA Level 3 Fallback in Submit Validation.
/// Tests DoA model validation, submit request model, and DoA logic.
/// </summary>
[Collection("Unit")]
[Trait("Category", "Unit")]
[Trait("Type", "Unit")]
public class UnitTests : PNO1197TestFixtureBase
{
    private static WorkflowSubmitRequest CreateValidSubmitRequest(int entityId = 1) => new()
    {
        EntityName = "opportunity",
        EntityId = entityId,
        NewStage = "GO",
        ConfirmedNonOMSubmission = false,
        ConfirmedOrgUnitWarning = true,
        AcknowledgedStatement = true
    };

    #region UNIT_001-007: DoA Model Validation

    [Fact]
    public void UNIT_001_EntityUserRole_RequiredFields_Present()
    {
        var entityRole = new EntityRole
        {
            Id = 1,
            Name = "DoA2",
            Code = "DoA2_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var eur = new EntityUserRole
        {
            UserId = 1,
            EntityRoleId = 1,
            EntityRole = entityRole,
            EntityId = 1,
            EntityType = "OrganizationHierarchy",
            Name = "DoA2 Holder",
            IsDeleted = false
        };

        eur.UserId.Should().Be(1);
        eur.EntityRoleId.Should().Be(1);
        eur.EntityId.Should().Be(1);
        eur.EntityType.Should().Be("OrganizationHierarchy");
        eur.EntityRole.Should().NotBeNull();
    }

    [Fact]
    public void UNIT_002_EntityRole_RequiredFields_Present()
    {
        var entityRole = new EntityRole
        {
            Id = 1,
            Name = "DoA Level 2 Holder",
            Code = "DoA2_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        entityRole.Id.Should().Be(1);
        entityRole.Name.Should().Be("DoA Level 2 Holder");
        entityRole.Code.Should().Be("DoA2_OrganizationHierarchy");
        entityRole.EntityType.Should().Be("OrganizationHierarchy");
    }

    [Fact]
    public void UNIT_003_EntityRole_CodeProperty_Accessible()
    {
        var entityRole = new EntityRole
        {
            Id = 1,
            Name = "DoA3",
            Code = "DoA3_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        entityRole.Code.Should().Be("DoA3_OrganizationHierarchy");
        entityRole.Code.Should().Contain("DoA3");
    }

    [Fact]
    public void UNIT_004_EntityUserRole_EntityTypeProperty_Accessible()
    {
        var eur = new EntityUserRole
        {
            UserId = 1,
            EntityRoleId = 1,
            EntityId = 1,
            EntityType = "OrganizationHierarchy",
            Name = "Test",
            IsDeleted = false
        };

        eur.EntityType.Should().Be("OrganizationHierarchy");
    }

    [Fact]
    public async Task UNIT_005_EntityRole_IsDeletedAffectsQuery()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var activeCount = await DbContext.EntityUserRoles
            .CountAsync(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1 && !eur.IsDeleted);
        activeCount.Should().BeGreaterThan(0);

        var deleted = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1)
            .FirstOrDefaultAsync();
        if (deleted != null)
        {
            deleted.IsDeleted = true;
            await DbContext.SaveChangesAsync();
            var afterDelete = await DbContext.EntityUserRoles
                .CountAsync(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1 && !eur.IsDeleted);
            afterDelete.Should().BeLessThan(activeCount);
        }
    }

    [Fact]
    public void UNIT_006_EntityUserRole_NavigationProperties_Accessible()
    {
        var entityRole = new EntityRole
        {
            Id = 1,
            Name = "DoA2",
            Code = "DoA2_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var eur = new EntityUserRole
        {
            UserId = 1,
            EntityRoleId = 1,
            EntityRole = entityRole,
            EntityId = 1,
            EntityType = "OrganizationHierarchy",
            Name = "Holder",
            IsDeleted = false
        };

        eur.EntityRole.Should().NotBeNull();
        eur.EntityRole!.Code.Should().Be("DoA2_OrganizationHierarchy");
    }

    [Fact]
    public void UNIT_007_EntityRole_CodeFormatValidation()
    {
        var doa2 = new EntityRole
        {
            Id = 1,
            Name = "DoA2",
            Code = "DoA2_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var doa3 = new EntityRole
        {
            Id = 2,
            Name = "DoA3",
            Code = "DoA3_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        doa2.Code.Should().Contain("DoA2");
        doa3.Code.Should().Contain("DoA3");
    }

    #endregion

    #region UNIT_008-014: Submit Request Model

    [Fact]
    public void UNIT_008_WorkflowSubmitRequest_RequiredFields_Present()
    {
        var request = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO"
        };

        request.EntityName.Should().Be("opportunity");
        request.EntityId.Should().Be(1);
        request.NewStage.Should().Be("GO");
    }

    [Fact]
    public void UNIT_009_WorkflowSubmitRequest_NewStageGoForSubmit()
    {
        var request = CreateValidSubmitRequest();
        request.NewStage.Should().Be("GO");
    }

    [Fact]
    public void UNIT_010_WorkflowSubmitRequest_EntityNameOpportunityRequired()
    {
        var request = CreateValidSubmitRequest();
        request.EntityName.Should().Be("opportunity");
    }

    [Fact]
    public void UNIT_011_WorkflowSubmitRequest_EntityIdPositiveRequired()
    {
        var request = CreateValidSubmitRequest(42);
        request.EntityId.Should().Be(42);
        request.EntityId.Should().BePositive();
    }

    [Fact]
    public void UNIT_012_WorkflowSubmitRequest_ConfirmedNonOMSubmissionFlag()
    {
        var request = CreateValidSubmitRequest();
        request.ConfirmedNonOMSubmission.Should().BeFalse();

        request.ConfirmedNonOMSubmission = true;
        request.ConfirmedNonOMSubmission.Should().BeTrue();
    }

    [Fact]
    public void UNIT_013_WorkflowSubmitRequest_ConfirmedOrgUnitWarningFlag()
    {
        var request = CreateValidSubmitRequest();
        request.ConfirmedOrgUnitWarning.Should().BeTrue();

        request.ConfirmedOrgUnitWarning = false;
        request.ConfirmedOrgUnitWarning.Should().BeFalse();
    }

    [Fact]
    public void UNIT_014_WorkflowSubmitRequest_AcknowledgedStatementFlag()
    {
        var request = CreateValidSubmitRequest();
        request.AcknowledgedStatement.Should().BeTrue();

        request.AcknowledgedStatement = false;
        request.AcknowledgedStatement.Should().BeFalse();
    }

    #endregion

    #region UNIT_015-021: DoA Logic Validation

    [Fact]
    public async Task UNIT_015_DoA2CodeDetection_Works()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        // Must have OM stakeholder - requirement 18 in ValidateOpportunityRequirementsAsync
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        SetupStandardSubmitMocks();

        var result = await Controller.Submit(CreateValidSubmitRequest());

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue("DoA2 holder should satisfy requirement");
    }

    [Fact]
    public async Task UNIT_016_DoA3CodeDetection_Works()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        // Must have OM stakeholder - requirement 18 in ValidateOpportunityRequirementsAsync
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        SetupStandardSubmitMocks();

        var result = await Controller.Submit(CreateValidSubmitRequest());

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue("DoA3 fallback should satisfy requirement");
    }

    [Fact]
    public async Task UNIT_017_DoACodeCaseSensitivity_Verified()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var entityRole = await DbContext.EntityRoles.FirstOrDefaultAsync(r => r.Code == "DoA2_OrganizationHierarchy");
        entityRole.Should().NotBeNull();
        entityRole!.Code.Should().Be("DoA2_OrganizationHierarchy");
        entityRole.Code.Should().NotBe("doa2_organizationhierarchy");
    }

    [Fact]
    public async Task UNIT_018_DoACodePartialMatch_Verified()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var doaRoles = await DbContext.EntityRoles
            .Where(r => r.Code != null && (r.Code.Contains("DoA2") || r.Code.Contains("DoA3")))
            .ToListAsync();
        doaRoles.Should().NotBeEmpty();
        doaRoles.Should().Contain(r => r.Code!.Contains("DoA2") || r.Code.Contains("DoA3"));
    }

    [Fact]
    public async Task UNIT_019_DoACodeWithPrefixSuffix_Verified()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var doa2Role = await DbContext.EntityRoles.FirstOrDefaultAsync(r => r.Code == "DoA2_OrganizationHierarchy");
        doa2Role.Should().NotBeNull();
        doa2Role!.Code.Should().StartWith("DoA");
        doa2Role.Code.Should().EndWith("OrganizationHierarchy");
    }

    [Fact]
    public async Task UNIT_020_OrgUnitMatchingLogic_Verified()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp.Should().NotBeNull();
        opp!.ResponsibleOrgUnitId.Should().Be(1);

        var doaHolders = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == opp.ResponsibleOrgUnitId && !eur.IsDeleted)
            .ToListAsync();
        doaHolders.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UNIT_021_EntityTypeMatchingLogic_Verified()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var orgHierarchyHolders = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && !eur.IsDeleted)
            .ToListAsync();
        orgHierarchyHolders.Should().NotBeEmpty();
        orgHierarchyHolders.Should().OnlyContain(eur => eur.EntityType == "OrganizationHierarchy");
    }

    #endregion
}
