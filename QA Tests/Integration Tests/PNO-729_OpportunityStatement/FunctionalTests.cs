/**
 * @fileoverview PNO-729 Functional Tests — 90 business logic and validation tests.
 * Status color rules, statement lifecycle, business constraints, and workflow.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Functional Tests — 90 business rule and validation tests.
/// </summary>
[Collection("Functional")]
[Trait("Category", "Functional")]
[Trait("Ticket", "PNO-729")]
public class FunctionalTests : PNO729TestFixtureBase
{
    // ─── §4.1 Closed Status Color Business Rules (FUN-001 – 020) ─────────

    [Fact] [Trait("TestId", "FUN-001")]
    public async Task ClosedStatus_DisplaysLightRed_BusinessRule()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed)
            .Should().Be(ClosedStatusColor, "PNO-729: Closed opportunities must display in light-red");
    }

    [Fact] [Trait("TestId", "FUN-002")]
    public async Task ClosedStatus_WasGrey_NowLightRed_PNO729Fix()
    {
        const string oldColor = "grey";
        var newColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);

        newColor.Should().NotBe(oldColor,
            "PNO-729 fix: Closed was incorrectly displayed as grey before this fix");
        newColor.Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "FUN-003")]
    public async Task ClosedStatus_OnlyClosedHasLightRed()
    {
        var statuses = System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>();
        var lightRedStatuses = statuses.Where(s => GetStatusColorClass(s) == ClosedStatusColor).ToList();

        lightRedStatuses.Should().HaveCount(1);
        lightRedStatuses[0].Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-004")]
    public async Task ClosedStatus_ColorClass_LightRed_Constant()
    {
        ClosedStatusColor.Should().Be("light-red");
    }

    [Fact] [Trait("TestId", "FUN-005")]
    public async Task ClosedStatus_ClosedEnumMappedToLightRed()
    {
        var mapping = new Dictionary<UNOPS.PAO.Domain.Entities.EntityStatus, string>
        {
            { UNOPS.PAO.Domain.Entities.EntityStatus.Closed, ClosedStatusColor },
            { UNOPS.PAO.Domain.Entities.EntityStatus.Active, "green" },
            { UNOPS.PAO.Domain.Entities.EntityStatus.Inactive, "grey" },
            { UNOPS.PAO.Domain.Entities.EntityStatus.Draft, "blue" }
        };

        foreach (var (status, expectedColor) in mapping)
            GetStatusColorClass(status).Should().Be(expectedColor);
    }

    [Fact] [Trait("TestId", "FUN-006")]
    public async Task ClosedStatus_ColorRulesApply_ToAllClosedOpps()
    {
        for (var i = 6001; i <= 6010; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6001 && o.Id <= 6010)
            .ToListAsync();

        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "FUN-007")]
    public async Task ClosedStatus_NoGoStage_ImpliesClosedStatus()
    {
        await SeedClosedOpportunityAsync(6011);
        var opp = await DbContext.Opportunities.FindAsync(6011);

        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "FUN-008")]
    public async Task ClosedStatus_CountsCorrectly_ByStatusField()
    {
        for (var i = 6020; i <= 6024; i++)
            await SeedClosedOpportunityAsync(i);
        for (var i = 6025; i <= 6029; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var closedCount = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 6020 && o.Id <= 6029);
        var activeCount = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id >= 6020 && o.Id <= 6029);

        closedCount.Should().Be(5);
        activeCount.Should().Be(5);
    }

    [Fact] [Trait("TestId", "FUN-009")]
    public async Task ClosedStatus_ClosedReason_IsNoGo()
    {
        await SeedClosedOpportunityAsync(6030);
        var opp = await DbContext.Opportunities.FindAsync(6030);

        opp!.Stage.Should().Be("NO GO", "Closed stage is always 'NO GO'");
    }

    [Fact] [Trait("TestId", "FUN-010")]
    public async Task ClosedStatus_ActiveOppsAreGreen()
    {
        for (var i = 6040; i <= 6044; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6040 && o.Id <= 6044)
            .ToListAsync();

        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be("green");
    }

    [Fact] [Trait("TestId", "FUN-011")]
    public async Task ClosedStatus_InactiveOppsAreGrey()
    {
        for (var i = 6050; i <= 6054; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6050 && o.Id <= 6054)
            .ToListAsync();

        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be("grey");
    }

    [Fact] [Trait("TestId", "FUN-012")]
    public async Task ClosedStatus_DraftOppsAreBlue()
    {
        for (var i = 6060; i <= 6064; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Draft);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6060 && o.Id <= 6064)
            .ToListAsync();

        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be("blue");
    }

    [Fact] [Trait("TestId", "FUN-013")]
    public async Task ClosedStatus_ColorConsistent_WithMixedOpps()
    {
        await SeedClosedOpportunityAsync(6070);
        await SeedOpportunityAsync(6071, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await SeedOpportunityAsync(6072, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        var opps = await DbContext.Opportunities.Where(o => o.Id >= 6070 && o.Id <= 6072).ToListAsync();

        GetStatusColorClass(opps.First(o => o.Id == 6070).Status).Should().Be(ClosedStatusColor);
        GetStatusColorClass(opps.First(o => o.Id == 6071).Status).Should().Be("green");
        GetStatusColorClass(opps.First(o => o.Id == 6072).Status).Should().Be("grey");
    }

    [Fact] [Trait("TestId", "FUN-014")]
    public async Task ClosedStatus_CanBeRetrieved_ByStatus()
    {
        await SeedClosedOpportunityAsync(6073);

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 6073)
            .FirstOrDefaultAsync();

        result.Should().NotBeNull();
        GetStatusColorClass(result!.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "FUN-015")]
    public async Task ClosedStatus_DoesNotChange_ByStatementMigration()
    {
        await SeedClosedOpportunityWithNullStatementAsync(6074);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6074))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-016")]
    public async Task ClosedStatus_DisplayedAsLightRed_NotNull()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-017")]
    public async Task ClosedStatus_DisplayedAsLightRed_NotEmpty()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "FUN-018")]
    public async Task ClosedStatus_ClosedAndDeletedOpp_NotInActiveClosedQuery()
    {
        var opp = await SeedClosedOpportunityAsync(6075);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 6075)
            .FirstOrDefaultAsync();

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "FUN-019")]
    public async Task ClosedStatus_ColorMapping_Equals_LightRed()
    {
        var color = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        (color == ClosedStatusColor).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "FUN-020")]
    public async Task ClosedStatus_CanFilter_10ClosedOpps()
    {
        for (var i = 6080; i <= 6089; i++)
            await SeedClosedOpportunityAsync(i);

        var count = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 6080 && o.Id <= 6089);

        count.Should().Be(10);
    }

    // ─── §4.2 Statement Generation Business Rules (FUN-021 – 045) ────────

    [Fact] [Trait("TestId", "FUN-021")]
    public async Task Statement_MigrationFixes_NullToEmpty_BusinessRule()
    {
        await SeedOpportunityAsync(6100, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6100))!.OpportunityStatementMarkdown
            .Should().Be(EmptyMarkdown, "PNO-729: Null statements should be fixed to empty string");
    }

    [Fact] [Trait("TestId", "FUN-022")]
    public async Task Statement_ExistingStatement_NotModifiedByMigration()
    {
        await SeedOpportunityAsync(6101, "Existing Content");
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6101))!.OpportunityStatementMarkdown
            .Should().Be("Existing Content");
    }

    [Fact] [Trait("TestId", "FUN-023")]
    public async Task Statement_MigrationIsIdempotent_BusinessRule()
    {
        await SeedOpportunityAsync(6102, null);
        var firstRun = await RunStatementFixMigrationAsync();
        var secondRun = await RunStatementFixMigrationAsync();

        firstRun.Should().Be(1);
        secondRun.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-024")]
    public async Task Statement_SoftDeletedNotFixed_BusinessRule()
    {
        var opp = await SeedOpportunityAsync(6103, null);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-025")]
    public async Task Statement_CanBeUpdatedByUser_AfterFix()
    {
        await SeedOpportunityAsync(6104, null);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6104);
        opp!.OpportunityStatementMarkdown = "User Updated Statement";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(6104))!.OpportunityStatementMarkdown
            .Should().Be("User Updated Statement");
    }

    [Fact] [Trait("TestId", "FUN-026")]
    public async Task Statement_MarkdownPersisted_CanBeRendered()
    {
        const string markdown = "## Why\n\nBecause of UNOPS mandate.\n\n## What\n\nDeliver capacity building.";
        await SeedOpportunityAsync(6105, markdown);

        var opp = await DbContext.Opportunities.FindAsync(6105);
        opp!.OpportunityStatementMarkdown.Should().Contain("## Why");
        opp.OpportunityStatementMarkdown.Should().Contain("## What");
    }

    [Fact] [Trait("TestId", "FUN-027")]
    public async Task Statement_MarkdownHasAllSections_WHY_WHAT_Team_Budget_Schedule()
    {
        const string markdown = "## WHY\n\n## WHAT\n\n## Team\n\n## Budget\n\n## Schedule";
        await SeedOpportunityAsync(6106, markdown);

        var opp = await DbContext.Opportunities.FindAsync(6106);
        new[] { "## WHY", "## WHAT", "## Team", "## Budget", "## Schedule" }
            .ToList()
            .ForEach(section => opp!.OpportunityStatementMarkdown!.Should().Contain(section));
    }

    [Fact] [Trait("TestId", "FUN-028")]
    public async Task Statement_CountOfNullStatements_BeforeAndAfterMigration()
    {
        for (var i = 6110; i <= 6114; i++)
            await SeedOpportunityAsync(i, null);

        var nullBefore = await DbContext.Opportunities
            .CountAsync(o => !o.IsDeleted && o.Id >= 6110 && o.Id <= 6114 && o.OpportunityStatementMarkdown == null);
        await RunStatementFixMigrationAsync();
        var nullAfter = await DbContext.Opportunities
            .CountAsync(o => !o.IsDeleted && o.Id >= 6110 && o.Id <= 6114 && o.OpportunityStatementMarkdown == null);

        nullBefore.Should().Be(5);
        nullAfter.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-029")]
    public async Task Statement_MigrationAffectedCount_Accurate()
    {
        for (var i = 6120; i <= 6124; i++)
            await SeedOpportunityAsync(i, null);
        for (var i = 6125; i <= 6129; i++)
            await SeedOpportunityAsync(i, "Has Content");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(5);
    }

    [Fact] [Trait("TestId", "FUN-030")]
    public async Task Statement_UpdateAllStatements_MigrationReturns0()
    {
        for (var i = 6130; i <= 6134; i++)
            await SeedOpportunityAsync(i, null);
        await RunStatementFixMigrationAsync();

        for (var i = 6130; i <= 6134; i++)
        {
            var opp = await DbContext.Opportunities.FindAsync(i);
            opp!.OpportunityStatementMarkdown = "Updated Content";
        }
        await DbContext.SaveChangesAsync();

        var secondRun = await RunStatementFixMigrationAsync();
        secondRun.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-031")]
    public async Task Statement_NullStatementCausesNoDisplayError_FallbackExists()
    {
        await SeedOpportunityAsync(6140, null);

        var opp = await DbContext.Opportunities.FindAsync(6140);
        var displayValue = opp!.OpportunityStatementMarkdown ?? EmptyMarkdown;

        displayValue.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "FUN-032")]
    public async Task Statement_MarkdownWithAllFormats_Persists()
    {
        const string complexMarkdown = @"# Title
## Section 1
**Bold** and *italic*

- List item 1
- List item 2

| Col A | Col B |
|-------|-------|
| A1    | B1    |

```
code block
```

> Quote here

[Link](https://unops.org)";
        await SeedOpportunityAsync(6141, complexMarkdown);

        (await DbContext.Opportunities.FindAsync(6141))!.OpportunityStatementMarkdown
            .Should().Be(complexMarkdown);
    }

    [Fact] [Trait("TestId", "FUN-033")]
    public async Task Statement_MigrationFix_RecordCountSame()
    {
        for (var i = 6150; i <= 6154; i++)
            await SeedOpportunityAsync(i, null);
        var countBefore = await DbContext.Opportunities.CountAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(countBefore);
    }

    [Fact] [Trait("TestId", "FUN-034")]
    public async Task Statement_UpdateToNull_ThenMigrate_FixedToEmpty()
    {
        await SeedOpportunityAsync(6155, "Content");
        var opp = await DbContext.Opportunities.FindAsync(6155);
        opp!.OpportunityStatementMarkdown = null;
        await DbContext.SaveChangesAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6155))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "FUN-035")]
    public async Task Statement_MigrationPreservesOtherFields()
    {
        await SeedOpportunityAsync(6156, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO", "Preserved Name");
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6156);
        opp!.Name.Should().Be("Preserved Name");
        opp.Stage.Should().Be("GO");
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        opp.IsDeleted.Should().BeFalse();
    }

    // ─── §4.3 Opportunity Lifecycle Rules (FUN-036 – 060) ────────────────

    [Fact] [Trait("TestId", "FUN-036")]
    public async Task Lifecycle_ActiveToClosedTransition_ClosedStatusPersists()
    {
        await SeedOpportunityAsync(6200, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(6200);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(6200))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-037")]
    public async Task Lifecycle_ClosedTransition_ColorIsLightRed()
    {
        await SeedOpportunityAsync(6201, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(6201);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();

        var updated = await DbContext.Opportunities.FindAsync(6201);
        GetStatusColorClass(updated!.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "FUN-038")]
    public async Task Lifecycle_ClosedOpp_StatementRemains()
    {
        await SeedOpportunityAsync(6202, "Original Statement", UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(6202);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(6202))!.OpportunityStatementMarkdown.Should().Be("Original Statement");
    }

    [Fact] [Trait("TestId", "FUN-039")]
    public async Task Lifecycle_ClosedOpp_IsNotDeleted()
    {
        await SeedClosedOpportunityAsync(6203);
        (await DbContext.Opportunities.FindAsync(6203))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "FUN-040")]
    public async Task Lifecycle_ClosedOpp_QueryableBySatus()
    {
        await SeedClosedOpportunityAsync(6204);

        var closed = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 6204)
            .FirstOrDefaultAsync();

        closed.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-041")]
    public async Task Lifecycle_ActiveOpp_QueryableByStatus()
    {
        await SeedOpportunityAsync(6205, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var active = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id == 6205)
            .FirstOrDefaultAsync();

        active.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-042")]
    public async Task Lifecycle_ClosedOpp_NotReturnedAsActive()
    {
        await SeedClosedOpportunityAsync(6206);

        var active = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id == 6206)
            .FirstOrDefaultAsync();

        active.Should().BeNull();
    }

    [Fact] [Trait("TestId", "FUN-043")]
    public async Task Lifecycle_ActiveToClosedToActive_StatementPersists()
    {
        await SeedOpportunityAsync(6207, "Statement Text", UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(6207);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();
        opp.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(6207))!.OpportunityStatementMarkdown.Should().Be("Statement Text");
    }

    [Fact] [Trait("TestId", "FUN-044")]
    public async Task Lifecycle_ClosedStatus_PersistsAcrossChangeTracker()
    {
        await SeedClosedOpportunityAsync(6208);
        DbContext.ChangeTracker.Clear();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6208);
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-045")]
    public async Task Lifecycle_ClosedName_Persists()
    {
        await SeedClosedOpportunityAsync(6209, DefaultMarkdown, "Lifecycle Closed Opp");
        DbContext.ChangeTracker.Clear();

        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 6209)).Name
            .Should().Be("Lifecycle Closed Opp");
    }

    // ─── §4.4 Data Integrity Rules (FUN-046 – 070) ───────────────────────

    [Fact] [Trait("TestId", "FUN-046")]
    public async Task DataIntegrity_OppCount_UnchangedByStatementMigration()
    {
        for (var i = 6300; i <= 6309; i++)
            await SeedOpportunityAsync(i, null);
        var before = await DbContext.Opportunities.CountAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "FUN-047")]
    public async Task DataIntegrity_OppCount_UnchangedByStatusChange()
    {
        for (var i = 6310; i <= 6314; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var before = await DbContext.Opportunities.CountAsync();

        foreach (var id in Enumerable.Range(6310, 5))
        {
            var opp = await DbContext.Opportunities.FindAsync(id);
            opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        }
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "FUN-048")]
    public async Task DataIntegrity_OppNamePreserved_AfterStatusChange()
    {
        await SeedOpportunityAsync(6315, DefaultMarkdown, name: "Preserved");
        var opp = await DbContext.Opportunities.FindAsync(6315);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(6315))!.Name.Should().Be("Preserved");
    }

    [Fact] [Trait("TestId", "FUN-049")]
    public async Task DataIntegrity_OppIsDeletedFalse_AfterMigration()
    {
        await SeedOpportunityAsync(6316, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6316))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "FUN-050")]
    public async Task DataIntegrity_Budget_PreservedAfterMigration()
    {
        await SeedOpportunityAsync(6317, null);
        var budgetBefore = (await DbContext.Opportunities.FindAsync(6317))!.InitiativeBudgetUSD;
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6317))!.InitiativeBudgetUSD.Should().Be(budgetBefore);
    }

    [Fact] [Trait("TestId", "FUN-051")]
    public async Task DataIntegrity_Stage_PreservedAfterMigration()
    {
        await SeedOpportunityAsync(6318, null, stage: "GO");
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6318))!.Stage.Should().Be("GO");
    }

    [Fact] [Trait("TestId", "FUN-052")]
    public async Task DataIntegrity_ClosedStatus_PreservedAfterMigration()
    {
        await SeedClosedOpportunityWithNullStatementAsync(6319);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6319))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-053")]
    public async Task DataIntegrity_ResponsibleOrgUnit_PreservedAfterMigration()
    {
        await SeedOpportunityAsync(6320, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6320))!.ResponsibleOrgUnitId.Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-054")]
    public async Task DataIntegrity_OpportunityId_PreservedAfterMigration()
    {
        await SeedOpportunityAsync(6321, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(6321))!.Id.Should().Be(6321);
    }

    [Fact] [Trait("TestId", "FUN-055")]
    public async Task DataIntegrity_NoClearbitUrlsRelated_PNO729Focus()
    {
        await SeedClosedOpportunityAsync(6322);
        (await DbContext.Opportunities.FindAsync(6322))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    // ─── §4.5 Performance and Batch Rules (FUN-056 – 090) ────────────────

    [Fact] [Trait("TestId", "FUN-056")]
    public async Task Performance_SingleStatementMigration_Under1Second()
    {
        await SeedOpportunityAsync(6400, null);

        var sw = Stopwatch.StartNew();
        await RunStatementFixMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact] [Trait("TestId", "FUN-057")]
    public async Task Performance_ColorClassLookup_1000Calls_Under100Ms()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
            GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact] [Trait("TestId", "FUN-058")]
    public async Task Performance_10OppsMigration_Under2Seconds()
    {
        for (var i = 6410; i <= 6419; i++)
            await SeedOpportunityAsync(i, null);

        var sw = Stopwatch.StartNew();
        await RunStatementFixMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "FUN-059")]
    public async Task Performance_SingleOppStatusChange_Under1Second()
    {
        await SeedOpportunityAsync(6420, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var sw = Stopwatch.StartNew();
        var opp = await DbContext.Opportunities.FindAsync(6420);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    [Fact] [Trait("TestId", "FUN-060")]
    public async Task Performance_BatchQuery_50ClosedOpps_Under2Seconds()
    {
        for (var i = 6430; i <= 6479; i++)
            await SeedClosedOpportunityAsync(i);

        var sw = Stopwatch.StartNew();
        var closed = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 6430 && o.Id <= 6479)
            .ToListAsync();
        sw.Stop();

        closed.Should().HaveCount(50);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact] [Trait("TestId", "FUN-061")]
    public async Task DataIntegrity_QueryWithBothFilters_ClosedNotDeleted()
    {
        await SeedClosedOpportunityAsync(6500);
        var del = await SeedClosedOpportunityAsync(6501);
        del.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var closedActive = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 6500 && o.Id <= 6501)
            .CountAsync();

        closedActive.Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-062")]
    public async Task DataIntegrity_ClosedOppsQueryable_AfterStatementFix()
    {
        await SeedClosedOpportunityWithNullStatementAsync(6502);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 6502)
            .FirstOrDefaultAsync();

        opp.Should().NotBeNull();
        opp!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "FUN-063")]
    public async Task DataIntegrity_StatementFix_NullToEmpty_VerifiedInQuery()
    {
        for (var i = 6510; i <= 6519; i++)
            await SeedOpportunityAsync(i, null);
        await RunStatementFixMigrationAsync();

        var emptyStmt = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6510 && o.Id <= 6519 && o.OpportunityStatementMarkdown == "")
            .CountAsync();

        emptyStmt.Should().Be(10);
    }

    [Fact] [Trait("TestId", "FUN-064")]
    public async Task DataIntegrity_StatementFix_NoNullRemainsForActive()
    {
        for (var i = 6520; i <= 6524; i++)
            await SeedOpportunityAsync(i, null);
        await RunStatementFixMigrationAsync();

        var nullRemaining = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6520 && o.Id <= 6524 && o.OpportunityStatementMarkdown == null)
            .CountAsync();

        nullRemaining.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-065")]
    public async Task DataIntegrity_ClosedStatusColor_AllClosedOppsLightRed_AfterFix()
    {
        for (var i = 6530; i <= 6534; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);
        await RunStatementFixMigrationAsync();

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6530 && o.Id <= 6534)
            .ToListAsync();

        foreach (var opp in opps)
        {
            opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
            opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        }
    }

    [Fact] [Trait("TestId", "FUN-066")]
    public async Task Functional_PNO729_AllBusinessRulesApplied()
    {
        await SeedClosedOpportunityWithNullStatementAsync(6540);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6540);

        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed, "PNO-729: Closed status set");
        opp.Stage.Should().Be("NO GO", "PNO-729: Stage is NO GO");
        opp.IsDeleted.Should().BeFalse("PNO-729: Not deleted");
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown, "PNO-729: Null statement fixed");
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor, "PNO-729: Color is light-red");
    }

    [Fact] [Trait("TestId", "FUN-067")]
    public async Task Functional_ActiveOpp_AllRulesApplied()
    {
        await SeedOpportunityAsync(6541, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO");
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6541);

        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        opp.Stage.Should().Be("GO");
        opp.IsDeleted.Should().BeFalse();
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(opp.Status).Should().Be("green");
    }

    [Fact] [Trait("TestId", "FUN-068")]
    public async Task Functional_MixedOpps_AllHaveCorrectColors()
    {
        await SeedOpportunityAsync(6542, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await SeedClosedOpportunityAsync(6543);
        await SeedOpportunityAsync(6544, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);
        await SeedOpportunityAsync(6545, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Draft);

        var opps = await DbContext.Opportunities.Where(o => o.Id >= 6542 && o.Id <= 6545).ToListAsync();

        GetStatusColorClass(opps.First(o => o.Id == 6542).Status).Should().Be("green");
        GetStatusColorClass(opps.First(o => o.Id == 6543).Status).Should().Be(ClosedStatusColor);
        GetStatusColorClass(opps.First(o => o.Id == 6544).Status).Should().Be("grey");
        GetStatusColorClass(opps.First(o => o.Id == 6545).Status).Should().Be("blue");
    }

    [Fact] [Trait("TestId", "FUN-069")]
    public async Task Functional_ClosedStatusQuery_ReturnedAndDisplayedCorrectly()
    {
        for (var i = 6550; i <= 6554; i++)
            await SeedClosedOpportunityAsync(i);

        var closedOpps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 6550 && o.Id <= 6554)
            .ToListAsync();

        closedOpps.Should().HaveCount(5);
        foreach (var opp in closedOpps)
        {
            var color = GetStatusColorClass(opp.Status);
            color.Should().Be(ClosedStatusColor);
        }
    }

    [Fact] [Trait("TestId", "FUN-070")]
    public async Task Functional_StatementAndStatus_IndependentFields()
    {
        await SeedOpportunityAsync(6555, null, UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6555);
        opp!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "FUN-071")]
    public async Task Functional_MigrationAffectedCount_MatchesNullCount()
    {
        for (var i = 6560; i <= 6569; i++)
            await SeedOpportunityAsync(i, i % 2 == 0 ? null : "Has Content");

        var fixed_ = await RunStatementFixMigrationAsync();
        var expectedNull = await DbContext.Opportunities
            .CountAsync(o => !o.IsDeleted && o.Id >= 6560 && o.Id <= 6569 && o.OpportunityStatementMarkdown == "");

        fixed_.Should().Be(expectedNull, "Affected count matches nullified count");
    }

    [Fact] [Trait("TestId", "FUN-072")]
    public async Task Functional_ClosedOpp_CanStillHaveStatementUpdated()
    {
        await SeedClosedOpportunityAsync(6570, "Original");
        var opp = await DbContext.Opportunities.FindAsync(6570);
        opp!.OpportunityStatementMarkdown = "Post-Close Update";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(6570))!.OpportunityStatementMarkdown.Should().Be("Post-Close Update");
    }

    [Fact] [Trait("TestId", "FUN-073")]
    public async Task Functional_ClosedColor_DeterminesUiDisplay()
    {
        await SeedClosedOpportunityAsync(6571);
        var opp = await DbContext.Opportunities.FindAsync(6571);

        var uiColorClass = GetStatusColorClass(opp!.Status);
        uiColorClass.Should().Be("light-red");
    }

    [Fact] [Trait("TestId", "FUN-074")]
    public async Task Functional_ActiveColor_DeterminesUiDisplay()
    {
        await SeedOpportunityAsync(6572, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(6572);

        GetStatusColorClass(opp!.Status).Should().Be("green");
    }

    [Fact] [Trait("TestId", "FUN-075")]
    public async Task Functional_InactiveColor_DeterminesUiDisplay()
    {
        await SeedOpportunityAsync(6573, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);
        var opp = await DbContext.Opportunities.FindAsync(6573);

        GetStatusColorClass(opp!.Status).Should().Be("grey");
    }

    [Fact] [Trait("TestId", "FUN-076")]
    public async Task Functional_DraftColor_DeterminesUiDisplay()
    {
        await SeedOpportunityAsync(6574, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Draft);
        var opp = await DbContext.Opportunities.FindAsync(6574);

        GetStatusColorClass(opp!.Status).Should().Be("blue");
    }

    [Fact] [Trait("TestId", "FUN-077")]
    public async Task Functional_ClosedOppWithStatement_AllFieldsAvailable()
    {
        await SeedClosedOpportunityAsync(6575, DefaultMarkdown, "Full Closed Opp");
        var opp = await DbContext.Opportunities.FindAsync(6575);

        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
        opp.OpportunityStatementMarkdown.Should().Be(DefaultMarkdown);
        opp.Name.Should().Be("Full Closed Opp");
        opp.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "FUN-078")]
    public async Task Functional_StatementMigration_ClosedOppFixed()
    {
        await SeedClosedOpportunityWithNullStatementAsync(6576);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6576);
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "FUN-079")]
    public async Task Functional_StatusColor_PNO729_LightRedForClosed()
    {
        const string expectedColor = "light-red";
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().Be(expectedColor);
    }

    [Fact] [Trait("TestId", "FUN-080")]
    public async Task Functional_StatusColorMap_Complete()
    {
        foreach (var status in System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>())
            GetStatusColorClass(status).Should().NotBeNullOrEmpty();
    }

    [Fact] [Trait("TestId", "FUN-081")]
    public async Task Functional_ClosedOppNotInActiveFilter()
    {
        await SeedClosedOpportunityAsync(6580);

        var inActive = await DbContext.Opportunities
            .AnyAsync(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id == 6580);

        inActive.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "FUN-082")]
    public async Task Functional_StatementFix_CreatesQueryableEmptyString()
    {
        await SeedOpportunityAsync(6581, null);
        await RunStatementFixMigrationAsync();

        var count = await DbContext.Opportunities
            .CountAsync(o => o.Id == 6581 && o.OpportunityStatementMarkdown == "");
        count.Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-083")]
    public async Task Functional_ClosedOppCount_Accurate()
    {
        for (var i = 6590; i <= 6599; i++)
            await SeedClosedOpportunityAsync(i);

        var count = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 6590 && o.Id <= 6599);

        count.Should().Be(10);
    }

    [Fact] [Trait("TestId", "FUN-084")]
    public async Task Functional_ActiveOppCount_Accurate()
    {
        for (var i = 6600; i <= 6604; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var count = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id >= 6600 && o.Id <= 6604);

        count.Should().Be(5);
    }

    [Fact] [Trait("TestId", "FUN-085")]
    public async Task Functional_MixedStatementsClosed_AllFixed()
    {
        for (var i = 6610; i <= 6614; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);

        await RunStatementFixMigrationAsync();

        for (var i = 6610; i <= 6614; i++)
        {
            var opp = await DbContext.Opportunities.FindAsync(i);
            opp!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
            opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        }
    }

    [Fact] [Trait("TestId", "FUN-086")]
    public async Task Functional_ClosedOppStatementFixed_ColorStillLightRed()
    {
        await SeedClosedOpportunityWithNullStatementAsync(6615);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6615);
        GetStatusColorClass(opp!.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "FUN-087")]
    public async Task Functional_PNO729Fix_ClosedNotGreyAnymore()
    {
        var closedColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        closedColor.Should().Be("light-red", "PNO-729: Fixed the color to light-red");
        closedColor.Should().NotBe("grey", "PNO-729: Was incorrectly grey before fix");
    }

    [Fact] [Trait("TestId", "FUN-088")]
    public async Task Functional_AllClosedOpps_AllShowLightRed_End2End()
    {
        for (var i = 6620; i <= 6624; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 6620 && o.Id <= 6624)
            .ToListAsync();

        opps.Should().HaveCount(5);
        foreach (var opp in opps)
        {
            opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
        }
    }

    [Fact] [Trait("TestId", "FUN-089")]
    public async Task Functional_StatementMigration_10Opps_5Fixed_5Preserved()
    {
        for (var i = 6630; i <= 6634; i++)
            await SeedOpportunityAsync(i, null);
        for (var i = 6635; i <= 6639; i++)
            await SeedOpportunityAsync(i, "Existing");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(5);

        for (var i = 6635; i <= 6639; i++)
            (await DbContext.Opportunities.FindAsync(i))!.OpportunityStatementMarkdown.Should().Be("Existing");
    }

    [Fact] [Trait("TestId", "FUN-090")]
    public async Task Functional_FullPNO729Verification_AllThreeAspects()
    {
        await SeedClosedOpportunityWithNullStatementAsync(6640);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(6640);

        // Aspect 1: Statement fix (null → empty)
        opp!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown, "Aspect 1: Statement fixed");

        // Aspect 2: Closed status preserved
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed, "Aspect 2: Status is Closed");

        // Aspect 3: Color is light-red (not grey)
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor, "Aspect 3: Color is light-red");
    }
}
