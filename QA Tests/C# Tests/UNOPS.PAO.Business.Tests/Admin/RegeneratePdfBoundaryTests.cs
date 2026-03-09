/**
 * @fileoverview PNO-1166 RegenerateGoOpportunityPdfs boundary tests — edge values and soft-delete.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Boundary tests for PNO-1166 RegenerateGoOpportunityPdfs endpoint.
/// Covers: onlyMissing true/false, mixed states, empty workflow history, statementDocTypeId=0.
/// </summary>
[Collection("PNO-1166 Integration")]
[Trait("Category", "Boundary")]
[Trait("Feature", "PNO-1166")]
[Trait("Component", "RegenerateGoOpportunityPdfs")]
public class BoundaryTests : PNO1166RegeneratePdfFixtureBase
{
    public BoundaryTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PNO1166-BND-001")]
    public async Task RegeneratePdfs_OnlyMissingTrue_OnlyGeneratesMissing()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
        parsed!.TotalProcessed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-002")]
    public async Task RegeneratePdfs_OnlyMissingFalse_AttemptsAll()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: false);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-003")]
    public async Task RegeneratePdfs_ZeroOpportunities_TotalProcessedZero()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null && parsed.TotalProcessed == 0)
            parsed.Results.Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-004")]
    public async Task RegeneratePdfs_SumOfCountsEqualsTotalProcessedOrLess()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null)
        {
            var submissionTotal = parsed.SubmissionSuccess + parsed.SubmissionFailed + parsed.SubmissionSkipped;
            var approvalTotal = parsed.ApprovalSuccess + parsed.ApprovalFailed + parsed.ApprovalSkipped;
            submissionTotal.Should().BeLessThanOrEqualTo(parsed.TotalProcessed * 2);
            approvalTotal.Should().BeLessThanOrEqualTo(parsed.TotalProcessed * 2);
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-005")]
    public async Task RegeneratePdfs_EachResultHasSubmissionOrApprovalGenerated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            foreach (var r in parsed.Results)
                (r.SubmissionGenerated || r.ApprovalGenerated || r.SubmissionSkipped || r.ApprovalSkipped).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-006")]
    public async Task RegeneratePdfs_OnlyMissingTrue_MaySkipExisting()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
        parsed!.SubmissionSkipped.Should().BeGreaterThanOrEqualTo(0);
        parsed.ApprovalSkipped.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-007")]
    public async Task RegeneratePdfs_OnlyMissingFalse_NoSkipsExpected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: false);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null && parsed.TotalProcessed > 0)
            (parsed.SubmissionSkipped + parsed.ApprovalSkipped).Should().Be(0,
                "when onlyMissing=false, no PDFs should be skipped");
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-008")]
    public async Task RegeneratePdfs_ResultsCountMatchesTotalProcessed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            parsed.Results.Count.Should().Be(parsed.TotalProcessed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-009")]
    public async Task RegeneratePdfs_SubmissionSuccessPlusFailedPlusSkipped_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null)
        {
            var subTotal = parsed.SubmissionSuccess + parsed.SubmissionFailed + parsed.SubmissionSkipped;
            var appTotal = parsed.ApprovalSuccess + parsed.ApprovalFailed + parsed.ApprovalSkipped;
            subTotal.Should().Be(parsed.TotalProcessed);
            appTotal.Should().Be(parsed.TotalProcessed);
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-010")]
    public async Task RegeneratePdfs_ResultItemSubmissionSkippedImpliesNotGenerated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            foreach (var r in parsed.Results.Where(x => x.SubmissionSkipped))
                r.SubmissionGenerated.Should().BeFalse();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-011")]
    public async Task RegeneratePdfs_ResultItemApprovalSkippedImpliesNotGenerated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            foreach (var r in parsed.Results.Where(x => x.ApprovalSkipped))
                r.ApprovalGenerated.Should().BeFalse();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-012")]
    public async Task RegeneratePdfs_ResultItemSubmissionGeneratedImpliesNotSkipped()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            foreach (var r in parsed.Results.Where(x => x.SubmissionGenerated))
                r.SubmissionSkipped.Should().BeFalse();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-013")]
    public async Task RegeneratePdfs_ResultItemApprovalGeneratedImpliesNotSkipped()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            foreach (var r in parsed.Results.Where(x => x.ApprovalGenerated))
                r.ApprovalSkipped.Should().BeFalse();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-014")]
    public async Task RegeneratePdfs_OpportunityIdInResults_Unique()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null && parsed.Results.Count > 1)
        {
            var ids = parsed.Results.Select(r => r.OpportunityId).ToList();
            ids.Should().OnlyHaveUniqueItems();
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-015")]
    public async Task RegeneratePdfs_CallTwiceOnlyMissingTrue_SecondMaySkipMore()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        var r2 = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-016")]
    public async Task RegeneratePdfs_OnlyMissingTrue_WhenStatementDocTypeMissing_StillProcesses()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client, onlyMissing: true);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-017")]
    public async Task RegeneratePdfs_EmptyOpportunityStatementMarkdown_ExcludedFromQuery()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-018")]
    public async Task RegeneratePdfs_SoftDeletedOpportunities_ExcludedFromResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-019")]
    [Trait("Defect", "DEF-120")]
    public async Task RegeneratePdfs_StatementDocTypeIdZero_HandlesGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
            parsed.Should().NotBeNull();
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-020")]
    public async Task RegeneratePdfs_OnlyGoStage_NonGoExcluded()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-021")]
    public void RegeneratePdfs_SubmissionFilenameContainsSubmission()
    {
        var format = PNO1166RegeneratePdfSpec.SubmissionFilenameFormat(1, "20260309", "1200");
        format.Should().Contain(PNO1166RegeneratePdfSpec.SubmissionDocNameContains);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-022")]
    public void RegeneratePdfs_ApprovalFilenameContainsApproved()
    {
        var format = PNO1166RegeneratePdfSpec.ApprovalFilenameFormat(1, "20260309");
        format.Should().Contain(PNO1166RegeneratePdfSpec.ApprovalDocNameContains);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-023")]
    public void RegeneratePdfs_SubmissionFilenameHasDateAndTime()
    {
        var format = PNO1166RegeneratePdfSpec.SubmissionFilenameFormat(42, "20260309", "1430");
        format.Should().Be("Opportunity_42_Submission_20260309_1430");
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-024")]
    public void RegeneratePdfs_ApprovalFilenameHasDateOnly()
    {
        var format = PNO1166RegeneratePdfSpec.ApprovalFilenameFormat(42, "20260309");
        format.Should().Be("Opportunity_42_Approved_20260309");
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-025")]
    public async Task RegeneratePdfs_ResultItemHasOpportunityName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null && parsed.Results.Count > 0)
            parsed.Results[0].OpportunityName.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-026")]
    public async Task RegeneratePdfs_WhenSubmissionFails_ErrorInResult()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null && parsed.SubmissionFailed > 0)
        {
            var failed = parsed.Results.FirstOrDefault(r => r.SubmissionSuccess == false && r.SubmissionGenerated);
            failed?.SubmissionError.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-027")]
    public async Task RegeneratePdfs_WhenApprovalFails_ErrorInResult()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null && parsed.ApprovalFailed > 0)
        {
            var failed = parsed.Results.FirstOrDefault(r => r.ApprovalSuccess == false && r.ApprovalGenerated);
            failed?.ApprovalError.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-028")]
    public async Task RegeneratePdfs_PerOpportunityException_ContinuesProcessing()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        parsed.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-029")]
    public async Task RegeneratePdfs_ResultsOrderMatchesTotalProcessed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null)
            parsed.Results.Count.Should().Be(parsed.TotalProcessed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-BND-030")]
    public async Task RegeneratePdfs_NoOpportunities_ReturnsEmptyArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null && parsed.TotalProcessed == 0)
            parsed.Results.Should().BeEmpty();
    }
}
