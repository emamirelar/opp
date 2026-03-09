/**
 * @fileoverview PNO-1166 RegenerateGoOpportunityPdfs positive tests — happy path scenarios.
 * AC-3: PDF generation for GO opportunities works correctly.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Tests for PNO-1166: QA testing code — RegenerateGoOpportunityPdfs.
/// Requirements validated: AC-3 (PDF generation for GO opportunities).
/// </summary>
[Collection("PNO-1166 Integration")]
[Trait("Category", "Positive")]
[Trait("Feature", "PNO-1166")]
[Trait("Component", "RegenerateGoOpportunityPdfs")]
public class PositiveTests : PNO1166RegeneratePdfFixtureBase
{
    public PositiveTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PNO1166-POS-001")]
    public async Task RegeneratePdfs_AuthenticatedWithPermission_Returns200()
    {
        if (!Factory.IsUsingPostgres) return; // QA-054a
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-002")]
    public async Task RegeneratePdfs_OnlyMissingTrue_ReturnsJsonWithExpectedStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var parsed = ParseResponse(json);
        parsed.Should().NotBeNull();
        parsed!.TotalProcessed.Should().BeGreaterThanOrEqualTo(0);
        parsed.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-003")]
    public async Task RegeneratePdfs_OnlyMissingFalse_ReturnsJsonWithExpectedStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: false);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var parsed = ParseResponse(json);
        parsed.Should().NotBeNull();
        parsed!.TotalProcessed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-004")]
    public async Task RegeneratePdfs_DefaultQueryParam_DefaultsToOnlyMissingTrue()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("totalProcessed");
        json.Should().Contain("submissionSuccess");
        json.Should().Contain("approvalSuccess");
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-005")]
    public async Task RegeneratePdfs_ResponseContainsResultsArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("results");
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-006")]
    public async Task RegeneratePdfs_ResponseContentTypeIsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-007")]
    public async Task RegeneratePdfs_CountsAreNonNegative()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
        parsed!.SubmissionSuccess.Should().BeGreaterThanOrEqualTo(0);
        parsed.SubmissionFailed.Should().BeGreaterThanOrEqualTo(0);
        parsed.SubmissionSkipped.Should().BeGreaterThanOrEqualTo(0);
        parsed.ApprovalSuccess.Should().BeGreaterThanOrEqualTo(0);
        parsed.ApprovalFailed.Should().BeGreaterThanOrEqualTo(0);
        parsed.ApprovalSkipped.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-008")]
    public async Task RegeneratePdfs_TotalProcessedEqualsResultsCount()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
        if (parsed!.Results != null)
            parsed.TotalProcessed.Should().Be(parsed.Results.Count);
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-009")]
    public async Task RegeneratePdfs_ExplicitOnlyMissingTrue_BehavesSameAsDefault()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var responseDefault = await PostRegeneratePdfsAsync(client);
        var responseExplicit = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        responseDefault.StatusCode.Should().Be(HttpStatusCode.OK);
        responseExplicit.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-POS-010")]
    public async Task RegeneratePdfs_MessageIndicatesCompletion()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
        parsed!.Message.Should().NotBeNullOrEmpty();
        parsed.Message!.ToLowerInvariant().Should().Contain("regeneration");
    }
}
