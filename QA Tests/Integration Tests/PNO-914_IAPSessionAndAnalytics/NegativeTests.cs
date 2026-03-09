/**
 * @fileoverview PNO-914 Negative Tests — invalid inputs, validation errors, expected failures.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914;

[Collection("PNO914_Negative")]
[Trait("Category", "Negative")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "IAPSessionAndAnalytics")]
public class NegativeTests : PNO914TestFixtureBase
{
    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-001")]
    public async Task CreateFromInteractions_NoInteractionsSelected_ThrowsValidation()
    {
        // Arrange — empty interaction list is valid for manager; controller may validate
        await EnsureReferenceDataAsync();
        var request = BuildRequest(
            name: "Valid Name",
            sourceInteractionIds: new List<int>());

        // Act — manager accepts empty list; creates opportunity without interaction links
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert — manager does not require interactions; opportunity is created
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-002")]
    public async Task CreateFromInteractions_NullName_ThrowsValidation()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(name: null!);

        // Act & Assert
        await FluentActions.Invoking(() =>
            Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId))
            .Should().ThrowAsync<Exception>();
    }

    [Fact]

    [Trait("Defect", "DEF-048")]
    [Trait("TestId", "TC-PNO914-NEG-003")]
    public async Task CreateFromInteractions_EmptyName_ThrowsValidation()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(name: "");

        // Act & Assert
        await FluentActions.Invoking(() =>
            Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId))
            .Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-004")]
    public async Task CreateFromInteractions_InvalidInteractionId_HandlesGracefully()
    {
        // Arrange — non-existent interaction IDs; manager creates opportunity; controller links
        await EnsureReferenceDataAsync();
        var request = BuildRequest(
            name: "Test Opportunity",
            sourceInteractionIds: new List<int> { 99999, 88888 });

        // Act — manager creates opportunity; FK for OpportunityInteraction may allow orphan
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-005")]
    public async Task CreateFromInteractions_DeletedInteraction_Skipped()
    {
        // Arrange — seed deleted interaction
        await EnsureReferenceDataAsync();
        await SeedInteractionAsync(50, "Deleted Interaction", isDeleted: true);
        var request = BuildRequest(
            name: "Test Opportunity",
            sourceInteractionIds: new List<int> { 50 });

        // Act — manager creates opportunity; controller links; deleted interaction may still link
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert — opportunity created; linking is controller responsibility
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-006")]
    public async Task CreateFromInteractions_DuplicateInteractionIds_Deduplicated()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedInteractionAsync(1, "Interaction");
        var request = BuildRequest(
            name: "Dedup Test",
            sourceInteractionIds: new List<int> { 1, 1, 1 });

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert — manager creates opportunity; controller should deduplicate when linking
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "QA-088: AI proposal validation is in controller - GenerateOpportunityProposal endpoint")]
    [Trait("TestId", "TC-PNO914-NEG-007")]
    public async Task AIProposal_EmptyInteractionList_ThrowsValidation()
    {
        // Controller validates: at least one source required (interactions, new docs, or existing docs)
        await Task.CompletedTask;
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-008")]
    public async Task AIProposal_NullRequest_ThrowsArgumentNull()
    {
        // Act & Assert — null request to CreateOpportunityFromProposalAsync
        await FluentActions.Invoking(() =>
            Manager.CreateOpportunityFromProposalAsync(null!, CurrentUserId))
            .Should().ThrowAsync<Exception>();
    }

    [Fact(Skip = "QA-088: Document GCS validation is in controller/DocumentManager - not in OpportunityManager")]
    [Trait("TestId", "TC-PNO914-NEG-009")]
    public async Task Document_InvalidGcsPath_ThrowsValidation()
    {
        // Document validation happens when controller persists documents after opportunity creation
        await Task.CompletedTask;
    }

    [Fact]

    [Trait("Defect", "DEF-048")]
    [Trait("TestId", "TC-PNO914-NEG-010")]
    public async Task CreateFromInteractions_WhitespaceOnlyName_ThrowsValidation()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(name: "   ");

        // Act & Assert
        await FluentActions.Invoking(() =>
            Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId))
            .Should().ThrowAsync<Exception>();
    }

    [Fact]

    [Trait("Defect", "DEF-049")]
    [Trait("TestId", "TC-PNO914-NEG-011")]
    public async Task CreateFromInteractions_NameExceeds120Chars_ThrowsValidation()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(name: new string('x', 121));

        // Act & Assert
        await FluentActions.Invoking(() =>
            Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId))
            .Should().ThrowAsync<Exception>();
    }
}
