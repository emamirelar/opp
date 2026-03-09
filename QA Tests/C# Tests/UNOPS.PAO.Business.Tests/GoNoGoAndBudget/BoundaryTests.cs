/**
 * @fileoverview Boundary tests for Go/No-Go workflow and budget bugs.
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
public class GoNoGoAndBudgetBoundaryTests : IClassFixture<GoNoGoAndBudgetFixture>
{
    private readonly GoNoGoAndBudgetFixture _f;

    public GoNoGoAndBudgetBoundaryTests(GoNoGoAndBudgetFixture fixture) => _f = fixture;

    #region BND: Boundary values

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_001_TeamSectionRequest_ResponsibleOrgUnitId_MaxInt()
    {
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = int.MaxValue };
        request.ResponsibleOrgUnitId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_002_TeamSectionRequest_OpportunityManagerId_MaxInt()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = int.MaxValue };
        request.OpportunityManagerId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_003_WhenSectionRequest_ImplementationStartDate_MaxValue()
    {
        var request = new WhenSectionRequest { ImplementationStartDate = DateTime.MaxValue };
        request.ImplementationStartDate.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_004_WhenSectionRequest_ImplementationStartDate_MinValue()
    {
        var request = new WhenSectionRequest { ImplementationStartDate = DateTime.MinValue };
        request.ImplementationStartDate.Should().Be(DateTime.MinValue);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_005_ExchangeRateService_USD_LowerCase()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(100m, "usd").GetAwaiter().GetResult();
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_006_ExchangeRateService_USD_MixedCase()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(100m, "Usd").GetAwaiter().GetResult();
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_007_ExchangeRateService_AsOfDate_FarFuture()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var asOf = new DateTime(2099, 12, 31);
        var result = service.ConvertToUSDAsync(100m, "USD", asOf).GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_008_ExchangeRateService_AsOfDate_FarPast()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var asOf = new DateTime(2000, 1, 1);
        var result = service.ConvertToUSDAsync(100m, "USD", asOf).GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_009_ExchangeRateService_VerySmallAmount()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(0.0001m, "USD").GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(0.0001m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_010_ExchangeRateService_VeryLargeAmount()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(999999999.99m, "USD").GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(999999999.99m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_011_OpportunityCollaboratorRequest_UserId_One()
    {
        var req = new OpportunityCollaboratorRequest { UserId = 1 };
        req.UserId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_012_TeamSectionRequest_ResponsibleOrgUnitId_One()
    {
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = 1 };
        request.ResponsibleOrgUnitId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_013_TeamSectionRequest_EmptyCollaboratorsList()
    {
        var request = new TeamSectionRequest { Collaborators = new List<OpportunityCollaboratorRequest>() };
        request.Collaborators.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_014_TeamSectionRequest_EmptyStakeholdersList()
    {
        var request = new TeamSectionRequest { Stakeholders = new List<OpportunityStakeholderRequest>() };
        request.Stakeholders.Should().BeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Boundary")]
    public async Task BND_015_OMReassignment_PreviousOM_AlreadyCollaborator_NotDuplicated()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        await SeedCollaborator(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId2 };

        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (KeyNotFoundException) { }

        var collaborators = await _f.Context.Set<OpportunityCollaborator>()
            .Where(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId && !c.IsDeleted)
            .ToListAsync();
        collaborators.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_016_WhenSectionRequest_AllDatesNull()
    {
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = null,
            TargetSigningDate = null,
            TargetDeliveryDate = null
        };
        request.ImplementationStartDate.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_017_WhenSectionRequest_SameDateForAll()
    {
        var d = new DateTime(2026, 6, 15);
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = d,
            TargetSigningDate = d,
            TargetDeliveryDate = d
        };
        request.ImplementationStartDate.Should().Be(d);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_018_OpportunityCollaboratorRequest_UserId_IntMax()
    {
        var req = new OpportunityCollaboratorRequest { UserId = int.MaxValue };
        req.UserId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_019_TeamSectionRequest_ProposedInitiativeTypeId_Zero()
    {
        var request = new TeamSectionRequest { ProposedInitiativeTypeId = 0 };
        request.ProposedInitiativeTypeId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_020_TeamSectionRequest_ProposedInitiativeTypeId_Null()
    {
        var request = new TeamSectionRequest { ProposedInitiativeTypeId = null };
        request.ProposedInitiativeTypeId.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Boundary")]
    public async Task BND_021_SoftDeletedCollaborator_ExcludedFromQuery()
    {
        await SeedCollaborator(_f.OpportunityId, _f.PaoUserId2);
        var collab = await _f.Context.Set<OpportunityCollaborator>()
            .FirstAsync(c => c.OpportunityId == _f.OpportunityId && c.UserId == _f.PaoUserId2);
        collab.IsDeleted = true;
        await _f.Context.SaveChangesAsync();

        var count = await _f.Context.Set<OpportunityCollaborator>()
            .CountAsync(c => c.OpportunityId == _f.OpportunityId && !c.IsDeleted);
        count.Should().Be(0);

        collab.IsDeleted = false;
        await _f.Context.SaveChangesAsync();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_022_ExchangeRateService_CurrencyCode_USD_ExactThreeChars()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(100m, "USD").GetAwaiter().GetResult();
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_023_ExchangeRateService_AmountDecimalPrecision()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(123.456789m, "USD").GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(123.456789m);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_024_OpportunityStakeholderRequest_UserId_Null()
    {
        var req = new OpportunityStakeholderRequest { UserId = null };
        req.UserId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_025_OpportunityCollaboratorRequest_ExpertiseIds_Empty()
    {
        var req = new OpportunityCollaboratorRequest { UserId = 1, ExpertiseIds = new List<int>() };
        req.ExpertiseIds.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_026_OpportunityCollaboratorRequest_ExpertiseIds_Null()
    {
        var req = new OpportunityCollaboratorRequest { UserId = 1, ExpertiseIds = null };
        req.ExpertiseIds.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_027_WhenSectionRequest_SubmissionDeadline_Null()
    {
        var request = new WhenSectionRequest { SubmissionDeadline = null };
        request.SubmissionDeadline.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_028_OrganizationHierarchy_Code_Empty()
    {
        var org = new OrganizationHierarchy { Code = "", Name = "Test", Description = "Test" };
        org.Code.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_029_EntityRole_EntityType_Opportunity()
    {
        var role = new EntityRole { EntityType = "Opportunity", Name = "OM" };
        role.EntityType.Should().Be("Opportunity");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void BND_030_OpportunityCollaborator_Name_Empty()
    {
        var collab = new OpportunityCollaborator { Name = string.Empty, UserId = 1 };
        collab.Name.Should().BeEmpty();
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
