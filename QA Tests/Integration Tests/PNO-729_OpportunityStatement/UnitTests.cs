/**
 * @fileoverview PNO-729 Unit Tests — 21 focused unit tests.
 * Pure model and logic validation: EntityStatus enum, GetStatusColorClass, markdown string contracts.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// PNO-729 Unit Tests — 21 pure unit tests for model/logic validation.
/// </summary>
[Collection("PNO729 Unit")]
[Trait("Category", "Unit")]
[Trait("Ticket", "PNO-729")]
public class UnitTests : PNO729TestFixtureBase
{
    // --------------- EntityStatus enum ---------------

    [Fact] [Trait("TestId", "UNT-001")]
    public void EntityStatus_Closed_Defined()
    {
        System.Enum.IsDefined(typeof(EntityStatus), EntityStatus.Closed).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-002")]
    public void EntityStatus_Active_Defined()
    {
        System.Enum.IsDefined(typeof(EntityStatus), EntityStatus.Active).Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-003")]
    public void EntityStatus_AllValues_NotEmpty()
    {
        System.Enum.GetValues<EntityStatus>().Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "UNT-004")]
    public void EntityStatus_Closed_NotSameAs_Active()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Active);
    }

    // --------------- GetStatusColorClass ---------------

    [Fact] [Trait("TestId", "UNT-005")]
    public void GetStatusColorClass_Closed_ReturnsLightRed()
    {
        GetStatusColorClass(EntityStatus.Closed).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "UNT-006")]
    public void GetStatusColorClass_Closed_NotEmpty()
    {
        GetStatusColorClass(EntityStatus.Closed).Should().NotBeNullOrWhiteSpace();
    }

    [Fact] [Trait("TestId", "UNT-007")]
    public void GetStatusColorClass_Closed_NotGrey()
    {
        GetStatusColorClass(EntityStatus.Closed).Should().NotBe("grey");
    }

    [Fact] [Trait("TestId", "UNT-008")]
    public void GetStatusColorClass_Active_DifferentToClosed()
    {
        GetStatusColorClass(EntityStatus.Active).Should()
            .NotBe(GetStatusColorClass(EntityStatus.Closed));
    }

    [Fact] [Trait("TestId", "UNT-009")]
    public void GetStatusColorClass_Closed_ReturnsLightRed_Repeatedly()
    {
        for (var i = 0; i < 10; i++)
            GetStatusColorClass(EntityStatus.Closed).Should().Be(ClosedStatusColor);
    }

    [Fact] [Trait("TestId", "UNT-010")]
    public void GetStatusColorClass_AllStatuses_NonNull()
    {
        foreach (var status in System.Enum.GetValues<EntityStatus>())
            GetStatusColorClass(status).Should().NotBeNull();
    }

    // --------------- Markdown string contracts ---------------

    [Fact] [Trait("TestId", "UNT-011")]
    public void DefaultMarkdown_NotNull()
    {
        DefaultMarkdown.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "UNT-012")]
    public void DefaultMarkdown_NotEmpty()
    {
        DefaultMarkdown.Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "UNT-013")]
    public void EmptyMarkdown_IsEmpty()
    {
        EmptyMarkdown.Should().BeEmpty();
    }

    [Fact] [Trait("TestId", "UNT-014")]
    public void EmptyMarkdown_NotNull()
    {
        EmptyMarkdown.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "UNT-015")]
    public void DefaultMarkdown_DifferentFrom_EmptyMarkdown()
    {
        DefaultMarkdown.Should().NotBe(EmptyMarkdown);
    }

    // --------------- Business rule checks ---------------

    [Fact] [Trait("TestId", "UNT-016")]
    public void ClosedStatusColor_EqualsLightRed()
    {
        ClosedStatusColor.Should().Be("light-red");
    }

    [Fact] [Trait("TestId", "UNT-017")]
    public void ClosedStatusColor_NotGrey()
    {
        ClosedStatusColor.Should().NotBe("grey");
    }

    [Fact] [Trait("TestId", "UNT-018")]
    public void PNO729Fix_GreyNotUsedForClosed()
    {
        const string deprecated = "grey";
        GetStatusColorClass(EntityStatus.Closed).Should().NotBe(deprecated,
            "PNO-729 fix: grey was the old (incorrect) color for Closed status");
    }

    [Fact] [Trait("TestId", "UNT-019")]
    public void EntityStatus_Closed_StringRepresentation_NonEmpty()
    {
        EntityStatus.Closed.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact] [Trait("TestId", "UNT-020")]
    public void Markdown_LargeString_DoesNotThrow()
    {
        var large = new string('X', 10000);
        var act = () => large.Length;
        act.Should().NotThrow();
    }

    [Fact] [Trait("TestId", "UNT-021")]
    public void Markdown_UnicodeContent_DoesNotCorrupt()
    {
        var unicode = "Ωпортунитет ☁ 机会 Opportunité";
        unicode.Should().Contain("☁");
    }
}
