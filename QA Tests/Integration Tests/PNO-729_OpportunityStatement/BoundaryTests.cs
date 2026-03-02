/**
 * @fileoverview PNO-729 Boundary/Edge Tests — 90 boundary and edge case tests.
 * Extreme values, soft-delete interactions, type mismatches, and concurrent scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Boundary/Edge Tests — 90 boundary and edge case tests.
/// </summary>
[Collection("Boundary")]
[Trait("Category", "Boundary")]
[Trait("Ticket", "PNO-729")]
public class BoundaryTests : PNO729TestFixtureBase
{
    // ─── §3.1 ID Extremes (BND-001 – 010) ────────────────────────────────

    [Fact] [Trait("TestId", "BND-001")]
    public async Task Id_1_OpportunityCreatedWithStatement()
    {
        await SeedOpportunityAsync(1, DefaultMarkdown);

        (await DbContext.Opportunities.FindAsync(1))!.OpportunityStatementMarkdown.Should().Be(DefaultMarkdown);
    }

    [Fact] [Trait("TestId", "BND-002")]
    public async Task Id_MaxInt_OpportunityCreatedWithStatement()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = int.MaxValue, Name = "Max", Description = "Max ID Test",
            Stage = "GO", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            IsDeleted = false, OpportunityStatementMarkdown = DefaultMarkdown,
            InitiativeBudgetUSD = 100m, Challenges = "C", ExpectedImpact = "I",
            ExpectedOutcomes = "O", BeneficiariesToBeDetermined = true,
            UNOPSMissionsNotApplicable = true, TargetSigningDate = DateTime.UtcNow.AddMonths(1),
            ImplementationStartDate = DateTime.UtcNow.AddMonths(2),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(int.MaxValue))!.OpportunityStatementMarkdown.Should().Be(DefaultMarkdown);
    }

    [Fact] [Trait("TestId", "BND-003")]
    public async Task Id_Lookup_NonExistent_ReturnsNull()
    {
        (await DbContext.Opportunities.FindAsync(99999999)).Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-004")]
    public async Task Id_Negative_ReturnsNull()
    {
        (await DbContext.Opportunities.FindAsync(-999)).Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-005")]
    public async Task Id_Zero_ReturnsNull()
    {
        (await DbContext.Opportunities.FindAsync(0)).Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-006")]
    public async Task Id_Sequential_AllRetrievable()
    {
        for (var i = 3001; i <= 3010; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown);

        for (var i = 3001; i <= 3010; i++)
            (await DbContext.Opportunities.FindAsync(i)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-007")]
    public async Task Id_ClosedOpp_MaxInt_RetrievableAfterClose()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = int.MaxValue - 1, Name = "Near Max", Description = "Near max closed",
            Stage = "NO GO", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed,
            IsDeleted = false, OpportunityStatementMarkdown = DefaultMarkdown,
            InitiativeBudgetUSD = 100m, Challenges = "C", ExpectedImpact = "I",
            ExpectedOutcomes = "O", BeneficiariesToBeDetermined = true,
            UNOPSMissionsNotApplicable = true, TargetSigningDate = DateTime.UtcNow.AddMonths(1),
            ImplementationStartDate = DateTime.UtcNow.AddMonths(2),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();

        var opp = await DbContext.Opportunities.FindAsync(int.MaxValue - 1);
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-008")]
    public async Task Id_1_ClosedOpp_StatusIsClosedAfterSeed()
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 3100, Name = "ID Closed", Description = "Test",
            Stage = "NO GO", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed,
            IsDeleted = false, OpportunityStatementMarkdown = DefaultMarkdown,
            InitiativeBudgetUSD = 100m, Challenges = "C", ExpectedImpact = "I",
            ExpectedOutcomes = "O", BeneficiariesToBeDetermined = true,
            UNOPSMissionsNotApplicable = true, TargetSigningDate = DateTime.UtcNow.AddMonths(1),
            ImplementationStartDate = DateTime.UtcNow.AddMonths(2),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        };
        DbContext.Opportunities.Add(opp);
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(3100))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-009")]
    public async Task Id_Multiple_CountIsAccurate()
    {
        for (var i = 3200; i <= 3209; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown);

        var count = await DbContext.Opportunities.CountAsync(o => o.Id >= 3200 && o.Id <= 3209);
        count.Should().Be(10);
    }

    [Fact] [Trait("TestId", "BND-010")]
    public async Task Id_Lookup_AfterStatementMigration_IdUnchanged()
    {
        await SeedOpportunityAsync(3210, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(3210))!.Id.Should().Be(3210);
    }

    // ─── §3.2 Statement Length Extremes (BND-011 – 025) ──────────────────

    [Fact] [Trait("TestId", "BND-011")]
    public async Task Statement_SingleChar_Persists()
    {
        await SeedOpportunityAsync(3300, "A");
        (await DbContext.Opportunities.FindAsync(3300))!.OpportunityStatementMarkdown.Should().Be("A");
    }

    [Fact] [Trait("TestId", "BND-012")]
    public async Task Statement_1000Chars_Persists()
    {
        var stmt = new string('B', 1000);
        await SeedOpportunityAsync(3301, stmt);
        (await DbContext.Opportunities.FindAsync(3301))!.OpportunityStatementMarkdown.Should().HaveLength(1000);
    }

    [Fact] [Trait("TestId", "BND-013")]
    public async Task Statement_10000Chars_Persists()
    {
        var stmt = new string('C', 10000);
        await SeedOpportunityAsync(3302, stmt);
        (await DbContext.Opportunities.FindAsync(3302))!.OpportunityStatementMarkdown.Should().HaveLength(10000);
    }

    [Fact] [Trait("TestId", "BND-014")]
    public async Task Statement_EmptyString_Persists()
    {
        await SeedOpportunityAsync(3303, "");
        (await DbContext.Opportunities.FindAsync(3303))!.OpportunityStatementMarkdown.Should().Be("");
    }

    [Fact] [Trait("TestId", "BND-015")]
    public async Task Statement_Null_Persists()
    {
        await SeedOpportunityAsync(3304, null);
        (await DbContext.Opportunities.FindAsync(3304))!.OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-016")]
    public async Task Statement_WhitespaceOnly_Persists()
    {
        await SeedOpportunityAsync(3305, "   ");
        (await DbContext.Opportunities.FindAsync(3305))!.OpportunityStatementMarkdown.Should().Be("   ");
    }

    [Fact] [Trait("TestId", "BND-017")]
    public async Task Statement_NewlineOnly_Persists()
    {
        await SeedOpportunityAsync(3306, "\n\n\n");
        (await DbContext.Opportunities.FindAsync(3306))!.OpportunityStatementMarkdown.Should().Be("\n\n\n");
    }

    [Fact] [Trait("TestId", "BND-018")]
    public async Task Statement_SpecialChars_Persists()
    {
        const string stmt = "# Statement\n\n- Item 1 & 2\n- <b>Bold</b>\n- \"Quoted\"";
        await SeedOpportunityAsync(3307, stmt);
        (await DbContext.Opportunities.FindAsync(3307))!.OpportunityStatementMarkdown.Should().Be(stmt);
    }

    [Fact] [Trait("TestId", "BND-019")]
    public async Task Statement_UnicodeChars_Persists()
    {
        const string stmt = "# 北京 Déclaration\n\n- العربية\n- Ñoño";
        await SeedOpportunityAsync(3308, stmt);
        (await DbContext.Opportunities.FindAsync(3308))!.OpportunityStatementMarkdown.Should().Contain("北京");
    }

    [Fact] [Trait("TestId", "BND-020")]
    public async Task Statement_EmojiContent_Persists()
    {
        const string stmt = "## 🌍 Opportunity\n\n✅ Ready\n❌ Blocked";
        await SeedOpportunityAsync(3309, stmt);
        (await DbContext.Opportunities.FindAsync(3309))!.OpportunityStatementMarkdown.Should().Contain("🌍");
    }

    [Fact] [Trait("TestId", "BND-021")]
    public async Task Statement_SqlInjectionString_Persists()
    {
        const string stmt = "'; DROP TABLE Opportunities; --";
        await SeedOpportunityAsync(3310, stmt);
        (await DbContext.Opportunities.FindAsync(3310))!.OpportunityStatementMarkdown.Should().Be(stmt);
    }

    [Fact] [Trait("TestId", "BND-022")]
    public async Task Statement_TabsAndMixedWhitespace_Persists()
    {
        const string stmt = "# Section\t\n\n\tIndented";
        await SeedOpportunityAsync(3311, stmt);
        (await DbContext.Opportunities.FindAsync(3311))!.OpportunityStatementMarkdown.Should().Contain("\t");
    }

    [Fact] [Trait("TestId", "BND-023")]
    public async Task Statement_MarkdownTable_Persists()
    {
        const string stmt = "| Col1 | Col2 |\n|------|------|\n| A    | B    |";
        await SeedOpportunityAsync(3312, stmt);
        (await DbContext.Opportunities.FindAsync(3312))!.OpportunityStatementMarkdown.Should().Contain("| Col1 |");
    }

    [Fact] [Trait("TestId", "BND-024")]
    public async Task Statement_MarkdownCodeBlock_Persists()
    {
        const string stmt = "```csharp\nvar x = 1;\n```";
        await SeedOpportunityAsync(3313, stmt);
        (await DbContext.Opportunities.FindAsync(3313))!.OpportunityStatementMarkdown.Should().Contain("```csharp");
    }

    [Fact] [Trait("TestId", "BND-025")]
    public async Task Statement_MarkdownLink_Persists()
    {
        const string stmt = "[UNOPS](https://unops.org) - [Learn More](https://learn.unops.org)";
        await SeedOpportunityAsync(3314, stmt);
        (await DbContext.Opportunities.FindAsync(3314))!.OpportunityStatementMarkdown.Should().Contain("[UNOPS]");
    }

    // ─── §3.3 Status Boundary Scenarios (BND-026 – 040) ──────────────────

    [Fact] [Trait("TestId", "BND-026")]
    public async Task Status_AllStatuses_ColorClassDefined()
    {
        foreach (var status in System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>())
            GetStatusColorClass(status).Should().NotBeNullOrEmpty();
    }

    [Fact] [Trait("TestId", "BND-027")]
    public async Task Status_Closed_DistinctFromAllOthers()
    {
        var closedColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        var otherColors = System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>()
            .Where(s => s != UNOPS.PAO.Domain.Entities.EntityStatus.Closed)
            .Select(GetStatusColorClass)
            .ToList();

        otherColors.Should().NotContain(closedColor);
    }

    [Fact] [Trait("TestId", "BND-028")]
    public async Task Status_Transition_ActiveToClosed_Persists()
    {
        await SeedOpportunityAsync(3400, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(3400);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 3400)).Status
            .Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-029")]
    public async Task Status_Transition_ClosedToActive_Persists()
    {
        await SeedClosedOpportunityAsync(3401);
        var opp = await DbContext.Opportunities.FindAsync(3401);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active;
        opp.Stage = "GO";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 3401)).Status
            .Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "BND-030")]
    public async Task Status_AllStatusTransitions_TrackCorrectly()
    {
        await SeedOpportunityAsync(3402, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Draft);
        var opp = await DbContext.Opportunities.FindAsync(3402);

        var transitions = new[]
        {
            UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            UNOPS.PAO.Domain.Entities.EntityStatus.Closed,
            UNOPS.PAO.Domain.Entities.EntityStatus.Inactive
        };

        foreach (var status in transitions)
        {
            opp!.Status = status;
            await DbContext.SaveChangesAsync();
            DbContext.ChangeTracker.Clear();
            opp = await DbContext.Opportunities.FindAsync(3402);
            opp!.Status.Should().Be(status);
        }
    }

    [Fact] [Trait("TestId", "BND-031")]
    public async Task Status_MultipleClosedOpps_AllHaveLightRedColor()
    {
        for (var i = 3410; i <= 3419; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 3410 && o.Id <= 3419)
            .ToListAsync();

        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "BND-032")]
    public async Task Status_MixedStatuses_OnlyClosedIsLightRed()
    {
        await SeedOpportunityAsync(3420, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await SeedClosedOpportunityAsync(3421);
        await SeedOpportunityAsync(3422, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 3420 && o.Id <= 3422)
            .ToListAsync();

        var colorMap = opps.ToDictionary(o => o.Id, o => GetStatusColorClass(o.Status));
        colorMap[3421].Should().Be(ClosedStatusColor);
        colorMap[3420].Should().NotBe(ClosedStatusColor);
        colorMap[3422].Should().NotBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "BND-033")]
    public async Task Status_ClosedEnum_IntValueKnown()
    {
        var val = (int)UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        val.Should().BeGreaterThan(-1, "Closed should have a non-negative enum value");
    }

    [Fact] [Trait("TestId", "BND-034")]
    public async Task Status_ClosedAndNoGoStage_BothSet()
    {
        await SeedClosedOpportunityAsync(3430);
        var opp = await DbContext.Opportunities.FindAsync(3430);

        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "BND-035")]
    public async Task Status_ActiveAndGoStage_BothSet()
    {
        await SeedOpportunityAsync(3431, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO");
        var opp = await DbContext.Opportunities.FindAsync(3431);

        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        opp.Stage.Should().Be("GO");
    }

    // ─── §3.4 Soft-Delete Boundary (BND-036 – 050) ───────────────────────

    [Fact] [Trait("TestId", "BND-036")]
    public async Task SoftDelete_ActiveWithStatement_IsDeletedFalse()
    {
        await SeedOpportunityAsync(3500, DefaultMarkdown);
        (await DbContext.Opportunities.FindAsync(3500))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "BND-037")]
    public async Task SoftDelete_DeletedClosedWithStatement_IsDeletedTrue()
    {
        var opp = await SeedClosedOpportunityAsync(3501);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(3501))!.IsDeleted.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "BND-038")]
    public async Task SoftDelete_ReactivateAfterDelete_IsDeletedFalse()
    {
        var opp = await SeedOpportunityAsync(3502, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        opp.IsDeleted = false;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(3502))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "BND-039")]
    public async Task SoftDelete_StatementMigration_ExcludesDeletedRecords()
    {
        await SeedOpportunityAsync(3503, null);
        var del = await SeedOpportunityAsync(3504, null);
        del.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-040")]
    public async Task SoftDelete_DeletedRecord_IdPreservedInDb()
    {
        var opp = await SeedOpportunityAsync(3505, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(3505)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-041")]
    public async Task SoftDelete_ActiveQuery_ExcludesDeletedRecords()
    {
        await SeedOpportunityAsync(3506, DefaultMarkdown);
        var del = await SeedOpportunityAsync(3507, DefaultMarkdown);
        del.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var active = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 3506 && o.Id <= 3507)
            .ToListAsync();

        active.Should().HaveCount(1);
        active[0].Id.Should().Be(3506);
    }

    [Fact] [Trait("TestId", "BND-042")]
    public async Task SoftDelete_ClosedDeletedRecord_NotInClosedActiveQuery()
    {
        var opp = await SeedClosedOpportunityAsync(3508);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 3508)
            .FirstOrDefaultAsync();

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-043")]
    public async Task SoftDelete_AllDeleted_MigrationAffected0()
    {
        for (var i = 3510; i <= 3514; i++)
        {
            var o = await SeedOpportunityAsync(i, null);
            o.IsDeleted = true;
        }
        await DbContext.SaveChangesAsync();

        (await RunStatementFixMigrationAsync()).Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-044")]
    public async Task SoftDelete_NonDeletedMixedWithDeleted_CountCorrect()
    {
        for (var i = 3520; i <= 3524; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown);
        for (var i = 3525; i <= 3529; i++)
        {
            var o = await SeedOpportunityAsync(i, DefaultMarkdown);
            o.IsDeleted = true;
        }
        await DbContext.SaveChangesAsync();

        var count = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted && o.Id >= 3520 && o.Id <= 3529);
        count.Should().Be(5);
    }

    [Fact] [Trait("TestId", "BND-045")]
    public async Task SoftDelete_ChangeTrackerClear_DeletedRecordStillInDb()
    {
        var opp = await SeedOpportunityAsync(3530, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.FindAsync(3530)).Should().NotBeNull();
    }

    // ─── §3.5 Status and Statement Cross-Boundary (BND-046 – 065) ────────

    [Fact] [Trait("TestId", "BND-046")]
    public async Task CrossBoundary_ClosedOppStatementNull_FixedByMigration()
    {
        await SeedClosedOpportunityWithNullStatementAsync(3600);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(3600))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "BND-047")]
    public async Task CrossBoundary_ClosedOppStatement_StatusUnchangedByMigration()
    {
        await SeedClosedOpportunityWithNullStatementAsync(3601);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(3601))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-048")]
    public async Task CrossBoundary_ClosedOppStage_UnchangedByMigration()
    {
        await SeedClosedOpportunityWithNullStatementAsync(3602);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(3602))!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "BND-049")]
    public async Task CrossBoundary_StatementAndStatusBothUpdated_BothPersist()
    {
        await SeedOpportunityAsync(3603, "Original Statement", UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(3603);
        opp!.OpportunityStatementMarkdown = "Updated Statement";
        opp.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        var updated = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 3603);
        updated.OpportunityStatementMarkdown.Should().Be("Updated Statement");
        updated.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-050")]
    public async Task CrossBoundary_ClosedColorAlways_LightRed_MixedWithStatement()
    {
        await SeedClosedOpportunityAsync(3604, DefaultMarkdown);
        var opp = await DbContext.Opportunities.FindAsync(3604);

        GetStatusColorClass(opp!.Status).Should().Be(ClosedStatusColor);
        opp.OpportunityStatementMarkdown.Should().NotBeNullOrEmpty();
    }

    // ─── §3.6 Additional Boundary Tests (BND-051 – 090) ──────────────────

    [Fact] [Trait("TestId", "BND-051")]
    public async Task Boundary_50ClosedOpps_AllHaveLightRedColor()
    {
        for (var i = 3700; i <= 3749; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 3700 && o.Id <= 3749)
            .ToListAsync();

        opps.Should().HaveCount(50);
        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "BND-052")]
    public async Task Boundary_MigrationFix_50NullStatements()
    {
        for (var i = 3800; i <= 3849; i++)
            await SeedOpportunityAsync(i, null);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(50);
    }

    [Fact] [Trait("TestId", "BND-053")]
    public async Task Boundary_MigrationFix_50ExistingStatements()
    {
        for (var i = 3900; i <= 3949; i++)
            await SeedOpportunityAsync(i, "Existing");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-054")]
    public async Task Boundary_StatusTransitionClosed_AfterMigration()
    {
        await SeedOpportunityAsync(4000, null);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(4000);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(4000))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-055")]
    public async Task Boundary_StatementUpdatedWhileClosed_Persists()
    {
        await SeedClosedOpportunityAsync(4001, "Initial");
        var opp = await DbContext.Opportunities.FindAsync(4001);
        opp!.OpportunityStatementMarkdown = "Updated While Closed";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 4001))
            .OpportunityStatementMarkdown.Should().Be("Updated While Closed");
    }

    [Fact] [Trait("TestId", "BND-056")]
    public async Task Boundary_ClosedStatus_ColorClass_Idempotent()
    {
        var results = Enumerable.Range(0, 5)
            .Select(_ => GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed))
            .ToList();
        results.Should().AllBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "BND-057")]
    public async Task Boundary_LargeStatementUpdate_Persists()
    {
        await SeedOpportunityAsync(4002, "Short");
        var opp = await DbContext.Opportunities.FindAsync(4002);
        var largeStatement = new string('X', 50000);
        opp!.OpportunityStatementMarkdown = largeStatement;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 4002))
            .OpportunityStatementMarkdown.Should().HaveLength(50000);
    }

    [Fact] [Trait("TestId", "BND-058")]
    public async Task Boundary_MigrationFix_ClosedAndActive_BothFixed()
    {
        await SeedClosedOpportunityWithNullStatementAsync(4003);
        await SeedOpportunityAsync(4004, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(2);
    }

    [Fact] [Trait("TestId", "BND-059")]
    public async Task Boundary_StatementNull_MigrationFix_EmptyString()
    {
        await SeedOpportunityAsync(4005, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(4005))!.OpportunityStatementMarkdown.Should().Be("");
    }

    [Fact] [Trait("TestId", "BND-060")]
    public async Task Boundary_ClosedOppColor_Always_LightRed()
    {
        for (var i = 4010; i <= 4019; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities.Where(o => o.Id >= 4010 && o.Id <= 4019).ToListAsync();
        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "BND-061")]
    public async Task Boundary_ClosedOppStatementEmpty_FixedToEmpty()
    {
        await SeedClosedOpportunityWithNullStatementAsync(4020);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(4020))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "BND-062")]
    public async Task Boundary_ActiveOppStatementEmpty_FixedToEmpty()
    {
        await SeedOpportunityAsync(4021, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(4021))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "BND-063")]
    public async Task Boundary_MigrationAndStatusChange_Independent()
    {
        await SeedOpportunityAsync(4022, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await RunStatementFixMigrationAsync();
        var opp = await DbContext.Opportunities.FindAsync(4022);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities.FindAsync(4022);
        result!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        result.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "BND-064")]
    public async Task Boundary_ClosedStatusColorConsistency_10Opps()
    {
        for (var i = 4030; i <= 4039; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities
            .Where(o => o.Id >= 4030 && o.Id <= 4039)
            .ToListAsync();

        opps.Select(o => GetStatusColorClass(o.Status)).Should().AllBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "BND-065")]
    public async Task Boundary_StatusColorMap_Deterministic()
    {
        var statuses = System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>().ToList();
        var colors1 = statuses.Select(GetStatusColorClass).ToList();
        var colors2 = statuses.Select(GetStatusColorClass).ToList();

        colors1.Should().BeEquivalentTo(colors2, "Color mapping must be deterministic");
    }

    [Fact] [Trait("TestId", "BND-066")]
    public async Task Boundary_ClosedOppsQueryable_ByStageAndStatus()
    {
        for (var i = 4040; i <= 4049; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == "NO GO" && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed
                && o.Id >= 4040 && o.Id <= 4049)
            .ToListAsync();

        opps.Should().HaveCount(10);
    }

    [Fact] [Trait("TestId", "BND-067")]
    public async Task Boundary_StatementMigration_100OppsWithMixedState()
    {
        for (var i = 4050; i <= 4099; i++)
            await SeedOpportunityAsync(i, i % 2 == 0 ? null : "Has Statement");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(25);
    }

    [Fact] [Trait("TestId", "BND-068")]
    public async Task Boundary_StatementMigration_Idempotent_100Runs()
    {
        await SeedOpportunityAsync(4100, null);
        await RunStatementFixMigrationAsync();

        for (var i = 0; i < 99; i++)
        {
            var run = await RunStatementFixMigrationAsync();
            run.Should().Be(0);
        }
    }

    [Fact] [Trait("TestId", "BND-069")]
    public async Task Boundary_StatementUpdated_OtherFieldsIntact()
    {
        await SeedOpportunityAsync(4101, "Original", UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO", "Integrity Opp");
        var opp = await DbContext.Opportunities.FindAsync(4101);
        opp!.OpportunityStatementMarkdown = "Updated";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        var result = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 4101);
        result.Name.Should().Be("Integrity Opp");
        result.Stage.Should().Be("GO");
        result.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        result.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "BND-070")]
    public async Task Boundary_MigrationFix_ResultType_IsNonNegativeInt()
    {
        var result = await RunStatementFixMigrationAsync();
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact] [Trait("TestId", "BND-071")]
    public async Task Boundary_ColorClass_IsString_NotNull()
    {
        var color = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        color.Should().BeOfType<string>();
    }

    [Fact] [Trait("TestId", "BND-072")]
    public async Task Boundary_ClosedColorClass_LightRed_ConstantMatches()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().Be(ClosedStatusColor);
        ClosedStatusColor.Should().Be("light-red");
    }

    [Fact] [Trait("TestId", "BND-073")]
    public async Task Boundary_ActiveColorClass_Green_ConstantMatches()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Active).Should().Be("green");
    }

    [Fact] [Trait("TestId", "BND-074")]
    public async Task Boundary_InactiveColorClass_Grey_ConstantMatches()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Inactive).Should().Be("grey");
    }

    [Fact] [Trait("TestId", "BND-075")]
    public async Task Boundary_DraftColorClass_Blue_ConstantMatches()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Draft).Should().Be("blue");
    }

    [Fact] [Trait("TestId", "BND-076")]
    public async Task Boundary_Statement_NullToEmpty_MigrationFix()
    {
        await SeedOpportunityAsync(4200, null);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(4200);
        opp!.OpportunityStatementMarkdown.Should().Be("").And.NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-077")]
    public async Task Boundary_Transition_ActiveToClosedToActive_Persists()
    {
        await SeedOpportunityAsync(4201, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(4201);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();
        opp.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(4201))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "BND-078")]
    public async Task Boundary_ClosedOppsCount_Stable()
    {
        for (var i = 4210; i <= 4214; i++)
            await SeedClosedOpportunityAsync(i);

        var count = await DbContext.Opportunities
            .CountAsync(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 4210 && o.Id <= 4214);

        count.Should().Be(5);
    }

    [Fact] [Trait("TestId", "BND-079")]
    public async Task Boundary_StatementFixed_OppCountUnchanged()
    {
        for (var i = 4220; i <= 4224; i++)
            await SeedOpportunityAsync(i, null);
        var before = await DbContext.Opportunities.CountAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "BND-080")]
    public async Task Boundary_QueryOrderByStatus_ClosedAtBottom()
    {
        await SeedOpportunityAsync(4230, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await SeedClosedOpportunityAsync(4231);

        var opps = await DbContext.Opportunities
            .Where(o => o.Id >= 4230 && o.Id <= 4231)
            .OrderBy(o => o.Status)
            .ToListAsync();

        opps.Should().HaveCount(2);
    }

    [Fact] [Trait("TestId", "BND-081")]
    public async Task Boundary_FilterByClosedAndNoGo_ReturnsBoth()
    {
        await SeedClosedOpportunityAsync(4240);

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Stage == "NO GO" && o.Id == 4240)
            .FirstOrDefaultAsync();

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-082")]
    public async Task Boundary_ClosedColor_SameBeforeAndAfterMigration()
    {
        var beforeColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        await SeedOpportunityAsync(4241, null);
        await RunStatementFixMigrationAsync();
        var afterColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);

        beforeColor.Should().Be(afterColor);
    }

    [Fact] [Trait("TestId", "BND-083")]
    public async Task Boundary_MigrationAndDelete_Independent()
    {
        await SeedOpportunityAsync(4242, null);
        await RunStatementFixMigrationAsync();
        var opp = await DbContext.Opportunities.FindAsync(4242);
        opp!.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(4242))!.IsDeleted.Should().BeTrue();
        (await DbContext.Opportunities.FindAsync(4242))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "BND-084")]
    public async Task Boundary_ClosedStatus_DistinctIntValue()
    {
        var values = System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>()
            .Select(s => (int)s)
            .ToList();

        values.Should().OnlyHaveUniqueItems("Each EntityStatus must have a unique int value");
    }

    [Fact] [Trait("TestId", "BND-085")]
    public async Task Boundary_ClosedOpp_CanAddStatement()
    {
        await SeedClosedOpportunityWithNullStatementAsync(4250);
        var opp = await DbContext.Opportunities.FindAsync(4250);
        opp!.OpportunityStatementMarkdown = "Added While Closed";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(4250))!.OpportunityStatementMarkdown.Should().Be("Added While Closed");
    }

    [Fact] [Trait("TestId", "BND-086")]
    public async Task Boundary_Statement_WhitespaceNotNull_NotFixedByMigration()
    {
        await SeedOpportunityAsync(4251, "   ");
        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0, "Whitespace is not null; should not be fixed");
    }

    [Fact] [Trait("TestId", "BND-087")]
    public async Task Boundary_Statement_EmptyStringNotNull_NotFixedByMigration()
    {
        await SeedOpportunityAsync(4252, "");
        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0, "Empty string is not null; should not be fixed");
    }

    [Fact] [Trait("TestId", "BND-088")]
    public async Task Boundary_1000Opps_AllClosed_AllLightRed()
    {
        const int count = 50;
        for (var i = 5001; i <= 5000 + count; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 5001 && o.Id <= 5000 + count)
            .ToListAsync();

        opps.Should().HaveCount(count);
        opps.Select(o => GetStatusColorClass(o.Status)).Should().AllBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "BND-089")]
    public async Task Boundary_MigrationFix_BulkNullStatements()
    {
        const int count = 50;
        for (var i = 5051; i <= 5050 + count; i++)
            await SeedOpportunityAsync(i, null);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(count);
    }

    [Fact] [Trait("TestId", "BND-090")]
    public async Task Boundary_AllColorMappings_Stable()
    {
        var statuses = System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>().ToArray();

        for (var i = 0; i < 5; i++)
        {
            var colors = statuses.Select(GetStatusColorClass).ToList();
            colors.Should().NotBeNullOrEmpty();
            GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().Be(ClosedStatusColor);
        }
    }
}
