/**
 * @fileoverview PNO-729 Negative Tests — 90 failure-path tests.
 * Invalid inputs, soft-delete interactions, missing data, and error scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Negative Tests — 90 tests for invalid inputs and failure scenarios.
/// </summary>
[Collection("Negative")]
[Trait("Category", "Negative")]
[Trait("Ticket", "PNO-729")]
public class NegativeTests : PNO729TestFixtureBase
{
    // ─── §2.1 Soft-Deleted Opportunities (NEG-001 – 015) ─────────────────

    [Fact] [Trait("TestId", "NEG-001")]
    public async Task SoftDeleted_OppNotReturnedByActiveQuery()
    {
        var opp = await SeedOpportunityAsync(2001, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id == 2001)
            .FirstOrDefaultAsync();

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-002")]
    public async Task SoftDeleted_OppNotCountedInActiveCount()
    {
        var opp = await SeedOpportunityAsync(2002, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var count = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted && o.Id == 2002);

        count.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-003")]
    public async Task SoftDeleted_StatementMigration_NotAffected()
    {
        var opp = await SeedOpportunityAsync(2003, null);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(0, "Soft-deleted opps not affected by migration");
        (await DbContext.Opportunities.FindAsync(2003))!.OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-004")]
    public async Task SoftDeleted_ClosedOpp_NotInActiveQuery()
    {
        var opp = await SeedClosedOpportunityAsync(2004);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 2004)
            .FirstOrDefaultAsync();

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-005")]
    public async Task SoftDeleted_IsDeletedTrue_MarksRecord()
    {
        var opp = await SeedOpportunityAsync(2005, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(2005))!.IsDeleted.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "NEG-006")]
    public async Task SoftDeleted_MixedDeletedAndActive_ActiveReturned()
    {
        await SeedOpportunityAsync(2006, DefaultMarkdown);
        var del = await SeedOpportunityAsync(2007, DefaultMarkdown);
        del.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var active = await DbContext.Opportunities.Where(o => !o.IsDeleted && o.Id >= 2006 && o.Id <= 2007).ToListAsync();

        active.Should().HaveCount(1);
        active[0].Id.Should().Be(2006);
    }

    [Fact] [Trait("TestId", "NEG-007")]
    public async Task SoftDeleted_CountByStatus_ExcludesDeleted()
    {
        await SeedClosedOpportunityAsync(2008);
        var del = await SeedClosedOpportunityAsync(2009);
        del.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var count = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 2008 && o.Id <= 2009);

        count.Should().Be(1);
    }

    [Fact] [Trait("TestId", "NEG-008")]
    public async Task SoftDeleted_StatementQuery_ExcludesDeleted()
    {
        var opp = await SeedOpportunityAsync(2010, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.OpportunityStatementMarkdown != null && o.Id == 2010)
            .FirstOrDefaultAsync();

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-009")]
    public async Task SoftDeleted_AllOpps_QueryReturnsNone()
    {
        for (var i = 2011; i <= 2013; i++)
        {
            var o = await SeedOpportunityAsync(i, DefaultMarkdown);
            o.IsDeleted = true;
        }
        await DbContext.SaveChangesAsync();

        var count = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted && o.Id >= 2011 && o.Id <= 2013);
        count.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-010")]
    public async Task SoftDeleted_MigrationCount_ZeroIfAllDeleted()
    {
        for (var i = 2014; i <= 2016; i++)
        {
            var o = await SeedOpportunityAsync(i, null);
            o.IsDeleted = true;
        }
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-011")]
    public async Task SoftDeleted_StatusColorClass_NotAffectedByDelete()
    {
        var color = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        color.Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-012")]
    public async Task SoftDeleted_IdStillExists_InDb()
    {
        var opp = await SeedOpportunityAsync(2017, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(2017)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-013")]
    public async Task SoftDeleted_CanBeReactivated()
    {
        var opp = await SeedOpportunityAsync(2018, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        opp.IsDeleted = false;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.Where(o => !o.IsDeleted && o.Id == 2018).FirstOrDefaultAsync())
            .Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-014")]
    public async Task SoftDeleted_ClosedWithStatement_NotInActiveQuery()
    {
        var opp = await SeedClosedOpportunityAsync(2019, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed)
            .FirstOrDefaultAsync(o => o.Id == 2019);

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-015")]
    public async Task SoftDeleted_HasCorrectIsDeletedFlag()
    {
        var opp = await SeedOpportunityAsync(2020, DefaultMarkdown);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(2020))!.IsDeleted.Should().BeTrue();
    }

    // ─── §2.2 Status Color Errors (NEG-016 – 030) ────────────────────────

    [Fact] [Trait("TestId", "NEG-016")]
    public async Task StatusColor_Closed_NotGrey()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBe("grey");
    }

    [Fact] [Trait("TestId", "NEG-017")]
    public async Task StatusColor_Closed_NotGreen()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBe("green");
    }

    [Fact] [Trait("TestId", "NEG-018")]
    public async Task StatusColor_Closed_NotBlue()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBe("blue");
    }

    [Fact] [Trait("TestId", "NEG-019")]
    public async Task StatusColor_Closed_NotOrange()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBe("orange");
    }

    [Fact] [Trait("TestId", "NEG-020")]
    public async Task StatusColor_Active_NotLightRed()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Active).Should().NotBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-021")]
    public async Task StatusColor_Inactive_NotLightRed()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Inactive).Should().NotBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-022")]
    public async Task StatusColor_Draft_NotLightRed()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Draft).Should().NotBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-023")]
    public async Task StatusColor_Closed_IsLightRed_PNO729BusinessRule()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed)
            .Should().Be(ClosedStatusColor, "PNO-729: Closed opportunities must show in light-red");
    }

    [Fact] [Trait("TestId", "NEG-024")]
    public async Task StatusColor_AllNonClosedStatuses_NotLightRed()
    {
        var nonClosed = new[]
        {
            UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            UNOPS.PAO.Domain.Entities.EntityStatus.Inactive,
            UNOPS.PAO.Domain.Entities.EntityStatus.Draft
        };

        foreach (var status in nonClosed)
            GetStatusColorClass(status).Should().NotBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-025")]
    public async Task StatusColor_Closed_ConsistentAcrossMultipleCalls()
    {
        var results = Enumerable.Range(0, 10)
            .Select(_ => GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed))
            .ToList();

        results.Should().AllBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-026")]
    public async Task StatusColor_GreyNotUsedForClosed_PNO729Fix()
    {
        var colorBeforeFix = "grey";
        var colorAfterFix = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);

        colorAfterFix.Should().NotBe(colorBeforeFix,
            "PNO-729 fixed: Closed should not be grey (was incorrect before fix)");
    }

    [Fact] [Trait("TestId", "NEG-027")]
    public async Task StatusColor_ActiveIsGreen_NotLightRed()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Active).Should().Be("green");
    }

    [Fact] [Trait("TestId", "NEG-028")]
    public async Task StatusColor_InactiveIsGrey_NotLightRed()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Inactive).Should().Be("grey");
    }

    [Fact] [Trait("TestId", "NEG-029")]
    public async Task StatusColor_ColorClassNotNull_ForAnyClosed()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-030")]
    public async Task StatusColor_ColorClassNotEmpty_ForAnyClosed()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().NotBeEmpty();
    }

    // ─── §2.3 Statement Invalid Input (NEG-031 – 050) ────────────────────

    [Fact] [Trait("TestId", "NEG-031")]
    public async Task Statement_NonExistentOppId_ReturnsNull()
    {
        var result = await DbContext.Opportunities.FindAsync(99999);
        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-032")]
    public async Task Statement_NegativeId_ReturnsNull()
    {
        var result = await DbContext.Opportunities.FindAsync(-1);
        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-033")]
    public async Task Statement_ZeroId_ReturnsNull()
    {
        var result = await DbContext.Opportunities.FindAsync(0);
        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-034")]
    public async Task Statement_MaxIntId_ReturnsNullIfNotSeeded()
    {
        var result = await DbContext.Opportunities.FindAsync(int.MaxValue);
        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-035")]
    public async Task Statement_OppWithNullStatement_HasNullField()
    {
        await SeedOpportunityAsync(2030, null);

        (await DbContext.Opportunities.FindAsync(2030))!.OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-036")]
    public async Task Statement_OppWithEmptyStatement_HasEmptyField()
    {
        await SeedOpportunityAsync(2031, "");

        (await DbContext.Opportunities.FindAsync(2031))!.OpportunityStatementMarkdown.Should().Be("");
    }

    [Fact] [Trait("TestId", "NEG-037")]
    public async Task Statement_QueryForNullStatement_ReturnsOnlyNullOnes()
    {
        await SeedOpportunityAsync(2032, null);
        await SeedOpportunityAsync(2033, "## Has Statement");

        var nullStmt = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Id >= 2032 && o.Id <= 2033 && o.OpportunityStatementMarkdown == null)
            .ToListAsync();

        nullStmt.Should().HaveCount(1);
        nullStmt[0].Id.Should().Be(2032);
    }

    [Fact] [Trait("TestId", "NEG-038")]
    public async Task Statement_UpdateToNull_Persists()
    {
        await SeedOpportunityAsync(2034, DefaultMarkdown);
        var opp = await DbContext.Opportunities.FindAsync(2034);
        opp!.OpportunityStatementMarkdown = null;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 2034))
            .OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-039")]
    public async Task Statement_DeletedOpp_StatementNotFixedByMigration()
    {
        var opp = await SeedOpportunityAsync(2035, null);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-040")]
    public async Task Statement_MigrationOnNonNullStatements_ZeroAffected()
    {
        await SeedOpportunityAsync(2036, "Has Statement");
        await SeedOpportunityAsync(2037, "Another Statement");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    // ─── §2.4 Status Validation Failures (NEG-041 – 060) ─────────────────

    [Fact] [Trait("TestId", "NEG-041")]
    public async Task Status_ActiveOpp_NotQueriedAsClosed()
    {
        await SeedOpportunityAsync(2040, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var closed = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 2040)
            .FirstOrDefaultAsync();

        closed.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-042")]
    public async Task Status_ClosedAndDeletedOpp_NotInClosedActiveQuery()
    {
        var opp = await SeedClosedOpportunityAsync(2041);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 2041)
            .FirstOrDefaultAsync();

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-043")]
    public async Task Status_ActiveCannotShowInLightRed_ByColorLogic()
    {
        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Active).Should().NotBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-044")]
    public async Task Status_InactiveNotClosed_DifferentStatusEnums()
    {
        UNOPS.PAO.Domain.Entities.EntityStatus.Inactive.Should().NotBe(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-045")]
    public async Task Status_DraftNotClosed()
    {
        UNOPS.PAO.Domain.Entities.EntityStatus.Draft.Should().NotBe(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-046")]
    public async Task Status_ActiveNotClosed()
    {
        UNOPS.PAO.Domain.Entities.EntityStatus.Active.Should().NotBe(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-047")]
    public async Task Status_FilterByWrongStatus_ReturnsMismatch()
    {
        await SeedClosedOpportunityAsync(2042);

        var activeQuery = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id == 2042)
            .FirstOrDefaultAsync();

        activeQuery.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-048")]
    public async Task Status_ClosedEnumNotDefault()
    {
        var defaultStatus = default(UNOPS.PAO.Domain.Entities.EntityStatus);
        defaultStatus.Should().NotBe(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-049")]
    public async Task Status_ClosedIntValue_Unique()
    {
        var closedVal = (int)UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        var activeVal = (int)UNOPS.PAO.Domain.Entities.EntityStatus.Active;
        var inactiveVal = (int)UNOPS.PAO.Domain.Entities.EntityStatus.Inactive;
        var draftVal = (int)UNOPS.PAO.Domain.Entities.EntityStatus.Draft;

        closedVal.Should().NotBe(activeVal);
        closedVal.Should().NotBe(inactiveVal);
        closedVal.Should().NotBe(draftVal);
    }

    [Fact] [Trait("TestId", "NEG-050")]
    public async Task Status_ClosedOpp_NamePreserved()
    {
        await SeedClosedOpportunityAsync(2043, DefaultMarkdown, "PNO-729 Closed");

        (await DbContext.Opportunities.FindAsync(2043))!.Name.Should().Be("PNO-729 Closed");
    }

    // ─── §2.5 Migration Failure Scenarios (NEG-051 – 075) ────────────────

    [Fact] [Trait("TestId", "NEG-051")]
    public async Task Migration_EmptyDb_Returns0()
    {
        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-052")]
    public async Task Migration_AllActiveNotNull_Returns0()
    {
        await SeedOpportunityAsync(2050, "## Statement 1");
        await SeedOpportunityAsync(2051, "## Statement 2");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-053")]
    public async Task Migration_AllDeletedNullStatement_Returns0()
    {
        for (var i = 2052; i <= 2054; i++)
        {
            var o = await SeedOpportunityAsync(i, null);
            o.IsDeleted = true;
        }
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-054")]
    public async Task Migration_SecondRun_Returns0()
    {
        await SeedOpportunityAsync(2055, null);
        await RunStatementFixMigrationAsync();

        var secondRun = await RunStatementFixMigrationAsync();
        secondRun.Should().Be(0, "Second run should be idempotent");
    }

    [Fact] [Trait("TestId", "NEG-055")]
    public async Task Migration_EmptyStringStatement_NotFixed()
    {
        await SeedOpportunityAsync(2056, "");
        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(0, "Empty string is not null; not targeted");
    }

    [Fact] [Trait("TestId", "NEG-056")]
    public async Task Migration_MixedDeletedAndActiveNull_OnlyActiveFixed()
    {
        await SeedOpportunityAsync(2057, null);
        var del = await SeedOpportunityAsync(2058, null);
        del.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(1);
    }

    [Fact] [Trait("TestId", "NEG-057")]
    public async Task Migration_ClosedOppWithNullStatement_IsFixed()
    {
        await SeedClosedOpportunityWithNullStatementAsync(2059);

        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(1);
        (await DbContext.Opportunities.FindAsync(2059))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "NEG-058")]
    public async Task Migration_ActiveOppWithNullStatement_IsFixed()
    {
        await SeedOpportunityAsync(2060, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(1);
    }

    [Fact] [Trait("TestId", "NEG-059")]
    public async Task Migration_PartialNull_OnlyNullOnesFixed()
    {
        await SeedOpportunityAsync(2061, null);
        await SeedOpportunityAsync(2062, "Has Statement");
        await SeedOpportunityAsync(2063, null);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(2);
    }

    [Fact] [Trait("TestId", "NEG-060")]
    public async Task Migration_Idempotent_NoSideEffects()
    {
        await SeedOpportunityAsync(2064, null);
        await SeedOpportunityAsync(2065, "Existing");

        await RunStatementFixMigrationAsync();
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2065))!.OpportunityStatementMarkdown.Should().Be("Existing");
    }

    // ─── §2.6 Opportunity Record Integrity (NEG-061 – 075) ───────────────

    [Fact] [Trait("TestId", "NEG-061")]
    public async Task Integrity_OppCount_NotChangedByStatementUpdate()
    {
        await SeedOpportunityAsync(2070, DefaultMarkdown);
        var before = await DbContext.Opportunities.CountAsync();
        var opp = await DbContext.Opportunities.FindAsync(2070);
        opp!.OpportunityStatementMarkdown = "Updated";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "NEG-062")]
    public async Task Integrity_OppId_NotChangedByStatementUpdate()
    {
        await SeedOpportunityAsync(2071, DefaultMarkdown);
        var opp = await DbContext.Opportunities.FindAsync(2071);
        opp!.OpportunityStatementMarkdown = "Updated";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(2071))!.Id.Should().Be(2071);
    }

    [Fact] [Trait("TestId", "NEG-063")]
    public async Task Integrity_OppName_NotChangedByStatusUpdate()
    {
        await SeedOpportunityAsync(2072, DefaultMarkdown, name: "Original Name");
        var opp = await DbContext.Opportunities.FindAsync(2072);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(2072))!.Name.Should().Be("Original Name");
    }

    [Fact] [Trait("TestId", "NEG-064")]
    public async Task Integrity_IsDeleted_NotChangedByStatementMigration()
    {
        await SeedOpportunityAsync(2073, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2073))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "NEG-065")]
    public async Task Integrity_Status_NotChangedByStatementMigration()
    {
        await SeedOpportunityAsync(2074, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2074))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "NEG-066")]
    public async Task Integrity_Stage_NotChangedByStatementMigration()
    {
        await SeedOpportunityAsync(2075, null, stage: "GO");
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2075))!.Stage.Should().Be("GO");
    }

    [Fact] [Trait("TestId", "NEG-067")]
    public async Task Integrity_ClosedStatus_NotChangedByStatementMigration()
    {
        await SeedClosedOpportunityWithNullStatementAsync(2076);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2076))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-068")]
    public async Task Integrity_ClosedStage_NotChangedByStatementMigration()
    {
        await SeedClosedOpportunityWithNullStatementAsync(2077);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2077))!.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "NEG-069")]
    public async Task Integrity_Budget_NotChangedByMigration()
    {
        await SeedOpportunityAsync(2078, null);
        var budgetBefore = (await DbContext.Opportunities.FindAsync(2078))!.InitiativeBudgetUSD;
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2078))!.InitiativeBudgetUSD.Should().Be(budgetBefore);
    }

    [Fact] [Trait("TestId", "NEG-070")]
    public async Task Integrity_ResponsibleOrgUnitId_NotChangedByMigration()
    {
        await SeedOpportunityAsync(2079, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2079))!.ResponsibleOrgUnitId.Should().Be(1);
    }

    // ─── §2.7 Additional Edge Failures (NEG-071 – 090) ───────────────────

    [Fact] [Trait("TestId", "NEG-071")]
    public async Task Failure_ClosedOpportunityStillRetrievable()
    {
        await SeedClosedOpportunityAsync(2080);

        var opp = await DbContext.Opportunities.FindAsync(2080);
        opp.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-072")]
    public async Task Failure_ClosedOpportunity_NotInActiveStageQuery()
    {
        await SeedClosedOpportunityAsync(2081);

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == "GO" && o.Id == 2081)
            .FirstOrDefaultAsync();

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-073")]
    public async Task Failure_ClosedOpportunity_IsInNoGoQuery()
    {
        await SeedClosedOpportunityAsync(2082);

        var result = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Stage == "NO GO" && o.Id == 2082)
            .FirstOrDefaultAsync();

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-074")]
    public async Task Failure_StatementMigration_OnlyTargetsNull()
    {
        await SeedOpportunityAsync(2083, "Not null");
        var before = (await DbContext.Opportunities.FindAsync(2083))!.OpportunityStatementMarkdown;

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2083))!.OpportunityStatementMarkdown.Should().Be(before);
    }

    [Fact] [Trait("TestId", "NEG-075")]
    public async Task Failure_StatusColorMap_HasNoNullValues()
    {
        var statuses = new[]
        {
            UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            UNOPS.PAO.Domain.Entities.EntityStatus.Inactive,
            UNOPS.PAO.Domain.Entities.EntityStatus.Closed,
            UNOPS.PAO.Domain.Entities.EntityStatus.Draft
        };

        foreach (var s in statuses)
            GetStatusColorClass(s).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-076")]
    public async Task Failure_StatusColorMap_HasNoEmptyValues()
    {
        var statuses = new[]
        {
            UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            UNOPS.PAO.Domain.Entities.EntityStatus.Inactive,
            UNOPS.PAO.Domain.Entities.EntityStatus.Closed,
            UNOPS.PAO.Domain.Entities.EntityStatus.Draft
        };

        foreach (var s in statuses)
            GetStatusColorClass(s).Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "NEG-077")]
    public async Task Failure_StatementFixedToEmpty_IsQueryable()
    {
        await SeedOpportunityAsync(2084, null);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities
            .Where(o => o.Id == 2084 && o.OpportunityStatementMarkdown == "")
            .FirstOrDefaultAsync();

        opp.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-078")]
    public async Task Failure_StatementFixedToEmpty_NotNull()
    {
        await SeedOpportunityAsync(2085, null);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(2085))!.OpportunityStatementMarkdown.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-079")]
    public async Task Failure_MigrationCountAccurate_AllNull()
    {
        for (var i = 2090; i <= 2094; i++)
            await SeedOpportunityAsync(i, null);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(5);
    }

    [Fact] [Trait("TestId", "NEG-080")]
    public async Task Failure_MigrationCountAccurate_MixedNull()
    {
        await SeedOpportunityAsync(2095, null);
        await SeedOpportunityAsync(2096, "Has it");
        await SeedOpportunityAsync(2097, null);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(2);
    }

    [Fact] [Trait("TestId", "NEG-081")]
    public async Task Failure_OppRecordCount_NotChangedByMigration()
    {
        for (var i = 2100; i <= 2104; i++)
            await SeedOpportunityAsync(i, null);
        var before = await DbContext.Opportunities.CountAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "NEG-082")]
    public async Task Failure_MigrationNoException_OnAllNull()
    {
        for (var i = 2105; i <= 2110; i++)
            await SeedOpportunityAsync(i, null);

        var act = async () => await RunStatementFixMigrationAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "NEG-083")]
    public async Task Failure_MigrationNoException_OnEmptyDb()
    {
        var act = async () => await RunStatementFixMigrationAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "NEG-084")]
    public async Task Failure_ClosedOppCount_CorrectAfterQuery()
    {
        for (var i = 2115; i <= 2119; i++)
            await SeedClosedOpportunityAsync(i);

        var count = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 2115 && o.Id <= 2119);

        count.Should().Be(5);
    }

    [Fact] [Trait("TestId", "NEG-085")]
    public async Task Failure_ActiveOppCount_ExcludesClosed()
    {
        await SeedClosedOpportunityAsync(2120);
        await SeedOpportunityAsync(2121, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var count = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id >= 2120 && o.Id <= 2121);

        count.Should().Be(1);
    }

    [Fact] [Trait("TestId", "NEG-086")]
    public async Task Failure_ClosedOppLightRed_NotGrey_PNO729Fix()
    {
        await SeedClosedOpportunityAsync(2122);
        var opp = await DbContext.Opportunities.FindAsync(2122);

        var color = GetStatusColorClass(opp!.Status);
        color.Should().Be(ClosedStatusColor);
        color.Should().NotBe("grey");
    }

    [Fact] [Trait("TestId", "NEG-087")]
    public async Task Failure_ActiveOppLightRed_NotAssigned()
    {
        await SeedOpportunityAsync(2123, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(2123);

        GetStatusColorClass(opp!.Status).Should().NotBe(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "NEG-088")]
    public async Task Failure_StatementNullification_NotValidOperation()
    {
        await SeedOpportunityAsync(2124, DefaultMarkdown);
        var opp = await DbContext.Opportunities.FindAsync(2124);

        opp!.OpportunityStatementMarkdown.Should().NotBeNull("Statement was set and should not self-nullify");
    }

    [Fact] [Trait("TestId", "NEG-089")]
    public async Task Failure_ClosedStage_NoGoAndClosed_Consistent()
    {
        await SeedClosedOpportunityAsync(2125);
        var opp = await DbContext.Opportunities.FindAsync(2125);

        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-090")]
    public async Task Failure_ClosedStatus_CanBeDetectedByStatusField()
    {
        await SeedClosedOpportunityAsync(2126);
        var opp = await DbContext.Opportunities.FindAsync(2126);

        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        (opp.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().BeTrue();
    }
}
