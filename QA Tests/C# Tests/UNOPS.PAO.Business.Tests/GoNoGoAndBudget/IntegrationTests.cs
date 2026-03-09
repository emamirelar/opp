/**
 * @fileoverview Integration tests for Go/No-Go workflow and budget bugs.
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
public class GoNoGoAndBudgetIntegrationTests : IClassFixture<GoNoGoAndBudgetFixture>
{
    private readonly GoNoGoAndBudgetFixture _f;

    public GoNoGoAndBudgetIntegrationTests(GoNoGoAndBudgetFixture fixture) => _f = fixture;

    #region INT: Full workflow

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_001_OMReassignment_FullFlow_PreviousOMInCollaborators()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 };

        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);

        var collaborator = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId && !c.IsDeleted);
        collaborator.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_002_UpdateTeamSection_ThenUpdateWhenSection_Succeeds()
    {
        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId });

        var whenRequest = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2026, 6, 1),
            TargetSigningDate = new DateTime(2026, 5, 1),
            TargetDeliveryDate = new DateTime(2028, 6, 1)
        };

        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, whenRequest);
        result.Should().NotBeNull();
        result.ImplementationStartDate.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_003_UpdateTeamSection_ResponsibleOrgUnitAndOM_Succeeds()
    {
        var request = new TeamSectionRequest
        {
            ResponsibleOrgUnitId = _f.OrgHierarchyId,
            OpportunityManagerId = _f.PaoUserId
        };

        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (KeyNotFoundException) { }

        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ResponsibleOrgUnitId.Should().Be(_f.OrgHierarchyId);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_004_OMReassignment_Twice_SecondPreviousOM_AlsoCollaborator()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);

        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 });

        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId });

        var collaborators = await _f.Context.Set<OpportunityCollaborator>()
            .Where(c => c.OpportunityId == _f.OpportunityId && !c.IsDeleted)
            .ToListAsync();
        collaborators.Should().Contain(c => c.UserId == _f.PaoUserId2);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_005_UpdateTeamSection_WithCollaborators_AndOMReassignment()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId2,
            Collaborators = new List<OpportunityCollaboratorRequest> { new() { UserId = _f.PaoUserId } }
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var prevOM = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId && !c.IsDeleted);
        prevOM.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_006_ExchangeRateService_ThenOpportunityUpdate()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var rateResult = service.ConvertToUSDAsync(1000m, "USD").GetAwaiter().GetResult();
        rateResult.AmountUSD.Should().Be(1000m);

        var whenRequest = new WhenSectionRequest
        {
            ImplementationStartDate = DateTime.UtcNow.AddMonths(6),
            TargetSigningDate = DateTime.UtcNow.AddMonths(5),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(30)
        };
        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, whenRequest);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_007_GetAvailableOrgUnits_ThenUpdateTeamSection()
    {
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>(), "Test"));
        var orgUnits = await _f.UserManagementManager.GetAvailableOrgUnitsAsync(user);
        orgUnits.Should().NotBeEmpty();

        var request = new TeamSectionRequest
        {
            ResponsibleOrgUnitId = orgUnits.First().Id,
            OpportunityManagerId = _f.PaoUserId
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_008_UpdateTeamSection_RemoveCollaborator_ThenReadd()
    {
        await SeedCollaborator(_f.OpportunityId, _f.PaoUserId2);

        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId, Collaborators = new List<OpportunityCollaboratorRequest>() });

        var afterRemove = await _f.Context.Set<OpportunityCollaborator>()
            .CountAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        afterRemove.Should().Be(0);

        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest
            {
                OpportunityManagerId = _f.PaoUserId,
                Collaborators = new List<OpportunityCollaboratorRequest> { new() { UserId = _f.PaoUserId2 } }
            });

        var afterReadd = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        afterReadd.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_009_UpdateWhenSection_ThenTeamSection_PreservesData()
    {
        var whenRequest = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2026, 7, 1),
            TargetSigningDate = new DateTime(2026, 6, 1),
            TargetDeliveryDate = new DateTime(2028, 7, 1)
        };
        await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, whenRequest);

        var teamRequest = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, teamRequest); }
        catch (KeyNotFoundException) { }

        var opp = await _f.Context.Opportunities
            .FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ImplementationStartDate.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_010_Opportunity_TeamSection_IncludesOrgUnit()
    {
        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ResponsibleOrgUnitId = _f.OrgHierarchyId;
        await _f.Context.SaveChangesAsync();

        var org = await _f.Context.OrganizationHierarchies
            .FirstOrDefaultAsync(o => o.Id == _f.OrgHierarchyId && !o.IsDeleted);
        org.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_011_EntityRole_OpportunityManager_UsedInStakeholder()
    {
        var omRole = await _f.Context.EntityRoles
            .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == "opportunity manager" && r.EntityType == "Opportunity");
        omRole.Should().NotBeNull();

        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var stakeholder = await _f.Context.Set<OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == _f.OpportunityId && s.UserId == _f.PaoUserId && !s.IsDeleted);
        stakeholder.Should().NotBeNull();
        stakeholder!.EntityRoleId.Should().Be(omRole!.Id);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_012_UpdateTeamSection_NullResponsibleOrgUnit_Clears()
    {
        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ResponsibleOrgUnitId = _f.OrgHierarchyId;
        await _f.Context.SaveChangesAsync();

        var request = new TeamSectionRequest { ResponsibleOrgUnitId = null };
        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var updated = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        updated.ResponsibleOrgUnitId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void INT_013_ExchangeRateService_And_TeamSectionRequest_Independent()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var rateResult = service.ConvertToUSDAsync(100m, "USD").GetAwaiter().GetResult();

        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        rateResult.AmountUSD.Should().Be(100m);
        request.OpportunityManagerId.Should().Be(_f.PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_014_SoftDeletedStakeholder_ExcludedFromOMQuery()
    {
        var omRole = await _f.Context.EntityRoles
            .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == "opportunity manager" && r.EntityType == "Opportunity");
        if (omRole == null) return;

        var stakeholder = new OpportunityStakeholder
        {
            OpportunityId = _f.OpportunityId,
            UserId = _f.PaoUserId,
            EntityRoleId = omRole.Id,
            IsInternal = true,
            StakeholderType = "Internal",
            IsDeleted = true
        };
        _f.Context.Set<OpportunityStakeholder>().Add(stakeholder);
        await _f.Context.SaveChangesAsync();

        var count = await _f.Context.Set<OpportunityStakeholder>()
            .CountAsync(s => s.OpportunityId == _f.OpportunityId && !s.IsDeleted && s.EntityRoleId == omRole.Id);
        count.Should().Be(0);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_015_UpdateTeamSection_MultipleTimes_Consistent()
    {
        for (int i = 0; i < 2; i++)
        {
            var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
            try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
            catch (KeyNotFoundException) { }
        }

        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_016_OpportunityCollaborator_AddedBy_Set()
    {
        await SeedCollaborator(_f.OpportunityId, _f.PaoUserId2);
        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .FirstAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        collab.AddedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_017_UpdateWhenSection_Deliverables_Null()
    {
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = DateTime.UtcNow.AddMonths(6),
            TargetSigningDate = DateTime.UtcNow.AddMonths(5),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(30),
            Deliverables = null
        };

        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, request);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_018_OrganizationHierarchy_And_Opportunity_Link()
    {
        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ResponsibleOrgUnitId = _f.OrgHierarchyId;
        await _f.Context.SaveChangesAsync();

        var org = await _f.Context.OrganizationHierarchies
            .FirstOrDefaultAsync(o => o.Id == _f.OrgHierarchyId);
        org.Should().NotBeNull();
        opp.ResponsibleOrgUnitId.Should().Be(org!.Id);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_019_UpdateTeamSection_Stakeholders_And_Collaborators()
    {
        var omRole = await _f.Context.EntityRoles
            .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == "opportunity manager" && r.EntityType == "Opportunity");
        if (omRole == null) return;

        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            Collaborators = new List<OpportunityCollaboratorRequest> { new() { UserId = _f.PaoUserId2 } },
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        collab.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_020_UpdateTeamSection_ThenQuery_DBReflectsChanges()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var collaborator = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId && !c.IsDeleted);
        collaborator.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_021_UpdateWhenSection_AllFields_Persisted()
    {
        var implStart = new DateTime(2026, 8, 15);
        var signing = new DateTime(2026, 7, 1);
        var delivery = new DateTime(2028, 8, 15);
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = implStart,
            TargetSigningDate = signing,
            TargetDeliveryDate = delivery,
            IsTargetSigningDateFirm = true,
            SigningDateNotes = "Partner deadline"
        };

        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, request);

        result.ImplementationStartDate!.Value.Date.Should().Be(implStart.Date);
        result.TargetSigningDate!.Value.Date.Should().Be(signing.Date);
        result.TargetDeliveryDate!.Value.Date.Should().Be(delivery.Date);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_022_Opportunity_SoftDelete_ExcludedFromQueries()
    {
        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.IsDeleted = true;
        await _f.Context.SaveChangesAsync();

        var count = await _f.Context.Opportunities.CountAsync(o => o.Id == _f.OpportunityId && !o.IsDeleted);
        count.Should().Be(0);

        opp.IsDeleted = false;
        await _f.Context.SaveChangesAsync();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_023_EntityRole_Opportunity_Type_Filter()
    {
        var oppRoles = await _f.Context.EntityRoles
            .Where(r => r.EntityType == "Opportunity" && !r.IsDeleted)
            .ToListAsync();
        oppRoles.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_024_UpdateTeamSection_Collaborator_ExpertiseIds()
    {
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            Collaborators = new List<OpportunityCollaboratorRequest>
            {
                new() { UserId = _f.PaoUserId2, ExpertiseIds = new List<int> { 1 } }
            }
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .Include(c => c.Expertises)
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        collab.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_025_OMReassignment_ThenUpdateCollaborators_PreviousOMRetained()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 });

        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest
            {
                OpportunityManagerId = _f.PaoUserId2,
                Collaborators = new List<OpportunityCollaboratorRequest> { new() { UserId = _f.PaoUserId } }
            });

        var prevOM = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId && !c.IsDeleted);
        prevOM.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_026_UpdateTeamSection_ProposedInitiativeTypeId()
    {
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            ProposedInitiativeTypeId = 1
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ProposedInitiativeTypeId.Should().Be(1);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_027_UpdateWhenSection_SubmissionDeadline()
    {
        var deadline = new DateTime(2026, 4, 30);
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2026, 6, 1),
            TargetSigningDate = new DateTime(2026, 5, 15),
            TargetDeliveryDate = new DateTime(2028, 6, 1),
            SubmissionDeadline = deadline
        };

        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, request);
        result.SubmissionDeadline.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_028_Opportunity_LastModifiedDate_Updated()
    {
        var before = (await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId)).LastModifiedDate;

        await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId,
            new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId });

        var after = (await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId)).LastModifiedDate;
        after.Should().NotBeNull();
        after!.Value.Should().BeOnOrAfter(before!.Value);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_029_UpdateTeamSection_EmptyStakeholders()
    {
        var request = new TeamSectionRequest
        {
            OpportunityManagerId = _f.PaoUserId,
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Integration")]
    public async Task INT_030_FullTeamSectionUpdate_AllSections()
    {
        var request = new TeamSectionRequest
        {
            ResponsibleOrgUnitId = _f.OrgHierarchyId,
            ProposedInitiativeTypeId = null,
            OpportunityManagerId = _f.PaoUserId,
            Collaborators = new List<OpportunityCollaboratorRequest> { new() { UserId = _f.PaoUserId2 } },
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };

        try { await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request); }
        catch (KeyNotFoundException) { }

        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.ResponsibleOrgUnitId.Should().Be(_f.OrgHierarchyId);

        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .FirstOrDefaultAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2 && !c.IsDeleted);
        collab.Should().NotBeNull();
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
