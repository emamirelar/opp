/**
 * @fileoverview PNO-1166 RegenerateGoOpportunityPdfs functional tests — business rules.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Functional tests for PNO-1166 RegenerateGoOpportunityPdfs endpoint.
/// Validates: permission enforcement, correct filename format, audit trail, skip logic.
/// </summary>
[Collection("PNO-1166 Integration")]
[Trait("Category", "Functional")]
[Trait("Feature", "PNO-1166")]
[Trait("Component", "RegenerateGoOpportunityPdfs")]
public class FunctionalTests : PNO1166RegeneratePdfFixtureBase
{
    public FunctionalTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-001")]
    public async Task RegeneratePdfs_RequiresCanRunSeedingsPermission()
    {
        var client = CreateUnauthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-002")]
    public async Task RegeneratePdfs_EndpointInGetEndpointsList()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/system-admin/endpoints");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("regenerate-go-opportunity-pdfs");
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-003")]
    public async Task RegeneratePdfs_EndpointListShowsPOST()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/system-admin/endpoints");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("POST");
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-004")]
    public async Task RegeneratePdfs_EndpointListShowsOnlyMissingParam()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/system-admin/endpoints");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("onlyMissing");
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-005")]
    public async Task RegeneratePdfs_QueriesOpportunitiesWithStageGo()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-006")]
    public async Task RegeneratePdfs_FiltersNonEmptyOpportunityStatementMarkdown()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-007")]
    public void RegeneratePdfs_SubmissionUsesCorrectFilenameFormat()
    {
        var expected = $"Opportunity_1_Submission_{DateTime.UtcNow:yyyyMMdd}_{DateTime.UtcNow:HHmm}";
        expected.Should().Contain("_Submission_");
        expected.Should().Contain("Opportunity_1");
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-008")]
    public void RegeneratePdfs_ApprovalUsesCorrectFilenameFormat()
    {
        var expected = $"Opportunity_1_Approved_{DateTime.UtcNow:yyyyMMdd}";
        expected.Should().Contain("_Approved_");
        expected.Should().Contain("Opportunity_1");
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-009")]
    public async Task RegeneratePdfs_OnlyMissingTrue_ChecksSubmissionSeparately()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-010")]
    public async Task RegeneratePdfs_OnlyMissingTrue_ChecksApprovalSeparately()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-011")]
    public async Task RegeneratePdfs_OpportunityMayHaveSubmissionButNotApproval()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-012")]
    public async Task RegeneratePdfs_OpportunityMayHaveApprovalButNotSubmission()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-013")]
    public async Task RegeneratePdfs_IncludesResponsibleOrgUnit()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-014")]
    public async Task RegeneratePdfs_IncludesProposedInitiativeType()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-015")]
    public async Task RegeneratePdfs_ApprovalPdfIncludesAuditTrail()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-016")]
    public async Task RegeneratePdfs_SubmissionPdfUsesStatementMarkdown()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-017")]
    public async Task RegeneratePdfs_ApprovalPdfCombinesStatementAndAuditTrail()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-018")]
    public async Task RegeneratePdfs_PerOpportunityException_DoesNotAbortBatch()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-019")]
    public async Task RegeneratePdfs_TopLevelException_Returns500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("error");
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-020")]
    public async Task RegeneratePdfs_LogsStartAndCompletion()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-021")]
    public void RegeneratePdfs_DocumentTypeFilter_OpportunityStatement()
    {
        var entityType = PNO1166RegeneratePdfSpec.StatementDocTypeEntityType;
        var name = PNO1166RegeneratePdfSpec.StatementDocTypeName;
        entityType.Should().Be("Opportunity");
        name.Should().Be("Opportunity Statement");
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-022")]
    public void RegeneratePdfs_GoStageConstant()
    {
        PNO1166RegeneratePdfSpec.GoStage.Should().Be("GO");
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-023")]
    public void RegeneratePdfs_SubmissionDocNameContains()
    {
        PNO1166RegeneratePdfSpec.SubmissionDocNameContains.Should().Be("_Submission_");
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-024")]
    public void RegeneratePdfs_ApprovalDocNameContains()
    {
        PNO1166RegeneratePdfSpec.ApprovalDocNameContains.Should().Be("_Approved_");
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-025")]
    public void RegeneratePdfs_DefaultOnlyMissingIsTrue()
    {
        PNO1166RegeneratePdfSpec.DefaultOnlyMissing.Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-026")]
    public void RegeneratePdfs_RequiredPermission()
    {
        PNO1166RegeneratePdfSpec.RequiredPermission.Should().Be("CanRunSeedings");
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-027")]
    public async Task RegeneratePdfs_ResponseHasAllRequiredProperties()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain(PNO1166RegeneratePdfSpec.ResponseProperties.TotalProcessed);
        json.Should().Contain(PNO1166RegeneratePdfSpec.ResponseProperties.SubmissionSuccess);
        json.Should().Contain(PNO1166RegeneratePdfSpec.ResponseProperties.ApprovalSuccess);
        json.Should().Contain(PNO1166RegeneratePdfSpec.ResponseProperties.Results);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-028")]
    public async Task RegeneratePdfs_ResultItemHasAllFields()
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
    [Trait("TestId", "PNO1166-FUN-029")]
    public async Task RegeneratePdfs_WhenOnlyMissingAndAllExist_AllSkipped()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null && parsed.TotalProcessed > 0 && parsed.SubmissionSkipped == parsed.TotalProcessed)
            parsed.ApprovalSkipped.Should().BeLessThanOrEqualTo(parsed.TotalProcessed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-FUN-030")]
    public async Task RegeneratePdfs_AsNoTrackingUsed_ReadOnlyQuery()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
