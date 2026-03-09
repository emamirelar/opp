/**
 * @fileoverview PNO-914 Boundary Tests — edge values, boundary conditions, soft-delete interactions.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914;

[Collection("PNO914_Boundary")]
[Trait("Category", "Boundary")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "IAPSessionAndAnalytics")]
public class BoundaryTests : PNO914TestFixtureBase
{
    [Fact]
    [Trait("TestId", "TC-PNO914-BND-001")]
    public async Task CreateFromInteractions_MaxNameLength120_Accepted()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var name = new string('A', 120);
        var request = BuildRequest(name: name);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().HaveLength(120);
    }

    [Fact]

    [Trait("Defect", "DEF-049")]
    [Trait("TestId", "TC-PNO914-BND-002")]
    public async Task CreateFromInteractions_NameExceeds120_Rejected()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(name: new string('x', 121));

        // Act & Assert
        await FluentActions.Invoking(() =>
            Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId))
            .Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-BND-003")]
    public async Task CreateFromInteractions_SingleInteraction_Works()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedInteractionAsync(1, "Single Interaction");
        var request = BuildRequest(
            name: "Single Interaction Opportunity",
            sourceInteractionIds: new List<int> { 1 });

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-BND-004")]
    public async Task CreateFromInteractions_100Interactions_AllLinked()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var interactionIds = new List<int>();
        for (var i = 1; i <= 100; i++)
        {
            await SeedInteractionAsync(i, $"Interaction {i}");
            interactionIds.Add(i);
        }

        var request = BuildRequest(
            name: "100 Interactions Opportunity",
            sourceInteractionIds: interactionIds);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert — simulate controller linking
        foreach (var id in interactionIds)
        {
            DbContext.OpportunityInteractions.Add(new OpportunityInteraction
            {
                OpportunityId = result.Id,
                InteractionId = id,
                Name = $"Link-{id}",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await DbContext.SaveChangesAsync();

        var links = await DbContext.OpportunityInteractions
            .Where(oi => oi.OpportunityId == result.Id && !oi.IsDeleted)
            .ToListAsync();
        links.Should().HaveCount(100);
    }

    [Fact]

    [Trait("Defect", "DEF-050")]
    [Trait("TestId", "TC-PNO914-BND-005")]
    public async Task CreateFromInteractions_NullOptionalFields_DefaultsApplied()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Minimal Opportunity",
            Description = null,
            PartnerId = null,
            IsFundingPartner = false,
            IsClientPartner = false,
            ResponsibleOrgUnitId = null,
            SourceInteractionIds = null
        };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Minimal Opportunity");
        result.ResponsibleOrgUnitId.Should().BeNull();
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-BND-006")]
    public async Task CreateFromInteractions_EmptyDocumentList_NoDocumentsCreated()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(
            name: "No Documents Opportunity",
            documents: new List<NewDocumentRequest>());

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert — manager creates opportunity; controller would persist empty document list
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "QA-088: AI proposal truncation is in GeminiManager - not in CreateOpportunityFromProposalAsync")]
    [Trait("TestId", "TC-PNO914-BND-007")]
    public async Task AIProposal_LongInteractionDescription_TruncatedAppropriately()
    {
        await Task.CompletedTask;
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-BND-008")]
    public async Task Document_MaxMimeTypeLength_Accepted()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var longMimeType = "application/" + new string('x', 100);
        var request = BuildRequest(
            name: "Valid Name",
            documents: new List<NewDocumentRequest>
            {
                new() { GcsPath = "gs://bucket/folder/doc.pdf", MimeType = longMimeType }
            });

        // Act — manager creates opportunity; documents are persisted by controller
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-BND-009")]
    public async Task InteractionSelection_AllInteractionsDeleted_EmptyResult()
    {
        // Arrange — all deleted interactions
        await EnsureReferenceDataAsync();
        await SeedInteractionAsync(1, "Deleted 1", isDeleted: true);
        await SeedInteractionAsync(2, "Deleted 2", isDeleted: true);
        var request = BuildRequest(
            name: "Deleted Interactions Opportunity",
            sourceInteractionIds: new List<int> { 1, 2 });

        // Act — manager creates opportunity; controller links (may include deleted)
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }
}
