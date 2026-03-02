/**
 * @fileoverview PNO-1156 Positive Tests — Responsible Org Unit in Create Opportunity from Interactions dialog.
 * Verifies happy-path CreateOpportunityFromProposalAsync with ResponsibleOrgUnitId.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1156;

[Collection("PNO1156_Positive")]
[Trait("Category", "Positive")]
[Trait("Feature", "PNO-1156")]
[Trait("Component", "ResponsibleOrgUnitCreateDialog")]
public class PositiveTests : PNO1156TestFixtureBase
{
    [Fact]
    [Trait("TestId", "TC-PNO1156-POS-001")]
    public async Task CreateOpportunity_WithResponsibleOrgUnit_SavesOrgUnitId()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedOrgUnitAsync(10, "South Asia Hub");

        var request = BuildRequest(
            name: "Opportunity with Org Unit",
            responsibleOrgUnitId: 10);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.ResponsibleOrgUnitId.Should().Be(10);

        await VerifyOpportunityHasOrgUnitAsync(result.Id, 10);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1156-POS-002")]
    public async Task CreateOpportunity_WithOrgUnitAndPartner_SavesBoth()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedOrgUnitAsync(20, "Bangladesh Office");
        await SeedPartnerAsync(100, "Test Partner");

        var request = BuildRequest(
            name: "Opportunity with Org Unit and Partner",
            responsibleOrgUnitId: 20,
            partnerId: 100,
            isFundingPartner: true);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.ResponsibleOrgUnitId.Should().Be(20);
        result.FundingPartners.Should().NotBeEmpty();
        result.FundingPartners!.First().PartnerId.Should().Be(100);

        await VerifyOpportunityHasOrgUnitAsync(result.Id, 20);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1156-POS-003")]
    public async Task CreateOpportunity_RequestHasValidName_OpportunityCreated()
    {
        // Arrange
        await EnsureReferenceDataAsync();
        await SeedOrgUnitAsync(30, "Regional Hub");

        var request = BuildRequest(
            name: "Valid Name Opportunity",
            responsibleOrgUnitId: 30);

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, CurrentUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Valid Name Opportunity");
        result.ResponsibleOrgUnitId.Should().Be(30);
    }
}
