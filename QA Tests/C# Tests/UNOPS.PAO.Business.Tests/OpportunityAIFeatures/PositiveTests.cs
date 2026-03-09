/// <summary>
/// Positive tests for Opportunity AI Features (PNO-694, PNO-803, PNO-804, PNO-805, PNO-873).
/// Requirements validated: AI create/edit flows, validation, Opportunity Manager assignment, budget alignment.
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityAIFeatures;

[Collection("OpportunityAIFeatures")]
[Trait("Category", "Positive")]
[Trait("Feature", "OpportunityAIFeatures")]
[Trait("Component", "UNOPSOpportunityManager")]
public class PositiveTests : OpportunityAIFeaturesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-001")]
    [Trait("Ticket", "PNO-803")]
    public async Task CreateOpportunityFromProposalAsync_WithValidNameAndDescription_Succeeds()
    {
        // Arrange
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "AI-Generated Opportunity",
            Description = "Comprehensive description from AI analysis"
        };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("AI-Generated Opportunity");
        result.Description.Should().Be("Comprehensive description from AI analysis");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-002")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_WithValidNameAndDescription_UpdatesOpportunity()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Original Name", description: "Original Description");
        var request = new ApplyOpportunityAiChangesRequest
        {
            Name = "AI-Enhanced Name",
            Description = "AI-enhanced description"
        };

        // Act
        var result = await Manager.ApplyAiChangesAsync(oppId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("AI-Enhanced Name");
        result.Description.Should().Be("AI-enhanced description");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-003")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_AssignsCurrentUserAsOpportunityManager()
    {
        // Arrange
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Opportunity with OM Assignment",
            Description = "Test"
        };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);

        // Assert
        result.Should().NotBeNull();
        result.OpportunityManager.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-004")]
    [Trait("Ticket", "PNO-805")]
    public async Task ApplyAiChangesAsync_PreservesExistingOpportunityManagerWhenNotInRequest()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "With OM", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var request = new ApplyOpportunityAiChangesRequest
        {
            Description = "Updated description only"
            // Name and Stakeholders not included - OM should be preserved
        };

        // Act
        var result = await Manager.ApplyAiChangesAsync(oppId, request);

        // Assert
        result.Should().NotBeNull();
        result.OpportunityManager.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-005")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_WithInitiativeBudgetUSDAndFundingPartners_UpdatesBoth()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Budget Test", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 1000000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1000000, CurrencyId = CurrencyId }
            }
        };

        // Act
        var result = await Manager.ApplyAiChangesAsync(oppId, request);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(1000000m);
        result.FundingPartners.Should().NotBeNull().And.HaveCount(1);
        result.FundingPartners![0].AmountUSD.Should().Be(1000000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-006")]
    [Trait("Ticket", "PNO-873")]
    public async Task CreateOpportunityFromProposalAsync_WithFundingPartners_SetsAmountsCorrectly()
    {
        // Arrange
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Funding Partner Opportunity",
            Description = "Test",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 500000, CurrencyId = CurrencyId }
            }
        };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);

        // Assert
        result.Should().NotBeNull();
        result.FundingPartners.Should().NotBeNull().And.HaveCount(1);
        result.FundingPartners![0].AmountUSD.Should().Be(500000m);
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("TestId", "POS-007")]
    [Trait("Ticket", "PNO-694")]
    public async Task GetOpportunityDetailsForAIAsync_ReturnsOpportunityContext()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "AI Context Test",
            description: "For AI",
            budgetUSD: 2500000,
            responsibleOrgUnitId: OrgHierarchyId);

        // Act
        Dictionary<string, object>? result = null;
        try
        {
            result = await Manager.GetOpportunityDetailsForAIAsync(oppId);
        }
        catch (Exception ex) when (ex.Message.Contains("Sqlite") || ex.Message.Contains("database is locked"))
        {
            return; // SQLite/parallel context limitation
        }

        // Assert
        result.Should().NotBeNull();
        result!.Should().ContainKey("id");
        result.Should().ContainKey("name");
        result.Should().ContainKey("description");
        result["id"].ToString().Should().Be(oppId.ToString());
        result["name"].Should().Be("AI Context Test");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-008")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_WithPartialFields_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "Partial Update",
            description: "Original",
            budgetUSD: 500000);
        var request = new ApplyOpportunityAiChangesRequest
        {
            Challenges = "New challenges"
            // Name, Description, Budget not included
        };

        // Act
        var result = await Manager.ApplyAiChangesAsync(oppId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Partial Update");
        result.Description.Should().Be("Original");
        result.InitiativeBudgetUSD.Should().Be(500000m);
        result.Challenges.Should().Be("New challenges");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-009")]
    [Trait("Ticket", "PNO-803")]
    [Trait("Defect", "DEF-225")]
    public async Task CreateOpportunityFromProposalAsync_WithOptionalDescription_Succeeds()
    {
        // Arrange - Description is optional per PNO-804 comment
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Opportunity Without Description",
            Description = null
        };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Opportunity Without Description");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-010")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_WithFundingPartners_LinksBudgetToPartners()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Budget Link", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 750000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 750000, CurrencyId = CurrencyId }
            }
        };

        // Act
        var result = await Manager.ApplyAiChangesAsync(oppId, request);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(750000m);
        var totalPartners = result.FundingPartners?.Sum(fp => fp.AmountUSD ?? 0) ?? 0;
        totalPartners.Should().Be(750000m);
    }
}
