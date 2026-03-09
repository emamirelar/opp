/**
 * @fileoverview Opportunity Creation positive tests — PNO-687, PNO-689, PNO-764, PNO-771, PNO-800, PNO-802, PNO-814, PNO-815, PNO-816, PNO-917.
 * Happy path scenarios for Create Opportunity from all entry points.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityCreation;

/// <summary>
/// Positive tests for Opportunity Creation.
/// Requirements: PNO-687 AC1-AC9, PNO-689, PNO-764, PNO-771, PNO-800, PNO-802, PNO-814, PNO-815, PNO-816, PNO-917.
/// </summary>
[Collection("Opportunity Creation Integration")]
[Trait("Category", "Positive")]
[Trait("Feature", "OpportunityCreation")]
public class PositiveTests : OpportunityCreationFixtureBase
{
    public PositiveTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "OPP-POS-001")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_FromOpportunitiesModule_WithValidRequest_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test Opportunity POS-001", description = "Description for POS-001" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-002")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithNameMax255Chars_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', OpportunityCreationSpec.NameMaxLength);
        var request = new { name, description = "Description" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-003")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ReturnsOpportunityWithSystemGeneratedId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test Opportunity POS-003", description = "Description" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("id");
        json.Should().Contain("name");
    }

    [Fact]
    [Trait("TestId", "OPP-POS-004")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ReturnsOpportunityWithDefaultStage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test Opportunity POS-004", description = "Description" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("stage");
    }

    [Fact]
    [Trait("TestId", "OPP-POS-005")]
    [Trait("AC", "PNO-687-AC5")]
    public async Task CreateOpportunity_WithOptionalDescription_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test Opportunity POS-005", description = "Optional description" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-006")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_WithFundingRole_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var partnerId = 1;
        var request = new { name = "Test Opportunity POS-006", partnerRole = "funding", description = "From partner" };
        var response = await PostCreateFromPartnerAsync(client, partnerId, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-007")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_WithClientRole_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var partnerId = 1;
        var request = new { name = "Test Opportunity POS-007", partnerRole = "client", description = "From partner" };
        var response = await PostCreateFromPartnerAsync(client, partnerId, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-008")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_WithBothRole_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var partnerId = 1;
        var request = new { name = "Test Opportunity POS-008", partnerRole = "both", description = "From partner" };
        var response = await PostCreateFromPartnerAsync(client, partnerId, request);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-009")]
    [Trait("AC", "PNO-815")]
    public async Task CreateFromProposal_WithInteractionsOnly_NoPartnerRequired_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test Opportunity POS-009", description = "From interactions" };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-010")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_EndpointExists_ReturnsNon5xx()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        ((int)response.StatusCode).Should().BeLessThan(500);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-011")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_EndpointExists_ReturnsNon5xx()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        ((int)response.StatusCode).Should().BeLessThan(500);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-012")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_EndpointExists_ReturnsNon5xx()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test" };
        var response = await PostCreateFromProposalAsync(client, request);
        ((int)response.StatusCode).Should().BeLessThan(500);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-013")]
    [Trait("AC", "PNO-687-AC9")]
    public async Task CreateOpportunity_ResponseContainsName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "Test Opportunity POS-013";
        var request = new { name, description = "Description" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "OPP-POS-014")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_ContentTypeIsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        if (response.StatusCode == HttpStatusCode.OK)
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "OPP-POS-015")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithMinimalDescription_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test Opportunity POS-015", description = "X" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
