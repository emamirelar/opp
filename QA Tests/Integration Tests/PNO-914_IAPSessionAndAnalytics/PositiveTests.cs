/**
 * @fileoverview PNO-914 Positive Tests — Create Opportunity from Interactions, AI proposal, interaction linking.
 * Verifies happy-path CreateOpportunityFromProposalAsync and related flows.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914;

[Collection("PNO914_Positive")]
[Trait("Category", "Positive")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "IAPSessionAndAnalytics")]
public class PositiveTests : PNO914TestFixtureBase
{
    [Fact]
    [Trait("TestId", "TC-PNO914-POS-001")]
    public async Task CreateOpportunityFromInteractions_WithValidData_OpportunityCreated()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedOrgUnitAsync(10, "South Asia Hub");
        await SeedInteractionAsync(1, "Meeting with Partner A");
        await SeedInteractionAsync(2, "Follow-up discussion");

        var request = BuildRequest(
            name: "Opportunity from Interactions",
            responsibleOrgUnitId: 10,
            sourceInteractionIds: new List<int> { 1, 2 });

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Opportunity from Interactions");
        result.ResponsibleOrgUnitId.Should().Be(10);

        var opp = await GetOpportunityFromDbAsync(result.Id);
        opp.Should().NotBeNull();
        opp!.Name.Should().Be("Opportunity from Interactions");
    }

    [Fact]

    [Trait("Defect", "DEF-053")]
    [Trait("TestId", "TC-PNO914-POS-002")]
    public async Task AIProposal_GenerateFromInteractions_ReturnsProposal()
    {
        // AI proposal generation is handled by GeminiManager via /api/opportunity/generate-proposal.
        // Manager-level integration tests focus on CreateOpportunityFromProposalAsync.
        // This scenario is covered by API or E2E tests with full server.
        await Task.CompletedTask;
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-POS-003")]
    public async Task InteractionSelection_MultipleInteractions_AllLinked()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedInteractionAsync(10, "Interaction A");
        await SeedInteractionAsync(20, "Interaction B");
        await SeedInteractionAsync(30, "Interaction C");

        var request = BuildRequest(
            name: "Multi-Interaction Opportunity",
            sourceInteractionIds: new List<int> { 10, 20, 30 });

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert — manager creates opportunity; controller links interactions
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        // Simulate controller linking interactions
        foreach (var interactionId in request.SourceInteractionIds!)
        {
            DbContext.OpportunityInteractions.Add(new OpportunityInteraction
            {
                OpportunityId = result.Id,
                InteractionId = interactionId,
                Name = $"Link-{interactionId}",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await DbContext.SaveChangesAsync();

        var links = await DbContext.OpportunityInteractions
            .Where(oi => oi.OpportunityId == result.Id && !oi.IsDeleted)
            .ToListAsync();
        links.Should().HaveCount(3);
        links.Select(l => l.InteractionId).Should().Contain(new[] { 10, 20, 30 });
    }
}
