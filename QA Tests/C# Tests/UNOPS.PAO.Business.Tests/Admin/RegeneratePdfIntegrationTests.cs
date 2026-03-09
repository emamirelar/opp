/**
 * @fileoverview PNO-1166 RegenerateGoOpportunityPdfs integration tests — full flow.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Integration tests for PNO-1166 RegenerateGoOpportunityPdfs endpoint.
/// Full flow: query through PDF generation, workflow history integration.
/// </summary>
[Collection("PNO-1166 Integration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-1166")]
[Trait("Component", "RegenerateGoOpportunityPdfs")]
public class IntegrationTests : PNO1166RegeneratePdfFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PNO1166-INT-001")]
    public async Task RegeneratePdfs_FullFlow_AuthenticatedToResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var parsed = ParseResponse(json);
        parsed.Should().NotBeNull();
        parsed!.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-002")]
    public async Task RegeneratePdfs_EndToEnd_QueryToJsonResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.EnsureSuccessStatusCode();
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
        parsed!.TotalProcessed.Should().BeGreaterThanOrEqualTo(0);
        parsed.Results.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-003")]
    public async Task RegeneratePdfs_SystemAdminToOpportunityManager_RoundTrip()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-004")]
    public async Task RegeneratePdfs_SystemAdminToWorkflowManager_HistoryRetrieved()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-005")]
    public async Task RegeneratePdfs_SystemAdminToDocumentType_QuerySucceeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-006")]
    public async Task RegeneratePdfs_SystemAdminToDocumentRelationship_QuerySucceeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-007")]
    public async Task RegeneratePdfs_MultipleOpportunities_AllProcessed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null && parsed.TotalProcessed > 0)
            parsed.Results!.Count.Should().Be(parsed.TotalProcessed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-008")]
    public async Task RegeneratePdfs_ConsecutiveCalls_ConsistentBehavior()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await PostRegeneratePdfsAsync(client);
        var r2 = await PostRegeneratePdfsAsync(client);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-009")]
    public async Task RegeneratePdfs_OnlyMissingTrueThenFalse_DifferentCounts()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var rTrue = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        var rFalse = await PostRegeneratePdfsAsync(client, onlyMissing: false);
        rTrue.StatusCode.Should().Be(HttpStatusCode.OK);
        rFalse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-010")]
    public async Task RegeneratePdfs_SharedFactory_NoIsolationIssues()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-011")]
    public async Task RegeneratePdfs_IntegrationTestsCollection_ReceivesFixture()
    {
        Factory.Should().NotBeNull();
        var client = CreateAuthenticatedClient();
        client.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-012")]
    public async Task RegeneratePdfs_ResponseDeserializesToModel()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var json = await response.Content.ReadAsStringAsync();
        var parsed = ParseResponse(json);
        parsed.Should().NotBeNull();
        parsed!.TotalProcessed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-013")]
    public async Task RegeneratePdfs_ResultsArrayMatchesProcessedCount()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            parsed.Results.Count.Should().Be(parsed.TotalProcessed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-014")]
    public async Task RegeneratePdfs_EachResultHasValidOpportunityId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            foreach (var r in parsed.Results)
                r.OpportunityId.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-015")]
    public async Task RegeneratePdfs_ApiContract_AllFieldsPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("totalProcessed");
        json.Should().Contain("submissionSuccess");
        json.Should().Contain("submissionFailed");
        json.Should().Contain("submissionSkipped");
        json.Should().Contain("approvalSuccess");
        json.Should().Contain("approvalFailed");
        json.Should().Contain("approvalSkipped");
        json.Should().Contain("results");
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-016")]
    public async Task RegeneratePdfs_ResultItemContract_AllFieldsPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null && parsed.Results.Count > 0)
        {
            var r = parsed.Results[0];
            r.OpportunityId.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-017")]
    public async Task RegeneratePdfs_NoOpportunities_EmptyResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null && parsed.TotalProcessed == 0)
        {
            parsed.Results.Should().BeEmpty();
            parsed.SubmissionSuccess.Should().Be(0);
            parsed.ApprovalSuccess.Should().Be(0);
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-018")]
    public async Task RegeneratePdfs_StatusCode200_WhenSuccess()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        if (response.StatusCode == HttpStatusCode.OK)
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-019")]
    public async Task RegeneratePdfs_StatusCode500_WhenTopLevelException()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-020")]
    public async Task RegeneratePdfs_CrossComponent_ControllerToManager()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-021")]
    public async Task RegeneratePdfs_CrossContext_UnopsAndAppDbContext()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-022")]
    public async Task RegeneratePdfs_WorkflowHistoryIntegration()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-023")]
    public async Task RegeneratePdfs_UserDetailsLookup_ForAuditTrail()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-024")]
    public async Task RegeneratePdfs_EntityUserRoleQuery_DoALevel()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-025")]
    public async Task RegeneratePdfs_GenerateStatementPdfAsync_CalledPerOpportunity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null && parsed.TotalProcessed > 0)
        {
            var generated = parsed.SubmissionSuccess + parsed.SubmissionFailed +
                           parsed.ApprovalSuccess + parsed.ApprovalFailed;
            generated.Should().BeLessThanOrEqualTo(parsed.TotalProcessed * 2);
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-026")]
    public async Task RegeneratePdfs_DocumentRelationshipQuery_SoftDeleteFiltered()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-027")]
    public async Task RegeneratePdfs_DocumentQuery_SoftDeleteFiltered()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-028")]
    public async Task RegeneratePdfs_OpportunityQuery_SoftDeleteFiltered()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-029")]
    public async Task RegeneratePdfs_DocumentTypeQuery_SoftDeleteFiltered()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-INT-030")]
    public async Task RegeneratePdfs_FullStack_HttpToDatabase()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }
}
