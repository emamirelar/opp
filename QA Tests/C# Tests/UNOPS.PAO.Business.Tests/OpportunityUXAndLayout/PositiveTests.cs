/**
 * @fileoverview Opportunity UX & Layout positive tests — PNO-769, PNO-862, PNO-863, PNO-871, PNO-876, PNO-877, PNO-882.
 * Happy path scenarios for header, key info, quick stats, comments, and risks section.
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
/// Positive tests for Opportunity UX & Layout.
/// Requirements: PNO-769 AC1-AC5, PNO-871, PNO-876.
/// </summary>
[Collection("Opportunity UX And Layout Integration")]
[Trait("Category", "Positive")]
[Trait("Feature", "OpportunityUXAndLayout")]
public class PositiveTests : OpportunityUXAndLayoutFixtureBase
{
    public PositiveTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "UX-POS-001")]
    [Trait("AC", "PNO-769-AC1")]
    public async Task GetOpportunity_ValidId_Returns200WithHeaderFields()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        foreach (var field in OpportunityUXAndLayoutSpec.RequiredHeaderFields)
        {
            root.TryGetProperty(field, out _).Should().BeTrue($"Header field '{field}' should be present (PNO-769 AC1)");
        }
    }

    [Fact]
    [Trait("TestId", "UX-POS-002")]
    [Trait("AC", "PNO-769-AC1")]
    public async Task GetOpportunity_ReturnsIdAndName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("id");
        json.Should().Contain("name");
    }

    [Fact]
    [Trait("TestId", "UX-POS-003")]
    [Trait("AC", "PNO-769-AC4")]
    public async Task GetOpportunity_ReturnsKeyInformationFields()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("name", out _).Should().BeTrue();
        root.TryGetProperty("description", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "UX-POS-004")]
    [Trait("AC", "PNO-769-AC5")]
    public async Task GetOpportunity_ReturnsQuickStatsRelatedData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("fundingPartners", out _).Should().BeTrue();
        root.TryGetProperty("clientPartners", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "UX-POS-005")]
    [Trait("AC", "PNO-871")]
    public async Task GetComments_OpportunityEntity_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-POS-006")]
    [Trait("AC", "PNO-871")]
    public async Task CreateComment_ValidRequest_Returns201()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "Test comment from UX POS-006" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-POS-007")]
    [Trait("AC", "PNO-876")]
    public async Task GetRisks_ValidOpportunityId_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-POS-008")]
    [Trait("AC", "PNO-769-AC1")]
    public async Task GetOpportunity_ReturnsStageField()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("stage");
    }

    [Fact]
    [Trait("TestId", "UX-POS-009")]
    [Trait("AC", "PNO-769-AC1")]
    public async Task GetOpportunity_ReturnsTargetSigningDate()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("targetSigningDate");
    }

    [Fact]
    [Trait("TestId", "UX-POS-010")]
    [Trait("AC", "PNO-871")]
    public async Task GetComments_WithIncludeRepliesFalse_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1, includeReplies: false);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
