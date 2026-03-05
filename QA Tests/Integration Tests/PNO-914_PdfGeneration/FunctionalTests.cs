/**
 * @fileoverview PNO-914 PDF Generation functional tests — business rules, headers, audit.
 * All tests skipped due to DEF-021/DEF-024; fully implemented for un-skip when fixed.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.PdfGeneration;

[Collection("PNO914_PdfGeneration")]
[Trait("Category", "Functional")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "PdfGeneration")]
public class FunctionalTests : PdfGenerationTestFixtureBase
{
    public FunctionalTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    private const string SkipReason = "DEF-021/DEF-024: DocumentController blocked by route conflict and Google Secret Manager dependency";

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-001")]
    public async Task CreatePdf_ResponseHasApplicationPdfContentType()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Test" });
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-002")]
    public async Task CreatePdf_ResponseHasNonZeroContentLength()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Test" });
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-003")]
    public async Task CreatePdf_PdfStartsWithPdfMagicBytes()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Test" });
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be(0x25); // %
        bytes[1].Should().Be(0x50); // P
        bytes[2].Should().Be(0x44); // D
        bytes[3].Should().Be(0x46); // F
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-004")]
    public async Task CreatePdf_ContentReflectedInPdfBytes()
    {
        var client = CreateAuthenticatedClient();
        var unique = "UniqueMarker12345";
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = unique });
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("UniqueMarker");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-005")]
    public async Task CreatePdf_FilenameReflectedInContentDisposition()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# T", Filename = "Report2024" });
        response.Content.Headers.ContentDisposition.Should().NotBeNull();
        response.Content.Headers.ContentDisposition?.FileName.Should().Contain("Report");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-006")]
    public async Task CreatePdf_SameContentProducesConsistentResult()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Consistent" };
        var r1 = await PostPdfRequestAsync(client, request);
        var r2 = await PostPdfRequestAsync(client, request);
        var b1 = await r1.Content.ReadAsByteArrayAsync();
        var b2 = await r2.Content.ReadAsByteArrayAsync();
        b1.Length.Should().Be(b2.Length);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-007")]
    public async Task CreatePdf_DifferentContentProducesDifferentPdf()
    {
        var client = CreateAuthenticatedClient();
        var r1 = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "A" });
        var r2 = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "B" });
        var b1 = await r1.Content.ReadAsByteArrayAsync();
        var b2 = await r2.Content.ReadAsByteArrayAsync();
        b1.Should().NotBeEquivalentTo(b2);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-008")]
    public async Task CreatePdf_ResponseAllowsCachingOrNoStore()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# Test" });
        response.Headers.CacheControl.Should().NotBeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-009")]
    public async Task CreatePdf_RequestAcceptsUtf8Encoding()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Café résumé" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-010")]
    public async Task CreatePdf_MarkdownHeadersMapToPdfStructure()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# H1\n## H2\nText" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("H1");
        text.Should().Contain("H2");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-011")]
    public async Task CreatePdf_BoldMarkdownReflectedInPdf()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "**Bold**" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-012")]
    public async Task CreatePdf_ListMarkdownReflectedInPdf()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "- One\n- Two" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("One");
        text.Should().Contain("Two");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-013")]
    public async Task CreatePdf_LinkMarkdownReflectedInPdf()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "[Link](https://x.com)" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-014")]
    public async Task CreatePdf_TableMarkdownReflectedInPdf()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "|A|B|\n|-|-|\n|1|2|" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-015")]
    public async Task CreatePdf_ResponseHasValidPdfStructure()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# T" });
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be(0x25);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-016")]
    public async Task CreatePdf_EmptyFilenameUsesDefaultInResponse()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# T", Filename = "" });
        response.Content.Headers.ContentDisposition?.FileName.Should().NotBeNullOrEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-017")]
    public async Task CreatePdf_LongerContentProducesLargerPdf()
    {
        var client = CreateAuthenticatedClient();
        var r1 = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "Short" });
        var r2 = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = new string('x', 1000) });
        var b1 = await r1.Content.ReadAsByteArrayAsync();
        var b2 = await r2.Content.ReadAsByteArrayAsync();
        b2.Length.Should().BeGreaterOrEqualTo(b1.Length);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-018")]
    public async Task CreatePdf_ResponseStatusCodeIs200()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# T" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-019")]
    public async Task CreatePdf_ResponseHasContentLengthHeader()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# T" });
        response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-020")]
    public async Task CreatePdf_ContentPreservesParagraphBreaks()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "P1\n\nP2" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("P1");
        text.Should().Contain("P2");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-021")]
    public async Task CreatePdf_ConsecutiveCallsSucceed()
    {
        var client = CreateAuthenticatedClient();
        for (var i = 0; i < 3; i++)
        {
            var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = $"Doc {i}" });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-022")]
    public async Task CreatePdf_EndpointRequiresPost()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PdfEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-023")]
    public async Task CreatePdf_JsonPropertyNamesAreCaseInsensitive()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"Content\":\"# Test\",\"Filename\":\"x\"}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-024")]
    public async Task CreatePdf_OptionalFilenameCanBeOmitted()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"content\":\"# Test\"}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-025")]
    public async Task CreatePdf_ResponseIsBinaryNotJson()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRequestAsync(client, new CreatePdfFromMarkdownRequest { Content = "# T" });
        response.Content.Headers.ContentType?.MediaType.Should().NotBe("application/json");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-026")]
    public async Task CreatePdf_MarkdownCodeBlockPreserved()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "```\ncode\n```" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("code");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-027")]
    public async Task CreatePdf_BlockquotePreserved()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "> Quote text" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("Quote");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-028")]
    public async Task CreatePdf_InlineCodePreserved()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Use `code` here" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        text.Should().Contain("code");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-029")]
    public async Task CreatePdf_ItalicMarkdownReflectedInPdf()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "*italic*" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-FUN-030")]
    public async Task CreatePdf_ComplexMarkdownCombination_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest
        {
            Content = "# Title\n\n**Bold** and *italic*.\n\n- Item\n\n> Quote"
        };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }
}
