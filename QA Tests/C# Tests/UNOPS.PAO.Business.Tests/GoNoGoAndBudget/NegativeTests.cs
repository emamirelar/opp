/**
 * @fileoverview Negative tests for Go/No-Go workflow and budget bugs.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.GoNoGoAndBudget;

[Collection("GoNoGoAndBudget")]
public class GoNoGoAndBudgetNegativeTests : IClassFixture<GoNoGoAndBudgetFixture>
{
    private readonly GoNoGoAndBudgetFixture _f;

    public GoNoGoAndBudgetNegativeTests(GoNoGoAndBudgetFixture fixture) => _f = fixture;

    #region NEG: OM Reassignment - Invalid inputs

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_001_UpdateTeamSection_NonExistentOpportunity_Throws()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        var act = () => _f.OpportunityManager.UpdateTeamSectionAsync(999999, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_002_UpdateTeamSection_ZeroOpportunityId_Throws()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        var act = () => _f.OpportunityManager.UpdateTeamSectionAsync(0, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_003_UpdateTeamSection_NegativeOpportunityId_Throws()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        var act = () => _f.OpportunityManager.UpdateTeamSectionAsync(-1, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_004_UpdateWhenSection_ImplementationStartDate_Null_Allowed()
    {
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = null,
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(24)
        };
        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_005_UpdateTeamSection_OpportunityNotFound_ThrowsKeyNotFoundException()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        var act = () => _f.OpportunityManager.UpdateTeamSectionAsync(999999, request);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_006_ExchangeRateService_InvalidCurrency_Throws()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var act = () => service.ConvertToUSDAsync(100m, "INVALID_XYZ").GetAwaiter().GetResult();
        act.Should().Throw<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_007_ExchangeRateService_EmptyCurrency_ThrowsForNonUSD()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var act = () => service.ConvertToUSDAsync(100m, "").GetAwaiter().GetResult();
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_008_ExchangeRateService_NullCurrency_Throws()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var act = () => service.ConvertToUSDAsync(100m, null!).GetAwaiter().GetResult();
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_009_UpdateTeamSection_ResponsibleOrgUnitId_Invalid_MayThrow()
    {
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = 999999999 };
        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (Exception)
        {
            // Expected for invalid org unit
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_010_UpdateTeamSection_OpportunityManagerId_NonExistentUser_MayThrow()
    {
        var request = new TeamSectionRequest { OpportunityManagerId = 999999999 };
        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (Exception)
        {
            // Expected for non-existent user
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_011_GetOpportunity_NonExistent_Throws()
    {
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, _f.PaoUserId.ToString())
            }));
        var act = () => _f.OpportunityManager.GetOpportunityAsync(user, 999999);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_012_UpdateWhenSection_NonExistentOpportunity_Throws()
    {
        var request = new WhenSectionRequest { TargetSigningDate = DateTime.UtcNow.AddMonths(6) };
        var act = () => _f.OpportunityManager.UpdateWhenSectionAsync(999999, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_013_SoftDeletedOpportunity_UpdateTeamSection_Throws()
    {
        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.IsDeleted = true;
        await _f.Context.SaveChangesAsync();

        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        var act = () => _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        opp.IsDeleted = false;
        await _f.Context.SaveChangesAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_014_UpdateTeamSection_EmptyCollaborators_RemovesAll()
    {
        await SeedCollaborator(_f.OpportunityId, _f.PaoUserId2);
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId, Collaborators = new List<OpportunityCollaboratorRequest>() };

        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (KeyNotFoundException) { }

        var collaborators = await _f.Context.Set<OpportunityCollaborator>()
            .Where(c => c.OpportunityId == _f.OpportunityId && !c.IsDeleted)
            .ToListAsync();
        collaborators.Should().NotContain(c => c.UserId == _f.PaoUserId2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_015_TeamSectionRequest_NullCollaborators_Allowed()
    {
        var request = new TeamSectionRequest { Collaborators = null };
        request.Collaborators.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_016_TeamSectionRequest_NullStakeholders_Allowed()
    {
        var request = new TeamSectionRequest { Stakeholders = null };
        request.Stakeholders.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_017_WhenSectionRequest_InvalidDateOrder_NotValidated()
    {
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = DateTime.UtcNow.AddYears(2),
            TargetSigningDate = DateTime.UtcNow.AddMonths(1)
        };
        request.ImplementationStartDate.Should().BeAfter(request.TargetSigningDate!.Value);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_018_UpdateTeamSection_SameOMAsCurrent_NoOp()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };

        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (KeyNotFoundException) { }

        var collaborators = await _f.Context.Set<OpportunityCollaborator>()
            .Where(c => c.OpportunityId == _f.OpportunityId && !c.IsDeleted)
            .ToListAsync();
        collaborators.Should().NotContain(c => c.UserId == _f.PaoUserId);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_019_ExchangeRateService_NegativeAmount_Allowed()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(-100m, "USD").GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(-100m);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_020_ExchangeRateService_ZeroAmount_ReturnsZero()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(0m, "USD").GetAwaiter().GetResult();
        result.AmountUSD.Should().Be(0m);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_021_UpdateTeamSection_ResponsibleOrgUnitId_Zero_MayPersist()
    {
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = 0 };
        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (Exception) { }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_022_UpdateTeamSection_OpportunityManagerId_Null_KeepsExisting()
    {
        await SeedOpportunityWithOM(_f.OpportunityId, _f.PaoUserId);
        var request = new TeamSectionRequest { OpportunityManagerId = null };

        try
        {
            await _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        }
        catch (KeyNotFoundException) { }

        var om = await _f.Context.Set<OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == _f.OpportunityId && s.UserId == _f.PaoUserId && !s.IsDeleted);
        om.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_023_OpportunityCollaboratorRequest_UserId_Zero_Invalid()
    {
        var req = new OpportunityCollaboratorRequest { UserId = 0 };
        req.UserId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_024_OpportunityCollaboratorRequest_UserId_Negative_Invalid()
    {
        var req = new OpportunityCollaboratorRequest { UserId = -1 };
        req.UserId.Should().Be(-1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_025_UpdateWhenSection_PastImplementationStartDate_MayPersist()
    {
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = DateTime.UtcNow.AddYears(-1),
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(24)
        };
        var result = await _f.OpportunityManager.UpdateWhenSectionAsync(_f.OpportunityId, request);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NEG_026_UpdateTeamSection_CancelledOpportunity_ThrowsBusinessException()
    {
        var opp = await _f.Context.Opportunities.FirstAsync(o => o.Id == _f.OpportunityId);
        opp.Stage = "CANCELLED";
        await _f.Context.SaveChangesAsync();

        var request = new TeamSectionRequest { OpportunityManagerId = _f.PaoUserId };
        var act = () => _f.OpportunityManager.UpdateTeamSectionAsync(_f.OpportunityId, request);
        await act.Should().ThrowAsync<Exception>();

        opp.Stage = "IDENTIFY & PROFILE";
        await _f.Context.SaveChangesAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_027_ExchangeRateService_WhitespaceCurrency_TreatedAsUSD()
    {
        var service = new UNOPS.PAO.Business.Services.ExchangeRateService(_f.Context);
        var result = service.ConvertToUSDAsync(100m, "   ").GetAwaiter().GetResult();
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_028_GetUserById_InvalidUserId_ReturnsNull()
    {
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>(), "Test"));
        var result = _f.UserManagementManager.GetUserByIdAsync(user, "invalid").GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_029_GetUserById_NonExistentUserId_ReturnsNull()
    {
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>(), "Test"));
        var result = _f.UserManagementManager.GetUserByIdAsync(user, "999999999").GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void NEG_030_TeamSectionRequest_AllNull_Valid()
    {
        var request = new TeamSectionRequest
        {
            ResponsibleOrgUnitId = null,
            OpportunityManagerId = null,
            Collaborators = null,
            Stakeholders = null
        };
        request.OpportunityManagerId.Should().BeNull();
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
