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

[Collection("Boundary")]
[Trait("Category", "Boundary")]
[Trait("Type", "Boundary")]
public class BoundaryTests : PNO1197TestFixtureBase, IDisposable
{
    #region BND_001-015: DoA code boundaries

    [Fact]
    public async Task BND_001_CodeExactlyDoA2_Succeeds()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(1));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_002_CodeDoA2Something_ExactMatchRequired()
    {
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 291, Name = "DoA2-suffix", Code = "DoA2-something", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "DoA2", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(2, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(2));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_003_CodeDOA2Uppercase_ExactMatchRequired()
    {
        await SeedOpportunityAsync(3, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 290, Name = "DOA2", Code = "DOA2_OrganizationHierarchy", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "DOA2", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(3, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(3));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_004_CodeDoA2Lowercase_ExactMatchRequired()
    {
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 289, Name = "doa2", Code = "doa2_OrganizationHierarchy", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "doa2", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(4, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(4));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_005_CodeDoASpace2_ExactMatchRequired()
    {
        await SeedOpportunityAsync(5, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 288, Name = "DoA 2", Code = "DoA 2", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "DoA 2", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(5, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(5));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_006_CodeDoA2Level2_Succeeds()
    {
        await SeedOpportunityAsync(6, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(6, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(6));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_007_CodeExactlyDoA3_Succeeds()
    {
        await SeedOpportunityAsync(7, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(7, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(7));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_008_CodeDoA3Something_ExactMatchRequired()
    {
        await SeedOpportunityAsync(8, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 287, Name = "DoA3-suffix", Code = "DoA3-something", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "DoA3", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(8, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(8));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_009_CodeWithTrailingWhitespace_Fails()
    {
        await SeedOpportunityAsync(9, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 286, Name = "DoA2 ", Code = "DoA2_OrganizationHierarchy ", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "DoA2", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(9, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(9));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_010_CodeWithLeadingWhitespace_Fails()
    {
        await SeedOpportunityAsync(10, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 285, Name = " DoA2", Code = " DoA2_OrganizationHierarchy", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "DoA2", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(10, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(10));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_011_CodeWithNull_Fails()
    {
        await SeedOpportunityAsync(11, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 284, Name = "Null", Code = null!, EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "Null", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(11, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(11));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_012_CodeWithEmptyString_Fails()
    {
        await SeedOpportunityAsync(12, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 283, Name = "Empty", Code = "", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "Empty", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(12, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(12));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_013_CodeWithOnlyNumbers_Fails()
    {
        await SeedOpportunityAsync(13, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 282, Name = "123", Code = "123", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "123", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(13, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(13));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_014_Code2DoA_Fails()
    {
        await SeedOpportunityAsync(14, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 281, Name = "2DoA", Code = "2DoA_OrganizationHierarchy", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "2DoA", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(14, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(14));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_015_CodeDoA23_Fails()
    {
        await SeedOpportunityAsync(15, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 280, Name = "DoA23", Code = "DoA23_OrganizationHierarchy", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "DoA23", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(15, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(15));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    #endregion

    #region BND_016-030: Entity boundaries

    [Fact]
    public async Task BND_016_EntityUserRoleWithIdZero_Succeeds()
    {
        await SeedOpportunityAsync(16, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(16, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(16));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_017_EntityUserRoleWithMaxId_Succeeds()
    {
        await SeedOpportunityAsync(17, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(17, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(17));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_018_MultipleDoAHoldersForSameOrg_Succeeds()
    {
        await SeedOpportunityAsync(18, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(18, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(18));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_019_DoAHolderWithMultipleRoles_Succeeds()
    {
        await SeedOpportunityAsync(19, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedOpportunityManagerStakeholderAsync(19, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(19));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_020_OrgUnitWithNoDoAHolders_Fails()
    {
        await SeedOpportunityAsync(20, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(20, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(20));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_021_OrgUnitAtRootLevel_Succeeds()
    {
        await SeedOpportunityAsync(21, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(21, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(21));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_022_OrgUnitAtDeepestLevel_Succeeds()
    {
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 60))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy { Id = 60, Name = "Deep", Code = "D", Description = "D", Status = EntityStatus.Active, IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }
        await SeedOpportunityAsync(22, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(22);
        opp!.ResponsibleOrgUnitId = 60;
        await DbContext.SaveChangesAsync();
        await SeedDoAHolderAsync(60, 2);
        await SeedOpportunityManagerStakeholderAsync(22, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(22));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_023_EntityRoleWithSameCodeForDifferentEntityTypes_Succeeds()
    {
        await SeedOpportunityAsync(23, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(23, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(23));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_024_EntityUserRoleWithNullEntityRoleId_Fails()
    {
        await SeedOpportunityAsync(24, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = 0, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "NoRole", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(24, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(24));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_025_EntityUserRoleWhereEntityRoleHasNoCode_Fails()
    {
        await SeedOpportunityAsync(25, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 279, Name = "NoCode", Code = null!, EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "NoCode", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(25, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(25));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_026_EntityRoleWithVeryLongCode_Succeeds()
    {
        await SeedOpportunityAsync(26, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(26, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(26));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_027_EntityRoleWithSpecialCharsInCode_Fails()
    {
        await SeedOpportunityAsync(27, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        var entityRole = new EntityRole { Id = 278, Name = "Special", Code = "DoA2@OrganizationHierarchy", EntityType = "OrganizationHierarchy", Status = EntityStatus.Active, IsDeleted = false };
        DbContext.EntityRoles.Add(entityRole);
        await DbContext.SaveChangesAsync();
        var nextId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1 : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole { Id = nextId, UserId = 1, EntityRoleId = entityRole.Id, EntityRole = entityRole, EntityId = 1, EntityType = "OrganizationHierarchy", Name = "Special", IsDeleted = false });
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(27, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(27));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_028_MultipleOrgUnitsWithDoA_Succeeds()
    {
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 61))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy { Id = 61, Name = "OU2", Code = "O2", Description = "O2", Status = EntityStatus.Active, IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }
        await SeedDoAHolderAsync(61, 2);
        await SeedOpportunityAsync(28, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(28, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(28));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_029_EntityUserRoleJustCreated_Succeeds()
    {
        await SeedOpportunityAsync(29, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedOpportunityManagerStakeholderAsync(29, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(29));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_030_EntityUserRoleJustDeleted_Fails()
    {
        await SeedOpportunityAsync(30, "IDENTIFY & PROFILE");
        var holders = await DbContext.EntityUserRoles.Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1).ToListAsync();
        foreach (var h in holders) h.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(30, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(30));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    #endregion

    #region BND_031-045: Opportunity boundaries

    [Fact]
    public async Task BND_031_OpportunityWithMinimumRequiredData_Succeeds()
    {
        await SeedOpportunityAsync(31, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(31, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(31));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_032_OpportunityWithMaximumData_Succeeds()
    {
        await SeedOpportunityAsync(32, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(32, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(32));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_033_OpportunityWithExactly1Country_Succeeds()
    {
        await SeedOpportunityAsync(33, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(33, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(33));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_034_OpportunityWith100Countries_Succeeds()
    {
        await SeedOpportunityAsync(34, "IDENTIFY & PROFILE");
        for (var i = 2; i <= 100; i++)
        {
            if (!await DbContext.Set<Country>().AnyAsync(c => c.Id == i))
            {
                DbContext.Set<Country>().Add(new Country { Id = i, Name = $"C{i}", Iso2Code = $"C{i}", Status = EntityStatus.Active, IsDeleted = false });
            }
            if (!await DbContext.Set<OpportunityCountry>().AnyAsync(oc => oc.OpportunityId == 34 && oc.CountryId == i))
            {
                DbContext.Set<OpportunityCountry>().Add(new OpportunityCountry { Id = 3400 + i, OpportunityId = 34, CountryId = i, Name = $"C{i}" });
            }
        }
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(34, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(34));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_035_OpportunityWithAllNullableFieldsNull_Succeeds()
    {
        await SeedOpportunityAsync(35, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(35, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(35));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_036_OpportunityWithAllFieldsSet_Succeeds()
    {
        await SeedOpportunityAsync(36, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(36, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(36));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_037_OpportunityAtStageBoundary_Succeeds()
    {
        await SeedOpportunityAsync(37, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(37, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(37));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_038_OpportunityWithBudgetAtDecimalLimit_Succeeds()
    {
        await SeedOpportunityAsync(38, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(38);
        opp!.InitiativeBudgetUSD = 999999999.99m;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(38, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(38));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_039_OpportunityWithDatesAtLimits_Succeeds()
    {
        await SeedOpportunityAsync(39, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(39);
        opp!.TargetSigningDate = DateTime.MinValue.AddYears(1);
        opp.ImplementationStartDate = DateTime.MinValue.AddYears(2);
        opp.TargetDeliveryDate = DateTime.MaxValue.AddYears(-1);
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(39, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(39));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_040_OpportunityJustCreated_Succeeds()
    {
        await SeedOpportunityAsync(40, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(40, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(40));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_041_OpportunityJustDeleted_Returns404()
    {
        await SeedOpportunityAsync(41, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(41);
        opp!.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "41")).ReturnsAsync(false);
        var result = await Controller.Submit(CreateSubmitRequest(41));
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task BND_042_OpportunityWith0StakeholdersThenOMAdded_Succeeds()
    {
        await SeedOpportunityAsync(42, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(42, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(42));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_043_OpportunityWithMaxIntId_Succeeds()
    {
        var maxId = 999999;
        await SeedOpportunityAsync(maxId, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(maxId, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = maxId, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_044_OpportunityWithNameAt120Chars_Succeeds()
    {
        await SeedOpportunityAsync(44, "IDENTIFY & PROFILE");
        var opp = await DbContext.Opportunities.FindAsync(44);
        opp!.Name = new string('a', 120);
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(44, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(44));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_045_OpportunityWithDescriptionAtMaxLength_Succeeds()
    {
        await SeedOpportunityAsync(45, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(45, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(45));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    #endregion

    #region BND_046-060: Concurrent/timing boundaries

    [Fact]
    public async Task BND_046_DoACheckWithEmptyDB_Fails()
    {
        await SeedOpportunityAsync(46, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(46, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(46));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_047_SubmitImmediatelyAfterDoACreation_Succeeds()
    {
        await SeedOpportunityAsync(47, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(47, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(47));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_048_SubmitImmediatelyAfterDoADeletion_Fails()
    {
        await SeedOpportunityAsync(48, "IDENTIFY & PROFILE");
        var holders = await DbContext.EntityUserRoles.Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1).ToListAsync();
        DbContext.EntityUserRoles.RemoveRange(holders);
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(48, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(48));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_049_SubmitAfterOrgUnitChange_FailsWhenNewOrgUnitHasNoDoA()
    {
        await SeedOpportunityAsync(49, "IDENTIFY & PROFILE");
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 62))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy { Id = 62, Name = "NoDoA", Code = "N", Description = "N", Status = EntityStatus.Active, IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }
        var opp = await DbContext.Opportunities.FindAsync(49);
        opp!.ResponsibleOrgUnitId = 62;
        await DbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(49, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(49));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_050_DoAHolderAddedDuringValidation_Succeeds()
    {
        await SeedOpportunityAsync(50, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(50, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(50));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_051_DoAHolderRemovedDuringValidation_Fails()
    {
        await SeedOpportunityAsync(51, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(51, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(51));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_052_ConcurrentDoAValidationsForSameOpportunity_Succeeds()
    {
        await SeedOpportunityAsync(52, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(52, 1);
        SetupStandardSubmitMocks();
        var task1 = Controller.Submit(CreateSubmitRequest(52));
        var task2 = Controller.Submit(CreateSubmitRequest(52));
        await Task.WhenAll(task1, task2);
        (task1.Result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
        (task2.Result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_053_RapidSequentialSubmitAttempts_Succeeds()
    {
        await SeedOpportunityAsync(53, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(53, 1);
        SetupStandardSubmitMocks();
        var result1 = await Controller.Submit(CreateSubmitRequest(53));
        (result1.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_054_SubmitWithStaleDoACache_Succeeds()
    {
        await SeedOpportunityAsync(54, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(54, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(54));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_055_DoACheckWithTransactionRollback_Fails()
    {
        await SeedOpportunityAsync(55, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedOpportunityManagerStakeholderAsync(55, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(55));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BND_056_DoAValidationWithConnectionRecovery_Succeeds()
    {
        await SeedOpportunityAsync(56, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(56, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(56));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_057_BulkDoACreationAndSubmit_Succeeds()
    {
        for (var i = 1; i <= 5; i++)
        {
            if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 70 + i))
            {
                DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy { Id = 70 + i, Name = $"OU{i}", Code = $"O{i}", Description = "D", Status = EntityStatus.Active, IsDeleted = false });
            }
            await SeedDoAHolderAsync(70 + i, 2);
        }
        await DbContext.SaveChangesAsync();
        await SeedOpportunityAsync(57, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(57, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(57));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_058_DoAValidationPerformanceWithManyEntityUserRoles_Succeeds()
    {
        await SeedOpportunityAsync(58, "IDENTIFY & PROFILE");
        for (var i = 0; i < 20; i++)
        {
            await SeedDoAHolderAsync(1, 2);
        }
        await SeedOpportunityManagerStakeholderAsync(58, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(58));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_059_DoACheckWithSlowDB_Succeeds()
    {
        await SeedOpportunityAsync(59, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(59, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(59));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BND_060_DoACheckDuringConcurrentSeeding_Succeeds()
    {
        await SeedOpportunityAsync(60, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(60, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(CreateSubmitRequest(60));
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    #endregion

    private static WorkflowSubmitRequest CreateSubmitRequest(int entityId) =>
        new()
        {
            EntityName = "opportunity",
            EntityId = entityId,
            NewStage = "GO",
            ConfirmedNonOMSubmission = false,
            ConfirmedOrgUnitWarning = true,
            AcknowledgedStatement = true
        };
}
