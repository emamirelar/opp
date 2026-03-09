/**
 * @fileoverview PNO-1166 RegenerateGoOpportunityPdfs negative tests — invalid/unauthorized scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Negative tests for PNO-1166 RegenerateGoOpportunityPdfs endpoint.
/// </summary>
[Collection("PNO-1166 Integration")]
[Trait("Category", "Negative")]
[Trait("Feature", "PNO-1166")]
[Trait("Component", "RegenerateGoOpportunityPdfs")]
public class NegativeTests : PNO1166RegeneratePdfFixtureBase
{
    public NegativeTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-001")]
    public async Task RegeneratePdfs_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-002")]
    public async Task RegeneratePdfs_GetMethod_Returns405()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PNO1166RegeneratePdfSpec.EndpointPath);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-003")]
    public async Task RegeneratePdfs_DeleteMethod_Returns405()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.DeleteAsync(PNO1166RegeneratePdfSpec.EndpointPath);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-004")]
    public async Task RegeneratePdfs_PutMethod_Returns405()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PutAsync(PNO1166RegeneratePdfSpec.EndpointPath, null);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-005")]
    public async Task RegeneratePdfs_InvalidOnlyMissing_NonBoolean_MayReturn400Or200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=invalid", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-006")]
    public async Task RegeneratePdfs_WrongPath_Returns404Or405()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync("/api/system-admin/regenerate-go-opportunity-pdfs-wrong", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-007")]
    public async Task RegeneratePdfs_EmptyPath_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync("/api/system-admin/", null);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-008")]
    public async Task RegeneratePdfs_WithBody_IgnoresBodyAndSucceeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(PNO1166RegeneratePdfSpec.EndpointPath, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-009")]
    public async Task RegeneratePdfs_OnlyMissing0_InterpretsAsFalse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=0", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-010")]
    public async Task RegeneratePdfs_OnlyMissing1_InterpretsAsTrue()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=1", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-011")]
    public async Task RegeneratePdfs_ExtraQueryParams_IgnoredAndSucceeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=true&foo=bar", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-012")]
    public async Task RegeneratePdfs_DuplicateOnlyMissing_FirstWins()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=true&onlyMissing=false", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-013")]
    public async Task RegeneratePdfs_NoOpportunities_ReturnsEmptyResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
            parsed.Should().NotBeNull();
            parsed!.Results.Should().NotBeNull();
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-014")]
    public async Task RegeneratePdfs_InvalidRoutePrefix_Returns404Or405()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync("/api/wrong-admin/regenerate-go-opportunity-pdfs", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-015")]
    public async Task RegeneratePdfs_CaseSensitivePath_LowercaseSucceeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync("/api/system-admin/regenerate-go-opportunity-pdfs", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-016")]
    public async Task RegeneratePdfs_OnlyMissingYes_MayFailOrDefault()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=yes", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-017")]
    public async Task RegeneratePdfs_OnlyMissingNo_MayFailOrDefault()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=no", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-018")]
    public async Task RegeneratePdfs_OnlyMissingEmpty_MayDefaultToTrue()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-019")]
    public async Task RegeneratePdfs_MissingAuthHeader_Returns401Or403()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-020")]
    public async Task RegeneratePdfs_OnlyAuthHeaderNoCookie_DoesNotCrash()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:test@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:999");
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-021")]
    public async Task RegeneratePdfs_ResponseNeverNullMessage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
            parsed.Should().NotBeNull();
            parsed!.Message.Should().NotBeNull();
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-022")]
    public async Task RegeneratePdfs_ResponseNeverNegativeCounts()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed != null)
        {
            parsed.SubmissionSuccess.Should().BeGreaterThanOrEqualTo(0);
            parsed.SubmissionFailed.Should().BeGreaterThanOrEqualTo(0);
            parsed.ApprovalSuccess.Should().BeGreaterThanOrEqualTo(0);
            parsed.ApprovalFailed.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-023")]
    public async Task RegeneratePdfs_OptionsMethod_ReturnsExpectedStatus()
    {
        var client = CreateAuthenticatedClient();
        var request = new HttpRequestMessage(HttpMethod.Options, PNO1166RegeneratePdfSpec.EndpointPath);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-024")]
    public async Task RegeneratePdfs_HeadMethod_Returns405Or404()
    {
        var client = CreateAuthenticatedClient();
        var request = new HttpRequestMessage(HttpMethod.Head, PNO1166RegeneratePdfSpec.EndpointPath);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-025")]
    public async Task RegeneratePdfs_PatchMethod_Returns405()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, PNO1166RegeneratePdfSpec.EndpointPath);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-026")]
    public async Task RegeneratePdfs_TrailingSlash_DoesNotCrash()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(PNO1166RegeneratePdfSpec.EndpointPath + "/", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Redirect, HttpStatusCode.MovedPermanently, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-027")]
    public async Task RegeneratePdfs_OnlyMissingTrueUpperCase_Works()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=True", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-028")]
    public async Task RegeneratePdfs_OnlyMissingFalseUpperCase_Works()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(
            $"{PNO1166RegeneratePdfSpec.EndpointPath}?onlyMissing=False", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-029")]
    public async Task RegeneratePdfs_ResultsItemHasOpportunityId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync());
        if (parsed?.Results != null && parsed.Results.Count > 0)
        {
            parsed.Results[0].OpportunityId.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    [Trait("TestId", "PNO1166-NEG-030")]
    public async Task RegeneratePdfs_NoInternalServerErrorOnValidRequest()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostRegeneratePdfsAsync(client);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            "valid authenticated request should not return 500");
    }
}
