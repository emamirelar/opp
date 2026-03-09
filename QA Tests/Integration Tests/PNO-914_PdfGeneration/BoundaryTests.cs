/**
 * @fileoverview PNO-914 PDF Generation boundary tests — edge cases, limits, special chars.
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
[Trait("Category", "Boundary")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "PdfGeneration")]
public class BoundaryTests : PdfGenerationTestFixtureBase
{
    public BoundaryTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    private const string SkipReason = "DEF-021/DEF-024: DocumentController blocked by route conflict and Google Secret Manager dependency";

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-001")]
    public async Task CreatePdf_SingleCharacterContent_GeneratesPdf()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "x" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-002")]
    public async Task CreatePdf_VeryLongMarkdown_HandlesWithinLimits()
    {
        var client = CreateAuthenticatedClient();
        var longContent = "# Title\n\n" + new string('A', 50000);
        var request = new CreatePdfFromMarkdownRequest { Content = longContent };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-003")]
    public async Task CreatePdf_MaxReasonableLength_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var content = new string('x', 100000);
        var request = new CreatePdfFromMarkdownRequest { Content = content };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-004")]
    public async Task CreatePdf_UnicodeContent_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Déclaration 北京 العربية" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-005")]
    public async Task CreatePdf_EmojiInContent_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Test 📄 PDF" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-006")]
    public async Task CreatePdf_SpecialCharacters_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "© ® ™ € £ ¥" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-007")]
    public async Task CreatePdf_EmptyFilename_AcceptsOrUsesDefault()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Test", Filename = "" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-008")]
    public async Task CreatePdf_NullFilename_AcceptsOrUsesDefault()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Test", Filename = null };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-009")]
    public async Task CreatePdf_FilenameWithExtension_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Test", Filename = "report.pdf" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-010")]
    public async Task CreatePdf_ContentWithOnlyWhitespace_HandlesOrRejects()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = " " };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-011")]
    public async Task CreatePdf_MarkdownWithManyNewlines_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Line1\n\n\n\n\nLine2" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-012")]
    public async Task CreatePdf_ContentWithMixedLineEndings_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "A\r\nB\rC\nD" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-013")]
    public async Task CreatePdf_ContentWithRtlText_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "مرحبا بالعالم" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-014")]
    public async Task CreatePdf_ContentAtExactMinLength_GeneratesPdf()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "a" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-015")]
    public async Task CreatePdf_ContentWithHtmlTags_ConvertsOrEscapes()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "<p>Hello</p>" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-016")]
    public async Task CreatePdf_ContentWithCodeBlock_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "```\ncode\n```" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-017")]
    public async Task CreatePdf_ContentWithBlockquote_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "> Quote" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-018")]
    public async Task CreatePdf_ContentWithHorizontalRule_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "---" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-019")]
    public async Task CreatePdf_ContentWithEscapedChars_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "\\*not bold\\*" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-020")]
    public async Task CreatePdf_ContentWithVeryLongWord_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = new string('a', 10000) };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-021")]
    public async Task CreatePdf_ContentWithMixedLanguages_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "English 中文 日本語 한국어" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-022")]
    public async Task CreatePdf_ContentWithZeroWidthChars_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "test\u200Bhidden" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-023")]
    public async Task CreatePdf_ContentWithSurrogatePairs_HandlesCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "Test \uD83D\uDE00" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-024")]
    public async Task CreatePdf_ContentWithJsonLikeStructure_ConvertsAsText()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "{\"key\":\"value\"}" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-025")]
    public async Task CreatePdf_ContentWithUrl_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "https://example.com/path?q=1" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-026")]
    public async Task CreatePdf_ContentWithBackticks_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "`inline code`" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-027")]
    public async Task CreatePdf_ContentWithStrikethrough_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "~~strikethrough~~" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-028")]
    public async Task CreatePdf_ContentWithNestedLists_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "- A\n  - B\n  - C" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-029")]
    public async Task CreatePdf_ContentWithOrderedList_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "1. First\n2. Second" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-BND-030")]
    public async Task CreatePdf_ContentWithImageSyntax_ConvertsCorrectly()
    {
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "![alt](url)" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
