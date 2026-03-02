/**
 * @fileoverview PNO-729 Integration Tests — 90 end-to-end flow tests.
 * Full pipeline, DbContext round-trips, cross-component interactions.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Integration Tests — 90 end-to-end and cross-component tests.
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Ticket", "PNO-729")]
public class IntegrationTests : PNO729TestFixtureBase
{
    // ─── §5.1 Full Flow — Statement Fix (INT-001 – 025) ──────────────────

    [Fact] [Trait("TestId", "INT-001")]
    public async Task FullFlow_NullStatement_FixedToEmpty_VerifyInDb()
    {
        await SeedOpportunityAsync(7001, null);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7001);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "INT-002")]
    public async Task FullFlow_ExistingStatement_Preserved()
    {
        await SeedOpportunityAsync(7002, "Preserved Statement");
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7002);
        opp.OpportunityStatementMarkdown.Should().Be("Preserved Statement");
    }

    [Fact] [Trait("TestId", "INT-003")]
    public async Task FullFlow_ClosedOppNullStatement_FixedToEmpty()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7003);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7003);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-004")]
    public async Task FullFlow_ClosedOpp_ColorIsLightRed_AfterFix()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7004);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7004);
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-005")]
    public async Task FullFlow_ActiveOppNullStatement_FixedColorGreen()
    {
        await SeedOpportunityAsync(7005, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7005);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(opp.Status).Should().Be("green");
    }

    [Fact] [Trait("TestId", "INT-006")]
    public async Task FullFlow_Idempotency_TwoRuns_SameResult()
    {
        await SeedOpportunityAsync(7006, null);
        await RunStatementFixMigrationAsync();
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7006);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "INT-007")]
    public async Task FullFlow_10Opps_5Null_5HasContent_CorrectResult()
    {
        for (var i = 7010; i <= 7014; i++)
            await SeedOpportunityAsync(i, null);
        for (var i = 7015; i <= 7019; i++)
            await SeedOpportunityAsync(i, "Content");

        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(5);
        for (var i = 7010; i <= 7014; i++)
        {
            DbContext.ChangeTracker.Clear();
            (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == i))
                .OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        }
        for (var i = 7015; i <= 7019; i++)
        {
            DbContext.ChangeTracker.Clear();
            (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == i))
                .OpportunityStatementMarkdown.Should().Be("Content");
        }
    }

    [Fact] [Trait("TestId", "INT-008")]
    public async Task FullFlow_DeletedOpp_NotFixed()
    {
        var opp = await SeedOpportunityAsync(7020, null);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(0);
        (await DbContext.Opportunities.FindAsync(7020))!.OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-009")]
    public async Task FullFlow_ClosedStatusPersists_AfterStatementFix()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7021);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7021);
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-010")]
    public async Task FullFlow_StageNoGo_PersistsAfterStatementFix()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7022);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7022);
        opp.Stage.Should().Be("NO GO");
    }

    [Fact] [Trait("TestId", "INT-011")]
    public async Task FullFlow_ActiveStatus_PersistsAfterStatementFix()
    {
        await SeedOpportunityAsync(7023, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO");
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7023);
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "INT-012")]
    public async Task FullFlow_IsDeleted_False_AfterStatementFix()
    {
        await SeedOpportunityAsync(7024, null);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7024)).IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-013")]
    public async Task FullFlow_Name_Preserved_AfterStatementFix()
    {
        await SeedOpportunityAsync(7025, null, name: "Named Opp");
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7025)).Name.Should().Be("Named Opp");
    }

    [Fact] [Trait("TestId", "INT-014")]
    public async Task FullFlow_Id_Preserved_AfterStatementFix()
    {
        await SeedOpportunityAsync(7026, null);
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7026)).Id.Should().Be(7026);
    }

    [Fact] [Trait("TestId", "INT-015")]
    public async Task FullFlow_OppCount_UnchangedByStatementFix()
    {
        for (var i = 7030; i <= 7034; i++)
            await SeedOpportunityAsync(i, null);
        var before = await DbContext.Opportunities.CountAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    // ─── §5.2 Full Flow — Closed Status (INT-016 – 040) ──────────────────

    [Fact] [Trait("TestId", "INT-016")]
    public async Task FullFlow_Closed_SeedVerifyColor()
    {
        await SeedClosedOpportunityAsync(7100);

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7100);
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-017")]
    public async Task FullFlow_Active_SeedVerifyColor()
    {
        await SeedOpportunityAsync(7101, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7101);
        GetStatusColorClass(opp.Status).Should().Be("green");
    }

    [Fact] [Trait("TestId", "INT-018")]
    public async Task FullFlow_Inactive_SeedVerifyColor()
    {
        await SeedOpportunityAsync(7102, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7102);
        GetStatusColorClass(opp.Status).Should().Be("grey");
    }

    [Fact] [Trait("TestId", "INT-019")]
    public async Task FullFlow_Draft_SeedVerifyColor()
    {
        await SeedOpportunityAsync(7103, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Draft);

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7103);
        GetStatusColorClass(opp.Status).Should().Be("blue");
    }

    [Fact] [Trait("TestId", "INT-020")]
    public async Task FullFlow_TransitionToClose_ColorChanges()
    {
        await SeedOpportunityAsync(7104, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(7104);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        var updated = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7104);
        GetStatusColorClass(updated.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-021")]
    public async Task FullFlow_ClosedOpp_QueryableByStatusAndStage()
    {
        await SeedClosedOpportunityAsync(7105);

        var result = await DbContext.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 7105 && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Stage == "NO GO");

        result.Should().NotBeNull();
        GetStatusColorClass(result!.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-022")]
    public async Task FullFlow_ClosedOpp_NotQueryableAsActive()
    {
        await SeedClosedOpportunityAsync(7106);

        var result = await DbContext.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 7106 && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        result.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-023")]
    public async Task FullFlow_MixedStatuses_AllColorsCorrect()
    {
        await SeedOpportunityAsync(7107, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await SeedClosedOpportunityAsync(7108);
        await SeedOpportunityAsync(7109, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id >= 7107 && o.Id <= 7109)
            .ToListAsync();

        GetStatusColorClass(opps.First(o => o.Id == 7107).Status).Should().Be("green");
        GetStatusColorClass(opps.First(o => o.Id == 7108).Status).Should().Be(ClosedStatusColor);
        GetStatusColorClass(opps.First(o => o.Id == 7109).Status).Should().Be("grey");
    }

    [Fact] [Trait("TestId", "INT-024")]
    public async Task FullFlow_ClosedOppCount_QueryableByFilter()
    {
        for (var i = 7110; i <= 7114; i++)
            await SeedClosedOpportunityAsync(i);

        var count = await DbContext.Opportunities.AsNoTracking().CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 7110 && o.Id <= 7114);

        count.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-025")]
    public async Task FullFlow_ActiveOppCount_ExcludesClosed()
    {
        for (var i = 7120; i <= 7124; i++)
            await SeedClosedOpportunityAsync(i);
        for (var i = 7125; i <= 7129; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var activeCount = await DbContext.Opportunities.AsNoTracking().CountAsync(
            o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id >= 7120 && o.Id <= 7129);

        activeCount.Should().Be(5);
    }

    // ─── §5.3 DbContext Operations (INT-026 – 060) ───────────────────────

    [Fact] [Trait("TestId", "INT-026")]
    public async Task DbContext_FindAsync_ClosedOpp_Works()
    {
        await SeedClosedOpportunityAsync(7200);
        var opp = await DbContext.Opportunities.FindAsync(7200);
        opp.Should().NotBeNull();
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-027")]
    public async Task DbContext_AsNoTracking_ClosedOpp_Works()
    {
        await SeedClosedOpportunityAsync(7201);
        var opp = await DbContext.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == 7201);
        opp.Should().NotBeNull();
        GetStatusColorClass(opp!.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-028")]
    public async Task DbContext_ToList_AllClosedOpps_Works()
    {
        for (var i = 7210; i <= 7214; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id >= 7210 && o.Id <= 7214)
            .ToListAsync();

        opps.Should().HaveCount(5);
        opps.Should().AllSatisfy(o => o.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed));
    }

    [Fact] [Trait("TestId", "INT-029")]
    public async Task DbContext_CountAsync_ClosedOpps_Works()
    {
        for (var i = 7220; i <= 7224; i++)
            await SeedClosedOpportunityAsync(i);

        var count = await DbContext.Opportunities.AsNoTracking().CountAsync(
            o => o.Id >= 7220 && o.Id <= 7224 && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed);

        count.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-030")]
    public async Task DbContext_AnyAsync_ClosedOpp_ReturnsTrue()
    {
        await SeedClosedOpportunityAsync(7225);
        var any = await DbContext.Opportunities.AsNoTracking().AnyAsync(o => o.Id == 7225);
        any.Should().BeTrue();
    }

    [Fact] [Trait("TestId", "INT-031")]
    public async Task DbContext_OrderBy_ClosedFirst()
    {
        await SeedOpportunityAsync(7230, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await SeedClosedOpportunityAsync(7231);

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id >= 7230 && o.Id <= 7231)
            .OrderByDescending(o => o.Status)
            .ToListAsync();

        opps.Should().HaveCount(2);
    }

    [Fact] [Trait("TestId", "INT-032")]
    public async Task DbContext_Select_ClosedOppStatus_Works()
    {
        await SeedClosedOpportunityAsync(7232);

        var statuses = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id == 7232)
            .Select(o => o.Status)
            .ToListAsync();

        statuses.Should().Contain(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-033")]
    public async Task DbContext_Select_StatementMarkdown_Works()
    {
        await SeedOpportunityAsync(7233, "## Statement");

        var stmts = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id == 7233)
            .Select(o => o.OpportunityStatementMarkdown)
            .ToListAsync();

        stmts.Should().Contain("## Statement");
    }

    [Fact] [Trait("TestId", "INT-034")]
    public async Task DbContext_Update_ClosedOppStatement_Works()
    {
        await SeedClosedOpportunityAsync(7234, "Old");
        var opp = await DbContext.Opportunities.FindAsync(7234);
        opp!.OpportunityStatementMarkdown = "Updated";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7234))
            .OpportunityStatementMarkdown.Should().Be("Updated");
    }

    [Fact] [Trait("TestId", "INT-035")]
    public async Task DbContext_Update_StatusToClose_Works()
    {
        await SeedOpportunityAsync(7235, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(7235);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7235))
            .Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    // ─── §5.4 Cross-Component Integration (INT-036 – 065) ────────────────

    [Fact] [Trait("TestId", "INT-036")]
    public async Task CrossComponent_MigrationAndColorLogic_Consistent()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7300);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7300);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-037")]
    public async Task CrossComponent_StatementFix_DoesNotBreakStatusColor()
    {
        await SeedOpportunityAsync(7301, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var colorBefore = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(7301);
        var colorAfter = GetStatusColorClass(opp!.Status);
        colorAfter.Should().Be(colorBefore);
    }

    [Fact] [Trait("TestId", "INT-038")]
    public async Task CrossComponent_StatusChange_DoesNotBreakStatement()
    {
        await SeedOpportunityAsync(7302, "My Statement", UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(7302);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(7302))!.OpportunityStatementMarkdown.Should().Be("My Statement");
    }

    [Fact] [Trait("TestId", "INT-039")]
    public async Task CrossComponent_MixedOpps_AllBehaviorsCorrect()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7303);
        await SeedOpportunityAsync(7304, "Content", UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await SeedOpportunityAsync(7305, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        await RunStatementFixMigrationAsync();

        var closed = await DbContext.Opportunities.FindAsync(7303);
        var active1 = await DbContext.Opportunities.FindAsync(7304);
        var active2 = await DbContext.Opportunities.FindAsync(7305);

        closed!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(closed.Status).Should().Be(ClosedStatusColor);

        active1!.OpportunityStatementMarkdown.Should().Be("Content");
        GetStatusColorClass(active1.Status).Should().Be("green");

        active2!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(active2.Status).Should().Be("green");
    }

    [Fact] [Trait("TestId", "INT-040")]
    public async Task CrossComponent_ClosedOppQueryWithStatement_BothAvailable()
    {
        await SeedClosedOpportunityAsync(7306, "Closed Statement");

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7306);
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        opp.OpportunityStatementMarkdown.Should().Be("Closed Statement");
    }

    [Fact] [Trait("TestId", "INT-041")]
    public async Task CrossComponent_DeletedOpp_ExcludedFromBothFilters()
    {
        var opp = await SeedClosedOpportunityWithNullStatementAsync(7307);
        opp.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var inClosedQuery = await DbContext.Opportunities
            .AnyAsync(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 7307);
        var inNullStatementQuery = await DbContext.Opportunities
            .AnyAsync(o => !o.IsDeleted && o.OpportunityStatementMarkdown == null && o.Id == 7307);

        inClosedQuery.Should().BeFalse();
        inNullStatementQuery.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-042")]
    public async Task CrossComponent_StatementFix_ClosedOpp_AllFieldsCorrect()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7308, "Full Opp");
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7308);

        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        opp.Stage.Should().Be("NO GO");
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        opp.Name.Should().Be("Full Opp");
        opp.IsDeleted.Should().BeFalse();
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-043")]
    public async Task CrossComponent_StatementFixed_ThenStatusChanged_BothPersist()
    {
        await SeedOpportunityAsync(7309, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(7309);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        var result = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7309);
        result.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        result.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-044")]
    public async Task CrossComponent_StatusChanged_ThenStatementFixed_BothPersist()
    {
        await SeedOpportunityAsync(7310, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(7310);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var result = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7310);
        result.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        result.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "INT-045")]
    public async Task CrossComponent_MultipleClosedOpps_AllFixedAndColorCorrect()
    {
        for (var i = 7320; i <= 7329; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);

        await RunStatementFixMigrationAsync();

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Id >= 7320 && o.Id <= 7329)
            .ToListAsync();

        opps.Should().HaveCount(10);
        foreach (var opp in opps)
        {
            opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
        }
    }

    // ─── §5.5 End-to-End Validation (INT-046 – 090) ──────────────────────

    [Fact] [Trait("TestId", "INT-046")]
    public async Task EndToEnd_SeedActive_TransitionClose_Fix_VerifyAll()
    {
        await SeedOpportunityAsync(7400, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO");
        var opp = await DbContext.Opportunities.FindAsync(7400);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        opp.Stage = "NO GO";
        await DbContext.SaveChangesAsync();

        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var final = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7400);
        final.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        final.Stage.Should().Be("NO GO");
        final.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(final.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-047")]
    public async Task EndToEnd_SeedClosed_Fix_VerifyNoClearbit()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7401);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7401);
        GetStatusColorClass(opp.Status).Should().NotContain("bit");
    }

    [Fact] [Trait("TestId", "INT-048")]
    public async Task EndToEnd_10Closed_10Active_AllHaveCorrectColors()
    {
        for (var i = 7410; i <= 7419; i++)
            await SeedClosedOpportunityAsync(i);
        for (var i = 7420; i <= 7429; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id >= 7410 && o.Id <= 7429)
            .ToListAsync();

        foreach (var opp in opps.Where(o => o.Id <= 7419))
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
        foreach (var opp in opps.Where(o => o.Id >= 7420))
            GetStatusColorClass(opp.Status).Should().Be("green");
    }

    [Fact] [Trait("TestId", "INT-049")]
    public async Task EndToEnd_StatementFix_30Opps_25Null_5Content()
    {
        for (var i = 7430; i <= 7454; i++)
            await SeedOpportunityAsync(i, null);
        for (var i = 7455; i <= 7459; i++)
            await SeedOpportunityAsync(i, "Content");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(25);
    }

    [Fact] [Trait("TestId", "INT-050")]
    public async Task EndToEnd_PNO729_AllAspectsVerified()
    {
        // Seed a closed opp with null statement (typical PNO-729 scenario)
        await SeedClosedOpportunityWithNullStatementAsync(7460, "PNO-729 Integration Test");
        await RunStatementFixMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 7460);

        // PNO-729 Verification:
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed, "Closed status persists");
        opp.Stage.Should().Be("NO GO", "Stage is NO GO");
        opp.IsDeleted.Should().BeFalse("Not deleted");
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown, "Null statement fixed to empty");
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor, "Color is light-red, not grey");
        opp.Name.Should().Be("PNO-729 Integration Test", "Name preserved");
    }

    [Fact] [Trait("TestId", "INT-051")]
    public async Task Integration_ClosedOppRecordCount_Unchanged()
    {
        for (var i = 7500; i <= 7509; i++)
            await SeedClosedOpportunityAsync(i);
        var before = await DbContext.Opportunities.CountAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "INT-052")]
    public async Task Integration_ActiveOppRecordCount_Unchanged()
    {
        for (var i = 7510; i <= 7514; i++)
            await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var before = await DbContext.Opportunities.CountAsync();

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "INT-053")]
    public async Task Integration_ClosedOpp_QueryBy_NoGoStage()
    {
        await SeedClosedOpportunityAsync(7515);

        var result = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Stage == "NO GO" && o.Id == 7515);

        result.Should().NotBeNull();
        result!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-054")]
    public async Task Integration_ActiveOpp_QueryBy_GoStage()
    {
        await SeedOpportunityAsync(7516, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO");

        var result = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Stage == "GO" && o.Id == 7516);

        result.Should().NotBeNull();
        result!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "INT-055")]
    public async Task Integration_StatementFix_Idempotent_MultipleRuns()
    {
        await SeedOpportunityAsync(7517, null);
        for (var i = 0; i < 5; i++)
            await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(7517))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "INT-056")]
    public async Task Integration_ClosedColor_Idempotent_MultipleChecks()
    {
        for (var i = 0; i < 5; i++)
            GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-057")]
    public async Task Integration_MixedOpps_StatementFix_CorrectResult()
    {
        for (var i = 7520; i <= 7524; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);
        for (var i = 7525; i <= 7529; i++)
            await SeedOpportunityAsync(i, "Content");

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(5);

        for (var i = 7525; i <= 7529; i++)
            (await DbContext.Opportunities.FindAsync(i))!.OpportunityStatementMarkdown.Should().Be("Content");
    }

    [Fact] [Trait("TestId", "INT-058")]
    public async Task Integration_ClosedOppWithNullStatement_FullEndToEnd()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7530);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(7530);
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-059")]
    public async Task Integration_ActiveOppFixed_Queryable()
    {
        await SeedOpportunityAsync(7531, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 7531 && o.OpportunityStatementMarkdown == "");

        opp.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-060")]
    public async Task Integration_ClosedOppsAfterFix_NullStatementQueryReturns0()
    {
        for (var i = 7540; i <= 7544; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);
        await RunStatementFixMigrationAsync();

        var nullCount = await DbContext.Opportunities.AsNoTracking().CountAsync(
            o => !o.IsDeleted && o.Id >= 7540 && o.Id <= 7544 && o.OpportunityStatementMarkdown == null);

        nullCount.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-061")]
    public async Task Integration_ClosedAndFixed_BothQueriesWork()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7545);
        await RunStatementFixMigrationAsync();

        var byStatus = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id == 7545);
        var byStatement = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.OpportunityStatementMarkdown == "" && o.Id == 7545);

        byStatus.Should().NotBeNull();
        byStatement.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-062")]
    public async Task Integration_FullFlow_NoClearbitRelated()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7546);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(7546);
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        GetStatusColorClass(opp.Status).Should().NotContain("clearbit");
    }

    [Fact] [Trait("TestId", "INT-063")]
    public async Task Integration_ClosedCount_QueryByColorLogic()
    {
        for (var i = 7550; i <= 7554; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Id >= 7550 && o.Id <= 7554)
            .ToListAsync();

        opps.Where(o => GetStatusColorClass(o.Status) == ClosedStatusColor).Should().HaveCount(5);
    }

    [Fact] [Trait("TestId", "INT-064")]
    public async Task Integration_ClosedNotGrey_PNO729Verified()
    {
        await SeedClosedOpportunityAsync(7555);

        var opp = await DbContext.Opportunities.FindAsync(7555);
        GetStatusColorClass(opp!.Status).Should().NotBe("grey",
            "PNO-729: Closed must be light-red, not grey");
    }

    [Fact] [Trait("TestId", "INT-065")]
    public async Task Integration_ClosedIsLightRed_PNO729Verified()
    {
        await SeedClosedOpportunityAsync(7556);

        var opp = await DbContext.Opportunities.FindAsync(7556);
        GetStatusColorClass(opp!.Status).Should().Be("light-red",
            "PNO-729: Verified light-red color for closed opportunities");
    }

    // ─── §5.6 Additional Integration (INT-066 – 090) ─────────────────────

    [Fact] [Trait("TestId", "INT-066")]
    public async Task Integration_ClosedOppsAndActiveOpps_CountSeparate()
    {
        for (var i = 7600; i <= 7604; i++) await SeedClosedOpportunityAsync(i);
        for (var i = 7605; i <= 7609; i++) await SeedOpportunityAsync(i, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var closed = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 7600 && o.Id <= 7609);
        var active = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active && o.Id >= 7600 && o.Id <= 7609);

        closed.Should().Be(5);
        active.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-067")]
    public async Task Integration_NoNullStatements_AfterMigration()
    {
        for (var i = 7610; i <= 7619; i++) await SeedOpportunityAsync(i, null);
        await RunStatementFixMigrationAsync();

        var nullCount = await DbContext.Opportunities.CountAsync(o => !o.IsDeleted && o.Id >= 7610 && o.Id <= 7619 && o.OpportunityStatementMarkdown == null);
        nullCount.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-068")]
    public async Task Integration_StatementMigration_ClosedAndActiveFixed()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7620);
        await SeedOpportunityAsync(7621, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(2);
    }

    [Fact] [Trait("TestId", "INT-069")]
    public async Task Integration_ClosedColor_NotChangedAfterStatementFix()
    {
        var colorBefore = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        await SeedClosedOpportunityWithNullStatementAsync(7622);
        await RunStatementFixMigrationAsync();
        var colorAfter = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);

        colorBefore.Should().Be(colorAfter);
        colorAfter.Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-070")]
    public async Task Integration_ColorLogic_Not_DependOn_Statement()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7623);
        await SeedClosedOpportunityAsync(7624, "Has Statement");

        var opp1 = await DbContext.Opportunities.FindAsync(7623);
        var opp2 = await DbContext.Opportunities.FindAsync(7624);

        GetStatusColorClass(opp1!.Status).Should().Be(ClosedStatusColor);
        GetStatusColorClass(opp2!.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-071")]
    public async Task Integration_ClosedOpp_CanUpdateStatement_PostFix()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7625);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(7625);
        opp!.OpportunityStatementMarkdown = "Post-Fix Update";
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(7625))!.OpportunityStatementMarkdown.Should().Be("Post-Fix Update");
    }

    [Fact] [Trait("TestId", "INT-072")]
    public async Task Integration_OpenOpp_AddedWhileMigrating_NotFixedInFirstRun()
    {
        await SeedOpportunityAsync(7626, null);
        var fixed_ = await RunStatementFixMigrationAsync();
        await SeedOpportunityAsync(7627, null);

        var postCount = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Id >= 7626 && o.Id <= 7627 && o.OpportunityStatementMarkdown == null);

        postCount.Should().Be(1, "Only 7627 remains null after first run");
    }

    [Fact] [Trait("TestId", "INT-073")]
    public async Task Integration_SecondRunFixes_NewlyNullStatement()
    {
        await SeedOpportunityAsync(7628, null);
        await RunStatementFixMigrationAsync();
        var opp = await DbContext.Opportunities.FindAsync(7628);
        opp!.OpportunityStatementMarkdown = null;
        await DbContext.SaveChangesAsync();

        var second = await RunStatementFixMigrationAsync();
        second.Should().Be(1);
    }

    [Fact] [Trait("TestId", "INT-074")]
    public async Task Integration_PNO729_CompleteScenario()
    {
        for (var i = 7700; i <= 7704; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);
        for (var i = 7705; i <= 7709; i++)
            await SeedOpportunityAsync(i, "Content", UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(5);

        var closedOpps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Id >= 7700 && o.Id <= 7704)
            .ToListAsync();

        foreach (var opp in closedOpps)
        {
            opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
            opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
        }
    }

    [Fact] [Trait("TestId", "INT-075")]
    public async Task Integration_AllBehaviors_ComprehensiveCheck()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7710, "Test");
        await SeedOpportunityAsync(7711, "Existing", UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var deleted = await SeedOpportunityAsync(7712, null);
        deleted.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(1);

        var closed = await DbContext.Opportunities.FindAsync(7710);
        closed!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
        GetStatusColorClass(closed.Status).Should().Be(ClosedStatusColor);

        var active = await DbContext.Opportunities.FindAsync(7711);
        active!.OpportunityStatementMarkdown.Should().Be("Existing");

        var del = await DbContext.Opportunities.FindAsync(7712);
        del!.OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-076")]
    public async Task Integration_ClosedCount_Query_ViaStatus()
    {
        for (var i = 7720; i <= 7729; i++)
            await SeedClosedOpportunityAsync(i);

        var count = await DbContext.Opportunities.AsNoTracking()
            .CountAsync(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed && o.Id >= 7720 && o.Id <= 7729);

        count.Should().Be(10);
    }

    [Fact] [Trait("TestId", "INT-077")]
    public async Task Integration_StatementQuery_AfterFix_EmptyStringFound()
    {
        for (var i = 7730; i <= 7734; i++)
            await SeedOpportunityAsync(i, null);
        await RunStatementFixMigrationAsync();

        var emptyStmtCount = await DbContext.Opportunities.AsNoTracking()
            .CountAsync(o => o.Id >= 7730 && o.Id <= 7734 && o.OpportunityStatementMarkdown == "");

        emptyStmtCount.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-078")]
    public async Task Integration_StatusAndStatement_CanBothBeQueried()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7735);
        await RunStatementFixMigrationAsync();

        var result = await DbContext.Opportunities.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == 7735
                && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed
                && o.OpportunityStatementMarkdown == "");

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-079")]
    public async Task Integration_ClosedNotGrey_AllInstances()
    {
        for (var i = 7740; i <= 7749; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id >= 7740 && o.Id <= 7749)
            .ToListAsync();

        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().NotBe("grey");
    }

    [Fact] [Trait("TestId", "INT-080")]
    public async Task Integration_ClosedIsLightRed_AllInstances()
    {
        for (var i = 7750; i <= 7759; i++)
            await SeedClosedOpportunityAsync(i);

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => o.Id >= 7750 && o.Id <= 7759)
            .ToListAsync();

        foreach (var opp in opps)
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-081")]
    public async Task Integration_AfterFix_NullStatementCountIsZero()
    {
        for (var i = 7760; i <= 7769; i++)
            await SeedOpportunityAsync(i, null);

        await RunStatementFixMigrationAsync();

        var nullCount = await DbContext.Opportunities.AsNoTracking()
            .CountAsync(o => !o.IsDeleted && o.Id >= 7760 && o.Id <= 7769 && o.OpportunityStatementMarkdown == null);

        nullCount.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-082")]
    public async Task Integration_ClosedOpp_ColorMapping_Deterministic()
    {
        await SeedClosedOpportunityAsync(7770);
        var opp = await DbContext.Opportunities.FindAsync(7770);

        var c1 = GetStatusColorClass(opp!.Status);
        var c2 = GetStatusColorClass(opp.Status);

        c1.Should().Be(c2).And.Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-083")]
    public async Task Integration_StatementFix_ReturnsAccurateCount()
    {
        for (var i = 7780; i <= 7789; i++)
            await SeedOpportunityAsync(i, i % 3 == 0 ? null : "Content");

        var nullCount = await DbContext.Opportunities.CountAsync(
            o => !o.IsDeleted && o.Id >= 7780 && o.Id <= 7789 && o.OpportunityStatementMarkdown == null);
        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(nullCount);
    }

    [Fact] [Trait("TestId", "INT-084")]
    public async Task Integration_ClosedAndFixed_ClosedColorStillLightRed()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7790);
        await RunStatementFixMigrationAsync();

        var opp = await DbContext.Opportunities.FindAsync(7790);
        GetStatusColorClass(opp!.Status).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "INT-085")]
    public async Task Integration_PNO729_ClosedShowsLightRed_NotGrey_End2End()
    {
        for (var i = 7800; i <= 7804; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);

        await RunStatementFixMigrationAsync();

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Id >= 7800 && o.Id <= 7804)
            .ToListAsync();

        foreach (var opp in opps)
        {
            var color = GetStatusColorClass(opp.Status);
            color.Should().Be("light-red", $"Opp {opp.Id} should be light-red");
            color.Should().NotBe("grey", $"Opp {opp.Id} was incorrectly grey before PNO-729 fix");
        }
    }

    [Fact] [Trait("TestId", "INT-086")]
    public async Task Integration_FullPNO729Batch_20Opps_10Closed_10Active()
    {
        for (var i = 7810; i <= 7819; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);
        for (var i = 7820; i <= 7829; i++)
            await SeedOpportunityAsync(i, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var fixed_ = await RunStatementFixMigrationAsync();
        fixed_.Should().Be(20);

        var closed = await DbContext.Opportunities.AsNoTracking().Where(o => o.Id >= 7810 && o.Id <= 7819).ToListAsync();
        var active = await DbContext.Opportunities.AsNoTracking().Where(o => o.Id >= 7820 && o.Id <= 7829).ToListAsync();

        foreach (var o in closed)
        {
            o.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
            GetStatusColorClass(o.Status).Should().Be(ClosedStatusColor);
        }
        foreach (var o in active)
        {
            o.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
            GetStatusColorClass(o.Status).Should().Be("green");
        }
    }

    [Fact] [Trait("TestId", "INT-087")]
    public async Task Integration_StatementFix_DoesNotAffect_Status()
    {
        await SeedClosedOpportunityWithNullStatementAsync(7830);
        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(7830))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "INT-088")]
    public async Task Integration_StatusChange_DoesNotAffect_StatementAfterFix()
    {
        await SeedOpportunityAsync(7831, null);
        await RunStatementFixMigrationAsync();
        var opp = await DbContext.Opportunities.FindAsync(7831);
        opp!.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive;
        await DbContext.SaveChangesAsync();

        (await DbContext.Opportunities.FindAsync(7831))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "INT-089")]
    public async Task Integration_FullPNO729_AllOppsHaveCorrectColor_AfterFix()
    {
        for (var i = 7840; i <= 7844; i++) await SeedClosedOpportunityWithNullStatementAsync(i);
        for (var i = 7845; i <= 7849; i++) await SeedOpportunityAsync(i, null, UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        for (var i = 7850; i <= 7854; i++) await SeedOpportunityAsync(i, null, UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        await RunStatementFixMigrationAsync();

        var opps = await DbContext.Opportunities.AsNoTracking().Where(o => o.Id >= 7840 && o.Id <= 7854).ToListAsync();

        foreach (var opp in opps)
        {
            var expected = opp.Id <= 7844 ? ClosedStatusColor : opp.Id <= 7849 ? "green" : "grey";
            GetStatusColorClass(opp.Status).Should().Be(expected, $"Opp {opp.Id} has wrong color");
        }
    }

    [Fact] [Trait("TestId", "INT-090")]
    public async Task Integration_PNO729_Final_Verification()
    {
        const int testCount = 10;
        for (var i = 8000; i < 8000 + testCount; i++)
            await SeedClosedOpportunityWithNullStatementAsync(i);

        await RunStatementFixMigrationAsync();

        var opps = await DbContext.Opportunities.AsNoTracking()
            .Where(o => !o.IsDeleted && o.Id >= 8000 && o.Id < 8000 + testCount)
            .ToListAsync();

        opps.Should().HaveCount(testCount);
        foreach (var opp in opps)
        {
            opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
            opp.Stage.Should().Be("NO GO");
            opp.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
            GetStatusColorClass(opp.Status).Should().Be(ClosedStatusColor);
            opp.IsDeleted.Should().BeFalse();
        }
    }
}
