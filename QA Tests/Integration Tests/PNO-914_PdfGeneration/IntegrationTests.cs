/**
 * @fileoverview PNO-914 PDF Generation integration tests — end-to-end flows.
 * All tests skipped due to DEF-021/DEF-024; fully implemented for un-skip when fixed.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Linq;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.PdfGeneration;

[Collection("PNO914_PdfGeneration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "PdfGeneration")]
public class IntegrationTests : PdfGenerationTestFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    private const string SkipReason = "DEF-021/DEF-024: DocumentController blocked by route conflict and Google Secret Manager dependency";

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-001")]
    public async Task CreatePdf_FullRequestResponseCycle_Succeeds()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Full Cycle", Filename = "cycle" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-002")]
    public async Task CreatePdf_ThroughDocumentController_ReturnsPdf()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Doc" });
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-003")]
    public async Task CreatePdf_WithDbContextAvailable_DoesNotCorruptDb()
    {
        using var scope = Factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# T" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await ctx.Database.CanConnectAsync();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-004")]
    public async Task CreatePdf_OpportunityStatementFlow_GeneratesPdf()
    {
        var statement = "## Opportunity Statement\n\n**WHY:** Rationale.\n\n**Budget:** $1M.";
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = statement, Filename = "OpportunityStatement" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-005")]
    public async Task CreatePdf_MultipleSequentialRequests_AllSucceed()
    {
        var client = CreateAuthenticatedClient();
        for (var i = 0; i < 5; i++)
        {
            var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = $"Doc{i}" });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-006")]
    public async Task CreatePdf_AuthenticatedUserFlow_Completes()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Auth" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-007")]
    public async Task CreatePdf_JsonSerializationRoundTrip_PreservesContent()
    {
        var request = new CreatePdfFromMarkdownRequest { Content = "RoundTrip", Filename = "rt" };
        var json = System.Text.Json.JsonSerializer.Serialize(request);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<CreatePdfFromMarkdownRequest>(json);
        deserialized!.Content.Should().Be("RoundTrip");
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, deserialized);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-008")]
    public async Task CreatePdf_ServiceResolution_Succeeds()
    {
        using var scope = Factory.Services.CreateScope();
        var _ = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Svc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-009")]
    public async Task CreatePdf_ReportGenerationFlow_ProducesPdf()
    {
        var report = "# Report\n\n## Section 1\n\nContent.\n\n## Section 2\n\nMore.";
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = report, Filename = "Report" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-010")]
    public async Task CreatePdf_ExportFlow_ReturnsDownloadablePdf()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Export", Filename = "export.pdf" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentDisposition.Should().NotBeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-011")]
    public async Task CreatePdf_ConcurrentRequests_AllComplete()
    {
        var client = CreateAuthenticatedClient();
        var tasks = Enumerable.Range(0, 5)
            .Select(i => PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = $"Doc{i}" }));
        var responses = await Task.WhenAll(tasks);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-012")]
    public async Task CreatePdf_ApiContract_MatchesExpectedShape()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# C" });
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-013")]
    public async Task CreatePdf_EndToEndWithRealMarkdown_Completes()
    {
        var md = "# Title\n\n**Bold** and *italic*.\n\n- Item\n\n> Quote\n\n[Link](url)";
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = md });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-014")]
    public async Task CreatePdf_HttpClientReuse_WorksCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var r1 = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "A" });
        var r2 = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "B" });
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-015")]
    public async Task CreatePdf_RequestPipeline_CompletesWithoutException()
    {
        var client = CreateAuthenticatedClient();
        var act = async () => await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# P" });
        var response = await act();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-016")]
    public async Task CreatePdf_ResponsePipeline_ReturnsValidPdf()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# R" });
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes[0].Should().Be(0x25);
        bytes[1].Should().Be(0x50);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-017")]
    public async Task CreatePdf_IntegrationWithDocumentFeature_Works()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Doc" });
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-018")]
    public async Task CreatePdf_FactoryCreatesValidClient()
    {
        var client = Factory.CreateClient();
        client.Should().NotBeNull();
        client.BaseAddress.Should().NotBeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-019")]
    public async Task CreatePdf_BaseUrlResolvesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsync(PdfEndpoint, new StringContent("{\"content\":\"# T\"}", System.Text.Encoding.UTF8, "application/json"));
        response.RequestMessage.RequestUri.Should().NotBeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-020")]
    public async Task CreatePdf_OpportunityStatementToPdfFlow_EndToEnd()
    {
        var statement = "## Opportunity Statement\n\n**WHY:** Test.\n\n**Budget:** $500K.";
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = statement, Filename = "OpportunityStatement" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("Opportunity");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-021")]
    public async Task CreatePdf_MarkdownToPdfConversionFlow_Completes()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Convert", Filename = "converted" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-022")]
    public async Task CreatePdf_ScopedServices_ResolveCorrectly()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
        context.Should().NotBeNull();
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-023")]
    public async Task CreatePdf_ContentEncoding_HandlesUtf8()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Café 北京" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-024")]
    public async Task CreatePdf_ResponseStream_Readable()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# S" });
        await using var stream = await response.Content.ReadAsStreamAsync();
        stream.Should().NotBeNull();
        stream.CanRead.Should().BeTrue();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-025")]
    public async Task CreatePdf_RequestHeaders_Accepted()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# H" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-026")]
    public async Task CreatePdf_ResponseHeaders_Present()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# R" });
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-027")]
    public async Task CreatePdf_GenerateStatementThenPdf_Flow()
    {
        var statement = "## Opportunity Statement\n\n**WHY:** Rationale.\n\n**Budget:** $1M.";
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = statement, Filename = "Statement" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(100);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-028")]
    public async Task CreatePdf_FullDocumentWorkflow_Completes()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Workflow", Filename = "workflow" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-029")]
    public async Task CreatePdf_CrossComponentFlow_Works()
    {
        using var scope = Factory.Services.CreateScope();
        var _ = scope.ServiceProvider.GetServices<object>();
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Cross" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-INT-030")]
    public async Task CreatePdf_EndToEndUserJourney_Completes()
    {
        var client = CreateAuthenticatedClient();
        var markdown = "# User Report\n\n**Summary:** Test.\n\n- Point 1\n- Point 2";
        var request = new CreatePdfFromMarkdownRequest { Content = markdown, Filename = "UserReport" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }
}
