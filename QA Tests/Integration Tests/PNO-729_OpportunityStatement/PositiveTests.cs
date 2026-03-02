/**
 * @fileoverview PNO-729 Positive Tests — 30 happy-path tests.
 * Statement generation, Closed status display, and core data persistence.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Positive Tests — 30 happy-path tests for opportunity statement and Closed status.
/// </summary>
[Collection("Positive")]
[Trait("Category", "Positive")]
[Trait("Ticket", "PNO-729")]
public class PositiveTests : PNO729TestFixtureBase
{
    // ─── §1.1 Statement Persistence (POS-001 – 010) ──────────────────────

    [Fact] [Trait("TestId", "POS-001")]
    public async Task Statement_SaveMarkdown_PersistsToDb()
    {
        await SeedOpportunityAsync(1001, DefaultMarkdown);

        var opp = await DbContext.Opportunities.FindAsync(1001);

        opp!.OpportunityStatementMarkdown.Should().Be(DefaultMarkdown);
    }

    [Fact] [Trait("TestId", "POS-002")]
    public async Task Statement_UpdateMarkdown_PersistsChange()
    {
        await SeedOpportunityAsync(1002, DefaultMarkdown);
        var opp = await DbContext.Opportunities.FindAsync(1002);
        opp!.OpportunityStatementMarkdown = "## Updated Statement";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1002))
            .OpportunityStatementMarkdown.Should().Be("## Updated Statement");
    }

    [Fact] [Trait("TestId", "POS-003")]
    public async Task Statement_NullMarkdown_CanBeStoredAndRetrieved()
    {
        await SeedOpportunityAsync(1003, null);

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1003);
        opp.OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "POS-004")]
    public async Task Statement_EmptyMarkdown_CanBeStoredAndRetrieved()
    {
        await SeedOpportunityAsync(1004, "");

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1004);
        opp.OpportunityStatementMarkdown.Should().Be("");
    }

    [Fact] [Trait("TestId", "POS-005")]
    public async Task Statement_LongMarkdown_PersistsCorrectly()
    {
        var longMarkdown = "# Statement\n\n" + new string('A', 5000);
        await SeedOpportunityAsync(1005, longMarkdown);

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1005);
        opp.OpportunityStatementMarkdown.Should().Be(longMarkdown);
    }

    [Fact] [Trait("TestId", "POS-006")]
    public async Task Statement_MarkdownWithHeaders_PersistsCorrectly()
    {
        var markdown = "## WHY\n\n## WHAT\n\n## Team\n\n## Budget\n\n## Schedule";
        await SeedOpportunityAsync(1006, markdown);

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1006);
        opp.OpportunityStatementMarkdown.Should().Contain("## WHY");
        opp.OpportunityStatementMarkdown.Should().Contain("## Budget");
    }

    [Fact] [Trait("TestId", "POS-007")]
    public async Task Statement_MarkdownWithSpecialChars_PersistsCorrectly()
    {
        var markdown = "# Statement\n\n- Item 1\n- Item 2\n\n**Bold** and *italic*\n\n[Link](https://unops.org)";
        await SeedOpportunityAsync(1007, markdown);

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1007);
        opp.OpportunityStatementMarkdown.Should().Contain("**Bold**");
    }

    [Fact] [Trait("TestId", "POS-008")]
    public async Task Statement_MultipleUpdates_LastUpdateWins()
    {
        await SeedOpportunityAsync(1008, "Version 1");
        var opp = await DbContext.Opportunities.FindAsync(1008);
        opp!.OpportunityStatementMarkdown = "Version 2";
        await DbContext.SaveChangesAsync();
        opp.OpportunityStatementMarkdown = "Version 3";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1008))
            .OpportunityStatementMarkdown.Should().Be("Version 3");
    }

    [Fact] [Trait("TestId", "POS-009")]
    public async Task Statement_ClearFromMarkdownToNull_PersistsNull()
    {
        await SeedOpportunityAsync(1009, DefaultMarkdown);
        var opp = await DbContext.Opportunities.FindAsync(1009);
        opp!.OpportunityStatementMarkdown = null;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1009))
            .OpportunityStatementMarkdown.Should().BeNull();
    }

    [Fact] [Trait("TestId", "POS-010")]
    public async Task Statement_MarkdownWithUnicodeChars_PersistsCorrectly()
    {
        var markdown = "# Déclaration d'opportunité\n\n- Büro\n- 北京\n- العربية";
        await SeedOpportunityAsync(1010, markdown);

        var opp = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1010);
        opp.OpportunityStatementMarkdown.Should().Contain("Déclaration");
        opp.OpportunityStatementMarkdown.Should().Contain("北京");
    }

    // ─── §1.2 Closed Status Display (POS-011 – 020) ──────────────────────

    [Fact] [Trait("TestId", "POS-011")]
    public async Task ClosedStatus_OpportunityStatusSetToClosed_PersistsCorrectly()
    {
        await SeedClosedOpportunityAsync(1011);

        var opp = await DbContext.Opportunities.FindAsync(1011);
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-012")]
    public async Task ClosedStatus_DisplayColorClass_IsLightRed()
    {
        var colorClass = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);

        colorClass.Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "POS-013")]
    public async Task ClosedStatus_AfterReject_StageIsNoGo()
    {
        await SeedClosedOpportunityAsync(1012);

        var opp = await DbContext.Opportunities.FindAsync(1012);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-014")]
    public async Task ClosedStatus_ColorClass_DifferentFromActive()
    {
        var closedColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        var activeColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        closedColor.Should().NotBe(activeColor);
    }

    [Fact] [Trait("TestId", "POS-015")]
    public async Task ClosedStatus_ColorClass_DifferentFromInactive()
    {
        var closedColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        var inactiveColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        closedColor.Should().NotBe(inactiveColor);
    }

    [Fact] [Trait("TestId", "POS-016")]
    public async Task ClosedStatus_ClosedOpportunity_HasStatementMarkdown()
    {
        await SeedClosedOpportunityAsync(1013, DefaultMarkdown);

        var opp = await DbContext.Opportunities.FindAsync(1013);
        opp!.OpportunityStatementMarkdown.Should().NotBeNullOrEmpty();
    }

    [Fact] [Trait("TestId", "POS-017")]
    public async Task ClosedStatus_EnumValue_IsDefined()
    {
        System.Enum.IsDefined(typeof(UNOPS.PAO.Domain.Entities.EntityStatus), UNOPS.PAO.Domain.Entities.EntityStatus.Closed)
            .Should().BeTrue();
    }

    [Fact] [Trait("TestId", "POS-018")]
    public async Task ClosedStatus_CanQueryByStatus()
    {
        await SeedClosedOpportunityAsync(1014);
        await SeedOpportunityAsync(1015, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var closedOpps = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed)
            .ToListAsync();

        closedOpps.Should().Contain(o => o.Id == 1014);
        closedOpps.Should().NotContain(o => o.Id == 1015);
    }

    [Fact] [Trait("TestId", "POS-019")]
    public async Task ClosedStatus_SetCorrectly_NoGoStageAndClosedStatus()
    {
        await SeedOpportunityAsync(1016, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active, "GO");
        var opp = await DbContext.Opportunities.FindAsync(1016);
        opp!.Stage = "NO GO";
        opp.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        var updated = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1016);
        updated.Stage.Should().Be("NO GO");
        updated.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-020")]
    public async Task ClosedStatus_CountQuery_ReturnsOnlyClosedOpps()
    {
        await SeedClosedOpportunityAsync(1017);
        await SeedClosedOpportunityAsync(1018);
        await SeedOpportunityAsync(1019, DefaultMarkdown, UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var closedCount = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Closed
                && o.Id >= 1017 && o.Id <= 1019)
            .CountAsync();

        closedCount.Should().Be(2);
    }

    // ─── §1.3 Combined Statement + Closed Status (POS-021 – 030) ─────────

    [Fact] [Trait("TestId", "POS-021")]
    public async Task Combined_ClosedOppWithStatement_BothFieldsCorrect()
    {
        await SeedClosedOpportunityAsync(1020, "## Closed Opportunity Statement");

        var opp = await DbContext.Opportunities.FindAsync(1020);
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        opp.OpportunityStatementMarkdown.Should().Contain("## Closed Opportunity Statement");
    }

    [Fact] [Trait("TestId", "POS-022")]
    public async Task Combined_ActiveOppWithStatement_StatusActive()
    {
        await SeedOpportunityAsync(1021, "## Active Statement", UNOPS.PAO.Domain.Entities.EntityStatus.Active);

        var opp = await DbContext.Opportunities.FindAsync(1021);
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        opp.OpportunityStatementMarkdown.Should().Contain("## Active Statement");
    }

    [Fact] [Trait("TestId", "POS-023")]
    public async Task Combined_UpdateStatementOnClosedOpp_BothFieldsPersist()
    {
        await SeedClosedOpportunityAsync(1022, "Old Statement");
        var opp = await DbContext.Opportunities.FindAsync(1022);
        opp!.OpportunityStatementMarkdown = "New Statement";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        var updated = await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1022);
        updated.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        updated.OpportunityStatementMarkdown.Should().Be("New Statement");
    }

    [Fact] [Trait("TestId", "POS-024")]
    public async Task Combined_MixedStatuses_ClosedColorCorrect()
    {
        var statuses = new[]
        {
            UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            UNOPS.PAO.Domain.Entities.EntityStatus.Closed,
            UNOPS.PAO.Domain.Entities.EntityStatus.Inactive,
            UNOPS.PAO.Domain.Entities.EntityStatus.Draft
        };

        GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "POS-025")]
    public async Task Combined_OppIdPreserved_AfterStatementAndStatusChange()
    {
        await SeedOpportunityAsync(1023, "Original", UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        var opp = await DbContext.Opportunities.FindAsync(1023);
        opp!.OpportunityStatementMarkdown = "Updated";
        opp.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed;
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Opportunities.AsNoTracking().FirstAsync(o => o.Id == 1023)).Id.Should().Be(1023);
    }

    [Fact] [Trait("TestId", "POS-026")]
    public async Task Combined_ClosedStatusDoesNotDeleteRecord()
    {
        await SeedClosedOpportunityAsync(1024);

        var opp = await DbContext.Opportunities.FindAsync(1024);
        opp!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "POS-027")]
    public async Task Combined_ClosedOppIsQueryable()
    {
        await SeedClosedOpportunityAsync(1025, DefaultMarkdown, "Closed Opp POS-027");

        var opp = await DbContext.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Name == "Closed Opp POS-027");

        opp.Should().NotBeNull();
        opp!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "POS-028")]
    public async Task Combined_StatementMigration_FixesNullStatements()
    {
        await SeedOpportunityAsync(1026, null);
        await SeedOpportunityAsync(1027, null);

        var fixed_ = await RunStatementFixMigrationAsync();

        fixed_.Should().Be(2);
        (await DbContext.Opportunities.FindAsync(1026))!.OpportunityStatementMarkdown.Should().Be(EmptyMarkdown);
    }

    [Fact] [Trait("TestId", "POS-029")]
    public async Task Combined_StatementMigration_PreservesExistingStatements()
    {
        await SeedOpportunityAsync(1028, "Existing Statement");

        await RunStatementFixMigrationAsync();

        (await DbContext.Opportunities.FindAsync(1028))!.OpportunityStatementMarkdown.Should().Be("Existing Statement");
    }

    [Fact] [Trait("TestId", "POS-030")]
    public async Task Combined_ClosedStatusColor_IsLightRed_NotGrey()
    {
        var closedColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Closed);
        var greyColor = GetStatusColorClass(UNOPS.PAO.Domain.Entities.EntityStatus.Inactive);

        closedColor.Should().Be(ClosedStatusColor);
        closedColor.Should().NotBe(greyColor,
            "PNO-729: Closed status must be displayed in light-red, not grey");
    }
}
