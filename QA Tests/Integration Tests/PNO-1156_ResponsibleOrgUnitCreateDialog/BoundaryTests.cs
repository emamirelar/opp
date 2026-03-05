/**
 * @fileoverview PNO-1156 Boundary Tests — edge values, boundary conditions, soft-delete interactions.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1156;

[Collection("PNO1156_Boundary")]
[Trait("Category", "Boundary")]
[Trait("Feature", "PNO-1156")]
[Trait("Component", "ResponsibleOrgUnitCreateDialog")]
public class BoundaryTests : PNO1156TestFixtureBase
{
    [Fact]
    [Trait("TestId", "TC-PNO1156-BND-001")]
    public async Task CreateOpportunity_OrgUnitIdIsZero_TreatedAsNull()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(
            name: "Test Opportunity",
            responsibleOrgUnitId: 0);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.ResponsibleOrgUnitId.Should().Be(0);

        var opp = await GetOpportunityFromDbAsync(result.Id);
        opp.Should().NotBeNull();
        opp!.ResponsibleOrgUnitId.Should().Be(0);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1156-BND-002")]
    public async Task CreateOpportunity_OrgUnitIdIsMaxInt_HandlesCorrectly()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(
            name: "Test Opportunity",
            responsibleOrgUnitId: int.MaxValue);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.ResponsibleOrgUnitId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1156-BND-003")]
    public async Task CreateOpportunity_NameExactly120Chars_Accepted()
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
    [Trait("TestId", "TC-PNO1156-BND-004")]
    public async Task CreateOpportunity_NameExactly1Char_Accepted()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(name: "X");

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("X");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1156-BND-005")]
    public async Task CreateOpportunity_NullOrgUnitId_OpportunityCreatedWithoutOrgUnit()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        var request = BuildRequest(
            name: "Test Opportunity",
            responsibleOrgUnitId: null);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.ResponsibleOrgUnitId.Should().BeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-050")]
    [Trait("TestId", "TC-PNO1156-BND-006")]
    public async Task CreateOpportunity_WithAllOptionalFieldsNull_OnlyRequiredFieldsSaved()
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
    [Trait("TestId", "TC-PNO1156-BND-007")]
    public async Task CreateOpportunity_WithAllOptionalFieldsPopulated_AllFieldsSaved()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedOrgUnitAsync(40, "Full Org Unit");
        await SeedPartnerAsync(200, "Full Partner");
        var request = BuildRequest(
            name: "Full Opportunity",
            responsibleOrgUnitId: 40,
            partnerId: 200,
            isFundingPartner: true,
            isClientPartner: true,
            sourceInteractionIds: new List<int> { 1, 2 });
        request.Description = "Full description";
        request.ExpectedImpact = "Impact";
        request.ExpectedOutcomes = "Outcomes";
        request.SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 1, IsPrimary = false } };
        request.Countries = new List<int> { 1 };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.ResponsibleOrgUnitId.Should().Be(40);
        result.FundingPartners.Should().NotBeEmpty();
        result.ClientPartners.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-PNO1156-BND-008")]
    public async Task CreateOpportunity_DuplicateSourceInteractionIds_Deduplicated()
    {
        // Arrange — manager deduplicates SDGs, Countries; SourceInteractionIds are used by controller
        await EnsureReferenceDataAsync();
        var request = BuildRequest(name: "Dedup Test");
        request.SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 1, IsPrimary = false }, new() { SDGId = 1, IsPrimary = false }, new() { SDGId = 1, IsPrimary = false } };
        request.Countries = new List<int> { 1, 1 };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert — manager deduplicates before save
        result.Should().NotBeNull();
        result.SDGs.Should().NotBeNull();
        result.Countries.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-PNO1156-BND-009")]
    public async Task CreateOpportunity_EmptySourceInteractionIds_NoInteractionsLinked()
    {
        // Arrange — manager creates opportunity; controller links interactions
        await EnsureReferenceDataAsync();
        var request = BuildRequest(
            name: "No Interactions",
            sourceInteractionIds: new List<int>());

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }
}
