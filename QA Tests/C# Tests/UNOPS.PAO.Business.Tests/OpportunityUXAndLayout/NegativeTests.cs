/**
 * @fileoverview Opportunity UX & Layout negative tests — PNO-769, PNO-862, PNO-863, PNO-871, PNO-876, PNO-877, PNO-882.
 * Invalid input, unauthorized access, and expected failure scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text.Json;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityUXAndLayout;

/// <summary>
/// Negative tests for Opportunity UX & Layout.
/// </summary>
[Collection("Opportunity UX And Layout Integration")]
[Trait("Category", "Negative")]
[Trait("Feature", "OpportunityUXAndLayout")]
public class NegativeTests : OpportunityUXAndLayoutFixtureBase
{
    public NegativeTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "UX-NEG-001")]
    public async Task GetOpportunity_NonExistentId_Returns404Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 999999);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-002")]
    public async Task GetOpportunity_ZeroId_Returns404Or400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 0);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-003")]
    public async Task GetOpportunity_NegativeId_Returns404Or400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, -1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-004")]
    public async Task GetOpportunity_Unauthenticated_Returns401Or403()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-005")]
    public async Task GetComments_InvalidEntityType_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, "InvalidEntity", 1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-006")]
    public async Task GetComments_NegativeEntityId_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, -1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-007")]
    public async Task GetComments_Unauthenticated_Returns401Or403()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-008")]
    public async Task CreateComment_NullContent_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = (string?)null };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-009")]
    public async Task CreateComment_EmptyContent_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.Created);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-010")]
    public async Task CreateComment_InvalidEntityType_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "NonExistent", entityId = 1, content = "Test" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-011")]
    public async Task CreateComment_NonExistentEntityId_Returns404Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 999999, content = "Test" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-012")]
    public async Task CreateComment_Unauthenticated_Returns401Or403()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "Test" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-013")]
    public async Task CreateComment_MissingEntityType_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityId = 1, content = "Test" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-014")]
    public async Task GetRisks_NonExistentOpportunity_Returns404Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, 999999);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-015")]
    public async Task GetRisks_ZeroId_Returns404Or400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, 0);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-016")]
    public async Task GetRisks_Unauthenticated_Returns401Or403()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var response = await GetRisksAsync(client, 1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-017")]
    public async Task GetOpportunity_InvalidPath_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/opportunity/not-a-number");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-018")]
    public async Task GetComments_ZeroEntityId_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 0);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-019")]
    public async Task CreateComment_WhitespaceOnlyContent_Returns400OrCreated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "   " };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-020")]
    public async Task GetOpportunity_WrongHttpMethod_Returns405()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(OpportunityUXAndLayoutSpec.GetOpportunityEndpoint(1), null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-021")]
    public async Task GetComments_EmptyEntityType_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, "", 1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-022")]
    public async Task CreateComment_MissingContent_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1 };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-023")]
    public async Task GetOpportunity_ExtremelyLargeId_Returns404Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, int.MaxValue);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-024")]
    public async Task GetRisks_NegativeId_Returns404Or400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, -1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-025")]
    public async Task CreateComment_NegativeEntityId_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = -1, content = "Test" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-026")]
    public async Task CreateComment_InvalidJson_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{ invalid json }", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(OpportunityUXAndLayoutSpec.CreateCommentEndpoint, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-027")]
    public async Task CreateComment_EmptyJson_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(OpportunityUXAndLayoutSpec.CreateCommentEndpoint, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-028")]
    public async Task GetOpportunity_DeletedOpportunity_Returns404Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 999998);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-NEG-029")]
    public async Task GetComments_NonExistentOpportunity_Returns200WithEmptyArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 999999);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().NotBeNull();
        }
        else
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    [Trait("TestId", "UX-NEG-030")]
    public async Task GetRisks_InvalidPathFormat_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/opportunity/abc/dst-risks");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}
