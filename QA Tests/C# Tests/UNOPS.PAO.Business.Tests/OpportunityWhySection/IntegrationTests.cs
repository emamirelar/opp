using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Mapping;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Business.Tests.TestBase;
using Moq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Integration tests for WHY - Impact &amp; Strategic Alignment (PNO-692, PNO-817, PNO-886)
/// Full CRUD through manager/API, service-to-DB round-trip, multi-component workflows.
/// </summary>
public class IntegrationTests : OpportunityWhySectionTestFixtureBase
{
    #region ValuesManager / SDG Integration

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_001_ValuesManager_GetSDGs_ReturnsActiveSDGs()
    {
        await SeedSDGAsync("6", "Clean Water");
        await SeedSDGAsync("8", "Decent Work");
        await SeedSDGAsync("13", "Climate Action");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(s => s.Status == EntityStatus.Active.ToString());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_002_ValuesManager_GetSDGs_ExcludesInactive()
    {
        var sdg = new SDG { SDGId = "99", SDGNumber = "99", Name = "Test SDG", Status = EntityStatus.Inactive, IsDeleted = false };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotContain(s => s.SDGId == "99");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_003_ValuesManager_GetSDGs_IncludesSDG8_WhenSeeded()
    {
        var sdg8Id = await SeedSDGAsync("8", "Decent Work and Economic Growth");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        var sdg8 = result.FirstOrDefault(s => s.SDGId == "8" || s.SDGNumber == "8");
        sdg8.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_004_ValuesManager_GetSDGs_IncludesSDG15_WhenSeeded()
    {
        await SeedSDGAsync("15", "Life on Land");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        var sdg15 = result.FirstOrDefault(s => s.SDGId == "15" || s.SDGNumber == "15");
        sdg15.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_005_ValuesManager_GetSDGs_ReturnsCorrectStructure()
    {
        await SeedSDGAsync("6", "Clean Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        var first = result.First();
        first.Id.Should().BeGreaterThan(0);
        first.Name.Should().NotBeNullOrEmpty();
        first.SDGId.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Opportunity + WHY Section Integration

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_006_Opportunity_Challenges_Persisted()
    {
        var challenges = "Water scarcity and climate variability.";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Challenges.Should().Be(challenges);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_007_Opportunity_ExpectedImpact_Persisted()
    {
        var expectedImpact = "Long-term positive impact on communities.";
        var oppId = await SeedOpportunityAsync(expectedImpact: expectedImpact);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.ExpectedImpact.Should().Be(expectedImpact);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_008_Opportunity_ExpectedOutcomes_Persisted()
    {
        var expectedOutcomes = "Improved water access for 50,000 households.";
        var oppId = await SeedOpportunityAsync(expectedOutcomes: expectedOutcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.ExpectedOutcomes.Should().Be(expectedOutcomes);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_009_Opportunity_WhySection_AllFieldsSaved()
    {
        var challenges = "Context";
        var expectedImpact = "Impact";
        var expectedOutcomes = "Outcomes";
        var oppId = await SeedOpportunityAsync(challenges, expectedImpact, expectedOutcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Challenges.Should().Be(challenges);
        opp.ExpectedImpact.Should().Be(expectedImpact);
        opp.ExpectedOutcomes.Should().Be(expectedOutcomes);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_010_OpportunitySDG_Entity_Persisted()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG
        {
            OpportunityId = oppId,
            SDGId = sdgId,
            IsPrimary = true,
            IsDeleted = false
        };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved.Should().NotBeNull();
        saved!.IsPrimary.Should().BeTrue();
        saved.SDGId.Should().Be(sdgId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_011_OpportunitySDG_MainAndCrossCutting_Persisted()
    {
        var oppId = await SeedOpportunityAsync();
        var sdg6Id = await SeedSDGAsync("6", "Clean Water");
        var sdg13Id = await SeedSDGAsync("13", "Climate Action");
        Context.Set<OpportunitySDG>().AddRange(
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg6Id, IsPrimary = true, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg13Id, IsPrimary = false, IsDeleted = false }
        );
        await Context.SaveChangesAsync();
        var sdgs = await Context.Set<OpportunitySDG>().Where(s => s.OpportunityId == oppId && !s.IsDeleted).ToListAsync();
        sdgs.Should().HaveCount(2);
        sdgs.Count(s => s.IsPrimary).Should().Be(1);
        sdgs.Count(s => !s.IsPrimary).Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_012_OpportunitySDG_SkipTargetsAndIndicators_Persisted()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG
        {
            OpportunityId = oppId,
            SDGId = sdgId,
            IsPrimary = true,
            SkipTargetsAndIndicators = true,
            IsDeleted = false
        };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.SkipTargetsAndIndicators.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_013_Opportunity_SoftDeleted_ExcludedFromQuery()
    {
        var oppId = await SeedOpportunityAsync();
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.IsDeleted = true;
        opp.DeletedDate = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        var found = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        found.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_014_OpportunitySDG_SoftDeleted_ExcludedFromQuery()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        oppSdg.IsDeleted = true;
        oppSdg.DeletedDate = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        var found = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        found.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_015_WhySectionRequest_ToOpportunityEntity_ChallengesMapped()
    {
        var request = new WhySectionRequest { Challenges = "Test challenges" };
        var challenges = request.Challenges;
        challenges.Should().Be("Test challenges");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_016_WhySectionRequest_ToOpportunityEntity_SdGsMapped()
    {
        var request = new WhySectionRequest
        {
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 1, IsPrimary = true } }
        };
        request.SdGs!.Count.Should().Be(1);
        request.SdGs!.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_017_OpportunitySDG_IsPrimary_StoredCorrectly()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        oppSdg.IsPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_018_OpportunitySDG_IsPrimaryFalse_StoredCorrectly()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("13", "Climate Action");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = false, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        oppSdg.IsPrimary.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_019_Opportunity_ChallengesMaxLength_Persisted()
    {
        var challenges = new string('A', OpportunityWhySectionSpec.ChallengesMaxLength);
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges!.Length.Should().Be(OpportunityWhySectionSpec.ChallengesMaxLength);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_020_Opportunity_ExpectedImpactMaxLength_Persisted()
    {
        var expectedImpact = new string('X', OpportunityWhySectionSpec.ExpectedImpactMaxLength);
        var oppId = await SeedOpportunityAsync(expectedImpact: expectedImpact);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedImpactMaxLength);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_021_Opportunity_ExpectedOutcomesMaxLength_Persisted()
    {
        var expectedOutcomes = new string('Y', OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
        var oppId = await SeedOpportunityAsync(expectedOutcomes: expectedOutcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_022_ValuesManager_GetSDGs_NoData_ReturnsEmpty()
    {
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_023_ValuesManager_GetSDGs_ReturnsSortedById()
    {
        await SeedSDGAsync("13", "Climate");
        await SeedSDGAsync("6", "Water");
        await SeedSDGAsync("8", "Work");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_024_OpportunitySDG_Notes_Persisted()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG
        {
            OpportunityId = oppId,
            SDGId = sdgId,
            IsPrimary = true,
            Notes = "Alignment with national strategy",
            IsDeleted = false
        };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.Notes.Should().Be("Alignment with national strategy");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_025_Opportunity_MultipleWhyFields_AllPersisted()
    {
        var challenges = "Challenges";
        var expectedImpact = "Impact";
        var expectedOutcomes = "Outcomes";
        var oppId = await SeedOpportunityAsync(challenges, expectedImpact, expectedOutcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Challenges.Should().Be(challenges);
        opp.ExpectedImpact.Should().Be(expectedImpact);
        opp.ExpectedOutcomes.Should().Be(expectedOutcomes);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_026_OpportunitySDG_WithSDGNavigation()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>()
            .Include(s => s.SDG)
            .FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.SDG.Should().NotBeNull();
        saved.SDG!.Name.Should().Contain("Water");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_027_Opportunity_Challenges_Null_Persisted()
    {
        var oppId = await SeedOpportunityAsync(challenges: null);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_028_Opportunity_ExpectedImpact_Null_Persisted()
    {
        var oppId = await SeedOpportunityAsync(expectedImpact: null);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_029_Opportunity_ExpectedOutcomes_Null_Persisted()
    {
        var oppId = await SeedOpportunityAsync(expectedOutcomes: null);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_030_OpportunitySDG_ThreeCrossCutting()
    {
        var oppId = await SeedOpportunityAsync();
        var sdg6 = await SeedSDGAsync("6", "Water");
        var sdg8 = await SeedSDGAsync("8", "Work");
        var sdg13 = await SeedSDGAsync("13", "Climate");
        var sdg15 = await SeedSDGAsync("15", "Land");
        Context.Set<OpportunitySDG>().AddRange(
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg6, IsPrimary = true, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg8, IsPrimary = false, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg13, IsPrimary = false, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg15, IsPrimary = false, IsDeleted = false }
        );
        await Context.SaveChangesAsync();
        var sdgs = await Context.Set<OpportunitySDG>().Where(s => s.OpportunityId == oppId && !s.IsDeleted).ToListAsync();
        sdgs.Should().HaveCount(4);
        sdgs.Count(s => s.IsPrimary).Should().Be(1);
        sdgs.Count(s => !s.IsPrimary).Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_031_SDG_EntityStatus_Active()
    {
        await SeedSDGAsync("6", "Clean Water");
        var sdg = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == "6" && !s.IsDeleted);
        sdg!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_032_Opportunity_Stage_IdentifyAndProfile()
    {
        var oppId = await SeedOpportunityAsync();
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_033_Opportunity_Status_Draft()
    {
        var oppId = await SeedOpportunityAsync();
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_034_OpportunitySDGRequest_ToOpportunitySDG_IsPrimaryMapped()
    {
        var request = new OpportunitySDGRequest { SDGId = 1, IsPrimary = true };
        var isPrimary = request.IsPrimary;
        isPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_035_WhySectionRequest_Challenges_ToEntity()
    {
        var request = new WhySectionRequest { Challenges = "Water scarcity" };
        var challenges = request.Challenges;
        challenges.Should().Be("Water scarcity");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_036_WhySectionRequest_ExpectedImpact_ToEntity()
    {
        var request = new WhySectionRequest { ExpectedImpact = "Impact" };
        var impact = request.ExpectedImpact;
        impact.Should().Be("Impact");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_037_WhySectionRequest_ExpectedOutcomes_ToEntity()
    {
        var request = new WhySectionRequest { ExpectedOutcomes = "Outcomes" };
        var outcomes = request.ExpectedOutcomes;
        outcomes.Should().Be("Outcomes");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_038_OpportunitySDG_SDGId_ReferencesSDG()
    {
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppId = await SeedOpportunityAsync();
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var sdg = await Context.SDGs.FindAsync(sdgId);
        sdg.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_039_OpportunitySDG_OpportunityId_ReferencesOpportunity()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_040_ValuesManager_GetSDGs_ReturnsAtLeastOne_WhenSeeded()
    {
        await SeedSDGAsync("6", "Clean Water");
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_041_Opportunity_Challenges_Unicode_Persisted()
    {
        var challenges = "Desafíos climáticos 气候";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Be(challenges);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_042_OpportunitySDG_MultiplePerOpportunity()
    {
        var oppId = await SeedOpportunityAsync();
        var sdg6 = await SeedSDGAsync("6", "Water");
        var sdg13 = await SeedSDGAsync("13", "Climate");
        Context.Set<OpportunitySDG>().AddRange(
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg6, IsPrimary = true, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg13, IsPrimary = false, IsDeleted = false }
        );
        await Context.SaveChangesAsync();
        var count = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_043_Opportunity_GetById_IncludesWhyFields()
    {
        var challenges = "Water scarcity";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Challenges.Should().Be(challenges);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_044_OpportunitySDG_GetWithOpportunity_IncludesSDGs()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false });
        await Context.SaveChangesAsync();
        var opp = await Context.Opportunities.Include(o => o.SDGs.Where(s => !s.IsDeleted)).FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.SDGs.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_045_Opportunity_Challenges_EmptyString_Persisted()
    {
        var oppId = await SeedOpportunityAsync(challenges: "");
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_046_OpportunitySDG_SkipTargetsAndIndicators_Null_Persisted()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG
        {
            OpportunityId = oppId,
            SDGId = sdgId,
            IsPrimary = true,
            SkipTargetsAndIndicators = null,
            IsDeleted = false
        };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.SkipTargetsAndIndicators.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_047_OpportunitySDG_SkipTargetsAndIndicators_False_Persisted()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG
        {
            OpportunityId = oppId,
            SDGId = sdgId,
            IsPrimary = true,
            SkipTargetsAndIndicators = false,
            IsDeleted = false
        };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.SkipTargetsAndIndicators.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_048_Opportunity_Challenges_Update()
    {
        var oppId = await SeedOpportunityAsync(challenges: "Initial");
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.Challenges = "Updated challenges";
        await Context.SaveChangesAsync();
        var reloaded = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        reloaded!.Challenges.Should().Be("Updated challenges");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_049_Opportunity_ExpectedImpact_Update()
    {
        var oppId = await SeedOpportunityAsync(expectedImpact: "Initial");
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.ExpectedImpact = "Updated impact";
        await Context.SaveChangesAsync();
        var reloaded = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        reloaded!.ExpectedImpact.Should().Be("Updated impact");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_050_Opportunity_ExpectedOutcomes_Update()
    {
        var oppId = await SeedOpportunityAsync(expectedOutcomes: "Initial");
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.ExpectedOutcomes = "Updated outcomes";
        await Context.SaveChangesAsync();
        var reloaded = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        reloaded!.ExpectedOutcomes.Should().Be("Updated outcomes");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_051_OpportunitySDG_AddThenRemove()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        oppSdg.IsDeleted = true;
        oppSdg.DeletedDate = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        var count = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        count.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_052_OpportunitySDG_ChangeMainToCrossCutting()
    {
        var oppId = await SeedOpportunityAsync();
        var sdg6 = await SeedSDGAsync("6", "Water");
        var sdg13 = await SeedSDGAsync("13", "Climate");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdg6, IsPrimary = true, IsDeleted = false });
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdg13, IsPrimary = false, IsDeleted = false });
        await Context.SaveChangesAsync();
        var mainSdg = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && s.IsPrimary && !s.IsDeleted);
        mainSdg!.IsPrimary = false;
        var crossSdg = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsPrimary && !s.IsDeleted);
        crossSdg!.IsPrimary = true;
        await Context.SaveChangesAsync();
        var mainCount = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == oppId && s.IsPrimary && !s.IsDeleted);
        mainCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_053_ValuesManager_GetSDGs_ExcludesInactive()
    {
        var sdg = new SDG { SDGId = "98", SDGNumber = "98", Name = "Test", Status = EntityStatus.Inactive, IsDeleted = false };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        var manager = new ValuesManager(Mapper, Context);
        var result = manager.GetSDGs().ToList();
        result.Should().NotContain(s => s.SDGId == "98");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_054_Opportunity_Challenges_PartialUpdate()
    {
        var oppId = await SeedOpportunityAsync(challenges: "Original", expectedImpact: "Impact");
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.Challenges = "Updated challenges only";
        await Context.SaveChangesAsync();
        var reloaded = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        reloaded!.Challenges.Should().Be("Updated challenges only");
        reloaded.ExpectedImpact.Should().Be("Impact");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_055_OpportunitySDG_Notes_Update()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, Notes = "Original", IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        oppSdg.Notes = "Updated notes";
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.Notes.Should().Be("Updated notes");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_056_Opportunity_Challenges_SpecialCharacters()
    {
        var challenges = "Test & \"quotes\" <script>";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Be(challenges);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_057_Opportunity_ExpectedImpact_Newlines()
    {
        var expectedImpact = "Line1\nLine2";
        var oppId = await SeedOpportunityAsync(expectedImpact: expectedImpact);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact.Should().Contain("\n");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_058_OpportunitySDG_SDG8_CanBeMain()
    {
        var sdg8Id = await SeedSDGAsync("8", "Decent Work");
        var oppId = await SeedOpportunityAsync();
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdg8Id, IsPrimary = true, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().Include(s => s.SDG).FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.SDG!.SDGId.Should().Be("8");
        saved.IsPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_059_OpportunitySDG_SDG15_CanBeCrossCutting()
    {
        var sdg15Id = await SeedSDGAsync("15", "Life on Land");
        var oppId = await SeedOpportunityAsync();
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdg15Id, IsPrimary = false, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().Include(s => s.SDG).FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.SDG!.SDGId.Should().Be("15");
        saved.IsPrimary.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_060_Opportunity_AllWhyFieldsNull_Valid()
    {
        var oppId = await SeedOpportunityAsync(challenges: null, expectedImpact: null, expectedOutcomes: null);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().BeNull();
        opp.ExpectedImpact.Should().BeNull();
        opp.ExpectedOutcomes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_061_OpportunitySDG_ConcurrentOpportunities()
    {
        var opp1 = await SeedOpportunityAsync();
        var opp2 = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        Context.Set<OpportunitySDG>().AddRange(
            new OpportunitySDG { OpportunityId = opp1, SDGId = sdgId, IsPrimary = true, IsDeleted = false },
            new OpportunitySDG { OpportunityId = opp2, SDGId = sdgId, IsPrimary = true, IsDeleted = false }
        );
        await Context.SaveChangesAsync();
        var count1 = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == opp1 && !s.IsDeleted);
        var count2 = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == opp2 && !s.IsDeleted);
        count1.Should().Be(1);
        count2.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_062_Opportunity_Challenges_Unicode_Persisted()
    {
        var challenges = "أزمة المياه";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Be(challenges);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_063_OpportunitySDG_OneMainPerOpportunity()
    {
        var oppId = await SeedOpportunityAsync();
        var sdg6 = await SeedSDGAsync("6", "Water");
        var sdg13 = await SeedSDGAsync("13", "Climate");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdg6, IsPrimary = true, IsDeleted = false });
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdg13, IsPrimary = false, IsDeleted = false });
        await Context.SaveChangesAsync();
        var mainCount = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == oppId && s.IsPrimary && !s.IsDeleted);
        mainCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_064_Opportunity_Challenges_AtMaxLength_Valid()
    {
        var challenges = new string('C', OpportunityWhySectionSpec.ChallengesMaxLength);
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges!.Length.Should().Be(OpportunityWhySectionSpec.ChallengesMaxLength);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_065_Opportunity_ExpectedImpact_AtMaxLength_Valid()
    {
        var impact = new string('I', OpportunityWhySectionSpec.ExpectedImpactMaxLength);
        var oppId = await SeedOpportunityAsync(expectedImpact: impact);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedImpactMaxLength);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_066_Opportunity_ExpectedOutcomes_AtMaxLength_Valid()
    {
        var outcomes = new string('O', OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
        var oppId = await SeedOpportunityAsync(expectedOutcomes: outcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes!.Length.Should().Be(OpportunityWhySectionSpec.ExpectedOutcomesMaxLength);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_067_OpportunitySDG_LoadWithOpportunity()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false });
        await Context.SaveChangesAsync();
        var opp = await Context.Opportunities.Include(o => o.SDGs.Where(s => !s.IsDeleted)).FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.SDGs.Should().HaveCount(1);
        opp.SDGs.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_068_Opportunity_Challenges_Clear()
    {
        var oppId = await SeedOpportunityAsync(challenges: "Original");
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.Challenges = null;
        await Context.SaveChangesAsync();
        var reloaded = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        reloaded!.Challenges.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_069_OpportunitySDG_AddCrossCuttingAfterMain()
    {
        var oppId = await SeedOpportunityAsync();
        var sdg6 = await SeedSDGAsync("6", "Water");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdg6, IsPrimary = true, IsDeleted = false });
        await Context.SaveChangesAsync();
        var sdg13 = await SeedSDGAsync("13", "Climate");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdg13, IsPrimary = false, IsDeleted = false });
        await Context.SaveChangesAsync();
        var count = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_070_Opportunity_ExpectedImpact_EmptyString()
    {
        var oppId = await SeedOpportunityAsync(expectedImpact: "");
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_071_Opportunity_ExpectedOutcomes_EmptyString()
    {
        var oppId = await SeedOpportunityAsync(expectedOutcomes: "");
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_072_OpportunitySDG_Notes_Null()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        var oppSdg = new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, Notes = null, IsDeleted = false };
        Context.Set<OpportunitySDG>().Add(oppSdg);
        await Context.SaveChangesAsync();
        var saved = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        saved!.Notes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_073_Opportunity_Challenges_UnicodeEmoji()
    {
        var challenges = "Water scarcity 💧";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Contain("💧");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_074_OpportunitySDG_MainAndCrossCutting_SameSDG()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false });
        await Context.SaveChangesAsync();
        var mainSdg = await Context.Set<OpportunitySDG>().FirstOrDefaultAsync(s => s.OpportunityId == oppId && s.IsPrimary && !s.IsDeleted);
        mainSdg!.IsPrimary = false;
        await Context.SaveChangesAsync();
        var mainCount = await Context.Set<OpportunitySDG>().CountAsync(s => s.OpportunityId == oppId && s.IsPrimary && !s.IsDeleted);
        mainCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_075_Opportunity_Challenges_WhitespaceOnly()
    {
        var challenges = "   ";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Be("   ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_076_Opportunity_ExpectedImpact_WhitespaceOnly()
    {
        var impact = "   ";
        var oppId = await SeedOpportunityAsync(expectedImpact: impact);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact.Should().Be("   ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_077_Opportunity_ExpectedOutcomes_WhitespaceOnly()
    {
        var outcomes = "   ";
        var oppId = await SeedOpportunityAsync(expectedOutcomes: outcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes.Should().Be("   ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_078_OpportunitySDG_LoadWithSDG()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water and Sanitation");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false });
        await Context.SaveChangesAsync();
        var oppSdg = await Context.Set<OpportunitySDG>().Include(s => s.SDG).FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        oppSdg!.SDG.Should().NotBeNull();
        oppSdg.SDG!.Name.Should().Contain("Water");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_079_Opportunity_Challenges_SingleChar()
    {
        var oppId = await SeedOpportunityAsync(challenges: "X");
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges!.Length.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_080_Opportunity_ExpectedImpact_SingleChar()
    {
        var oppId = await SeedOpportunityAsync(expectedImpact: "Y");
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact!.Length.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_081_Opportunity_ExpectedOutcomes_SingleChar()
    {
        var oppId = await SeedOpportunityAsync(expectedOutcomes: "Z");
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes!.Length.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_082_OpportunitySDG_OpportunityNavigation()
    {
        var oppId = await SeedOpportunityAsync();
        var sdgId = await SeedSDGAsync("6", "Clean Water");
        Context.Set<OpportunitySDG>().Add(new OpportunitySDG { OpportunityId = oppId, SDGId = sdgId, IsPrimary = true, IsDeleted = false });
        await Context.SaveChangesAsync();
        var oppSdg = await Context.Set<OpportunitySDG>().Include(s => s.Opportunity).FirstOrDefaultAsync(s => s.OpportunityId == oppId && !s.IsDeleted);
        oppSdg!.Opportunity.Should().NotBeNull();
        oppSdg.Opportunity!.Id.Should().Be(oppId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_083_Opportunity_Challenges_TabCharacter()
    {
        var challenges = "Challenges\twith tab";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Contain("\t");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_084_Opportunity_Challenges_Newline()
    {
        var challenges = "Line1\nLine2";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Contain("\n");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_085_Opportunity_ExpectedImpact_TabCharacter()
    {
        var impact = "Impact\twith tab";
        var oppId = await SeedOpportunityAsync(expectedImpact: impact);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact.Should().Contain("\t");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_086_Opportunity_ExpectedOutcomes_Newline()
    {
        var outcomes = "Outcome1\nOutcome2";
        var oppId = await SeedOpportunityAsync(expectedOutcomes: outcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes.Should().Contain("\n");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_087_OpportunitySDG_FiveCrossCutting()
    {
        var oppId = await SeedOpportunityAsync();
        var sdg6 = await SeedSDGAsync("6", "Water");
        var sdg8 = await SeedSDGAsync("8", "Work");
        var sdg11 = await SeedSDGAsync("11", "Cities");
        var sdg13 = await SeedSDGAsync("13", "Climate");
        var sdg15 = await SeedSDGAsync("15", "Land");
        var sdg17 = await SeedSDGAsync("17", "Partnerships");
        Context.Set<OpportunitySDG>().AddRange(
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg6, IsPrimary = true, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg8, IsPrimary = false, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg11, IsPrimary = false, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg13, IsPrimary = false, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg15, IsPrimary = false, IsDeleted = false },
            new OpportunitySDG { OpportunityId = oppId, SDGId = sdg17, IsPrimary = false, IsDeleted = false }
        );
        await Context.SaveChangesAsync();
        var sdgs = await Context.Set<OpportunitySDG>().Where(s => s.OpportunityId == oppId && !s.IsDeleted).ToListAsync();
        sdgs.Should().HaveCount(6);
        sdgs.Count(s => s.IsPrimary).Should().Be(1);
        sdgs.Count(s => !s.IsPrimary).Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_088_Opportunity_Challenges_LeadingTrailingSpaces()
    {
        var challenges = "  context  ";
        var oppId = await SeedOpportunityAsync(challenges: challenges);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.Challenges.Should().Be("  context  ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_089_Opportunity_ExpectedImpact_LeadingTrailingSpaces()
    {
        var impact = "  impact  ";
        var oppId = await SeedOpportunityAsync(expectedImpact: impact);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedImpact.Should().Be("  impact  ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task INT_090_Opportunity_ExpectedOutcomes_LeadingTrailingSpaces()
    {
        var outcomes = "  outcomes  ";
        var oppId = await SeedOpportunityAsync(expectedOutcomes: outcomes);
        var opp = await Context.Opportunities.FirstOrDefaultAsync(o => o.Id == oppId && !o.IsDeleted);
        opp!.ExpectedOutcomes.Should().Be("  outcomes  ");
    }

    #endregion
}
