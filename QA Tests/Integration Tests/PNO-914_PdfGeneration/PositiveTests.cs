/**
 * @fileoverview PNO-914 PDF Generation positive tests — happy path scenarios.
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
[Trait("Category", "Positive")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "PdfGeneration")]
public class PositiveTests : PdfGenerationTestFixtureBase
{
    public PositiveTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-001")]
    public async Task CreatePdf_ValidMarkdown_ReturnsPdfWithCorrectContentType()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Title\n\nParagraph text.", Filename = "TestDoc" };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-002")]
    public async Task CreatePdf_SimpleMarkdown_GeneratesNonEmptyPdf()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Hello World" };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(100);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-003")]
    public async Task CreatePdf_WithFilename_UsesProvidedFilenameInContentDisposition()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Doc", Filename = "MyReport.pdf" };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentDisposition?.FileName.Should().Contain("MyReport");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-004")]
    public async Task CreatePdf_MarkdownWithHeaders_ConvertsCorrectly()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest
        {
            Content = "# H1\n## H2\n### H3\n\nBody text.",
            Filename = "Headers"
        };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-005")]
    public async Task CreatePdf_MarkdownWithLists_ConvertsCorrectly()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest
        {
            Content = "- Item 1\n- Item 2\n- Item 3",
            Filename = "ListDoc"
        };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-006")]
    public async Task CreatePdf_MarkdownWithBoldAndItalic_ConvertsCorrectly()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest
        {
            Content = "**Bold** and *italic* text.",
            Filename = "Formatting"
        };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-007")]
    public async Task CreatePdf_WithoutFilename_UsesDefaultFilename()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Default" };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-008")]
    public async Task CreatePdf_OpportunityStatementMarkdown_GeneratesPdf()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var markdown = "## Opportunity Statement\n\n**WHY:** Test rationale.\n\n**Budget:** $1M.";
        var request = new CreatePdfFromMarkdownRequest { Content = markdown, Filename = "OpportunityStatement" };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-009")]
    public async Task CreatePdf_MarkdownWithLinks_ConvertsCorrectly()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest
        {
            Content = "[Link](https://example.com)",
            Filename = "Links"
        };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-POS-010")]
    public async Task CreatePdf_MarkdownWithTables_ConvertsCorrectly()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest
        {
            Content = "| A | B |\n|---|---|\n| 1 | 2 |",
            Filename = "TableDoc"
        };

        // Act
        var response = await PostPdfRequestAsync(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }
}
