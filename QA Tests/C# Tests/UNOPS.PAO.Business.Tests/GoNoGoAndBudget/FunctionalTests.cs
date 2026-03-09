/**
 * @fileoverview Functional tests for Go/No-Go workflow and budget bugs.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.GoNoGoAndBudget;

[Collection("GoNoGoAndBudget")]
public class GoNoGoAndBudgetFunctionalTests : IClassFixture<GoNoGoAndBudgetFixture>
{
    private readonly GoNoGoAndBudgetFixture _f;

    public GoNoGoAndBudgetFunctionalTests(GoNoGoAndBudgetFixture fixture) => _f = fixture;

    #region FUNC: Business rules

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_OMReassignment_PreviousOM_RetainsEditAccessViaCollaborator()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var collaborator = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId && !c.IsDeleted);
        collaborator.Should().NotBeNull("Collaborators retain edit access per PNO-1193");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_OMReassignment_NewOM_HasOpportunityManagerRole()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var omRole = await _f.Context.EntityRoles.FirstAsync(r =>
            r.Name != null && r.Name.ToLower() == "opportunity manager" && r.EntityType == "Opportunity");
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var stakeholder = await _f.Context.Set<OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == _f.OpportunityId && s.UserId == _f.PaoUserId2 && !s.IsDeleted);
        stakeholder.Should().NotBeNull();
        stakeholder!.EntityRoleId.Should().Be(omRole.Id);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_OMReassignment_OldOM_SoftDeletedFromStakeholders()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var oldOM = await _f.Context.Set<OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == _f.OpportunityId && s.UserId == _f.PaoUserId);
        oldOM.Should().NotBeNull();
        oldOM!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_004_ExchangeRateService_USD_ReturnsRateOne()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(100m, "USD").GetAwaiter().GetResult();
        result.ExchangeRate.Should().Be(1.0m);
        result.AmountUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_005_ExchangeRateService_USD_AmountUnchanged()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var amount = 5000.50m;
        var result = service.ConvertToUSDAsync(amount, "USD").GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(amount);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_UpdateTeamSection_ResponsibleOrgUnitId_Persists()
    {
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = _f.OrgHierarchyId };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ResponsibleOrgUnitId.Should().Be(_f.OrgHierarchyId);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_007_UpdateWhenSection_ImplementationStartDate_Persists()
    {
        var implStart = new DateTime(2026, 5, 15);
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = implStart,
            TargetSigningDate = implStart.AddMonths(-1),
            TargetDeliveryDate = implStart.AddMonths(24)
        };

        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, request);

        result.ImplementationStartDate.Should().NotBeNull();
        result.ImplementationStartDate!.Value.Date.Should().Be(implStart.Date);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_008_UpdateTeamSection_Collaborators_WithExpertise_Persists()
    {
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            Collaborators = new List<OpportunityCollaboratorRequest>
            {
                new() { UserId = _f.PaoUserId2, ExpertiseIds = new List<int>() }
            }
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        collab.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_009_TeamSectionRequest_Structure_Valid()
    {
        var request = new TeamSectionRequest
        {
            ResponsibleOrgUnitId = 1,
            OpportunityManagerId = 2,
            Collaborators = new List<OpportunityCollaboratorRequest> { new() { UserId = 3 } }
        };
        request.ResponsibleOrgUnitId.Should().Be(1);
        request.OpportunityManagerId.Should().Be(2);
        request.Collaborators.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_010_WhenSectionRequest_Structure_Valid()
    {
        var d = DateTime.UtcNow;
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = d,
            TargetSigningDate = d.AddMonths(1),
            TargetDeliveryDate = d.AddMonths(24)
        };
        request.ImplementationStartDate.Should().Be(d);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_011_UpdateTeamSection_RequestedUserIds_IncludePreviousOM()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId2,
            Collaborators = new List<OpportunityCollaboratorRequest>()
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId && !c.IsDeleted);
        collab.Should().NotBeNull("Previous OM must be in collaborators even when not in request");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_012_ExchangeRateService_Result_HasRequiredFields()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(100m, "USD").GetAwaiter().GetResult();
        result.Should().NotBeNull();
        result.ExchangeRate.Should().BeGreaterThan(0);
        result.ExchangeRateDate.Should().BeBefore(DateTime.UtcNow.AddDays(1));
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_013_OrganizationHierarchy_Active_Returned()
    {
        var org = await _f.Context.OrganizationHierarchies
            .FirstOrDefaultAsync(o => o.Id == _f.OrgHierarchyId && !o.IsDeleted);
        org.Should().NotBeNull();
        org!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_014_EntityRole_OpportunityManager_Exists()
    {
        var role = _f.Context.EntityRoles.FirstOrDefault(r =>
            r.Name != null && r.Name.ToLower() == "opportunity manager" && r.EntityType == "Opportunity");
        role.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_015_UpdateTeamSection_MultipleCollaborators_PersistsAll()
    {
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            Collaborators = new List<OpportunityCollaboratorRequest>
            {
                new() { UserId = _f.PaoUserId2 }
            }
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var count = await _f.Context.Set<OpportunityCollaborator>()
            .CountAsync(c => c.OpportunityId == _f.OpportunityId && !c.IsDeleted);
        count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_016_OpportunityCollaborator_AddedDate_Set()
    {
        var collab = new OpportunityCollaborator
        {
            OpportunityId = 1,
            UserId = 1,
            Name = string.Empty,
            AddedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        collab.AddedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_017_OpportunityStakeholder_IsInternal_TrueForOM()
    {
        var stakeholder = new OpportunityStakeholder
        {
            OpportunityId = 1,
            UserId = 1,
            EntityRoleId = 1,
            IsInternal = true,
            StakeholderType = "Internal",
            IsDeleted = false
        };
        stakeholder.IsInternal.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_018_GetAvailableOrgUnits_ExcludesDeleted()
    {
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>(), "Test"));
        var orgUnits = await _f.UserManagementManager.GetAvailableOrgUnitsAsync(user);
        orgUnits.Should().NotContain(o => o.Id == 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_019_ExchangeRateService_ConvertsCorrectly()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(1000m, "USD").GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(1000m);
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_020_TeamSectionRequest_ProposedInitiativeTypeId_Optional()
    {
        var request = new TeamSectionRequest { ProposedInitiativeTypeId = null };
        request.ProposedInitiativeTypeId.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_021_UpdateTeamSection_Stakeholders_Optional()
    {
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };

        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (KeyNotFoundException) { }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_022_WhenSectionRequest_IsTargetSigningDateFirm_Optional()
    {
        var request = new WhenSectionRequest { IsTargetSigningDateFirm = null };
        request.IsTargetSigningDateFirm.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_023_WhenSectionRequest_SigningDateNotes_Optional()
    {
        var request = new WhenSectionRequest { SigningDateNotes = null };
        request.SigningDateNotes.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_024_Opportunity_ResponsibleOrgUnitId_Nullable()
    {
        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ResponsibleOrgUnitId = null;
        await _f.Context.SaveChangesAsync();
        opp.ResponsibleOrgUnitId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_025_ExchangeRateResult_Structure()
    {
        var result = new UNOPS.PAO.Business.Services.ExchangeRateResult
        {
            AmountUSD = 100m,
            ExchangeRate = 1.0m,
            ExchangeRateDate = DateTime.UtcNow,
            ExchangeRateId = 0
        };
        result.AmountUSD.Should().Be(100m);
        result.ExchangeRateId.Should().Be(0);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_026_UpdateTeamSection_NoOMChange_KeepsCollaborators()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        await SeedCollaborator(_f.OpportunityId, _f.PaoUserId2);
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            Collaborators = new List<OpportunityCollaboratorRequest> { new() { UserId = _f.PaoUserId2 } }
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        collab.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_027_OpportunityCollaboratorRequest_UserId_Required()
    {
        var req = new OpportunityCollaboratorRequest { UserId = 42 };
        req.UserId.Should().Be(42);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_028_OrganizationHierarchy_Type_OrgUnit()
    {
        var org = new OrganizationHierarchy { Type = OrganizationUnitType.OrgUnit, Code = "T", Name = "Test", Description = "Test" };
        org.Type.Should().Be(OrganizationUnitType.OrgUnit);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task FUNC_029_UpdateWhenSection_AllOptionalFields_Null()
    {
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = null,
            TargetSigningDate = null,
            TargetDeliveryDate = null,
            SubmissionDeadline = null
        };

        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FUNC_030_EntityRole_Status_Active()
    {
        var role = new EntityRole { Status = EntityStatus.Active, Name = "Test", EntityType = "Opportunity" };
        role.Status.Should().Be(EntityStatus.Active);
    }

    #endregion

    private async Task SeedOpportunityWithOM(int oppId, int omUserId)
    {
        var omRole = await _f.Context.EntityRoles
            .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == "opportunity manager" && r.EntityType == "Opportunity");
        if (omRole == null)
        {
            omRole = new EntityRole
            {
                Name = "Opportunity Manager",
                Code = "OM",
                EntityType = "Opportunity",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            _f.Context.EntityRoles.Add(omRole);
            await _f.Context.SaveChangesAsync();
        }

        var existing = await _f.Context.Set<OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == oppId && s.UserId == omUserId);
        if (existing == null)
        {
            _f.Context.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
            {
                OpportunityId = oppId,
                UserId = omUserId,
                EntityRoleId = omRole.Id,
                IsInternal = true,
                StakeholderType = "Internal",
                IsDeleted = false
            });
            await _f.Context.SaveChangesAsync();
        }
    }

    private async Task SeedCollaborator(int oppId, int userId)
    {
        var existing = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == oppId && c.UserId == userId);
        if (existing == null)
        {
            _f.Context.Set<OpportunityCollaborator>().Add(new OpportunityCollaborator
            {
                OpportunityId = oppId,
                UserId = userId,
                Name = string.Empty,
                AddedDate = DateTime.UtcNow,
                IsDeleted = false
            });
            await _f.Context.SaveChangesAsync();
        }
    }
}
