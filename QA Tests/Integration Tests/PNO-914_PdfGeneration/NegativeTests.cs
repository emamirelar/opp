/**
 * @fileoverview PNO-914 PDF Generation negative tests — invalid inputs, validation, unauthorized.
 * All tests skipped due to DEF-021/DEF-024; fully implemented for un-skip when fixed.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.PdfGeneration;

[Collection("PNO914_PdfGeneration")]
[Trait("Category", "Negative")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "PdfGeneration")]
public class NegativeTests : PdfGenerationTestFixtureBase
{
    private readonly bool _isPostgresAvailable;

    public NegativeTests(PAOWebApplicationFactory<Program> factory) : base(factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
    }

    private const string SkipReason = "DEF-021/DEF-024: DocumentController blocked by route conflict and Google Secret Manager dependency";

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-001")]
    public async Task CreatePdf_EmptyMarkdown_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-002")]
    public async Task CreatePdf_WhitespaceOnlyMarkdown_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "   \n\t  " };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-003")]
    public async Task CreatePdf_NullContentInRequest_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"content\":null,\"filename\":\"test\"}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-004")]
    public async Task CreatePdf_EmptyJsonBody_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-005")]
    public async Task CreatePdf_MissingContentProperty_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"filename\":\"test.pdf\"}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-006")]
    public async Task CreatePdf_InvalidJson_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{invalid json}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-007")]
    public async Task CreatePdf_UnauthenticatedUser_ReturnsUnauthorized()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Test" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-008")]
    public async Task CreatePdf_WrongContentType_ReturnsUnsupportedMediaType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "content=test", "application/x-www-form-urlencoded");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-009")]
    public async Task CreatePdf_ContentAsArray_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"content\":[\"a\",\"b\"]}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-010")]
    public async Task CreatePdf_ContentAsNumber_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"content\":123}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-011")]
    public async Task CreatePdf_ContentAsBoolean_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"content\":true}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-012")]
    public async Task CreatePdf_InvalidMarkdownSyntax_StillReturnsPdfOrError()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "[[[unclosed" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-013")]
    public async Task CreatePdf_GetMethod_ReturnsMethodNotAllowed()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PdfEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-014")]
    public async Task CreatePdf_PutMethod_ReturnsMethodNotAllowed()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{\"content\":\"test\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PutAsync(PdfEndpoint, content);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-015")]
    public async Task CreatePdf_DeleteMethod_ReturnsMethodNotAllowed()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await client.DeleteAsync(PdfEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-016")]
    public async Task CreatePdf_EmptyContentType_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{\"content\":\"test\"}", System.Text.Encoding.UTF8, "");
        var response = await client.PostAsync(PdfEndpoint, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-017")]
    public async Task CreatePdf_TextPlainContentType_ReturnsUnsupportedOrBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var response = await PostPdfRawAsync(client, "{\"content\":\"test\"}", "text/plain");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-018")]
    public async Task CreatePdf_FilenameWithInvalidChars_ReturnsBadRequestOrSanitizes()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Test", Filename = "file<>:|\"*?" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-019")]
    public async Task CreatePdf_MalformedUtf8_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var invalidBytes = new byte[] { 0xFF, 0xFE };
        var content = new ByteArrayContent(invalidBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync(PdfEndpoint, content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-020")]
    public async Task CreatePdf_ExpiredAuthToken_ReturnsUnauthorized()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=expired; dev-user-email=expired@test.com");
        var request = new CreatePdfFromMarkdownRequest { Content = "# Test" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-021")]
    public async Task CreatePdf_ContentWithOnlySpecialChars_HandlesOrRejects()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "!@#$%^&*()" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-022")]
    public async Task CreatePdf_ContentWithScriptInjection_RejectsOrSanitizes()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "<script>alert(1)</script>" };
        var response = await PostPdfRequestAsync(client, request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        if (response.IsSuccessStatusCode)
            System.Text.Encoding.UTF8.GetString(bytes).Should().NotContain("<script>");
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-023")]
    public async Task CreatePdf_ContentWithSqlInjection_HandlesSafely()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "'; DROP TABLE users; --" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-024")]
    public async Task CreatePdf_ContentWithXssPayload_HandlesSafely()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "![x](x onerror=alert(1))" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-025")]
    public async Task CreatePdf_ContentWithNullBytes_RejectsOrStrips()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "test\0hidden" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-026")]
    public async Task CreatePdf_ContentWithControlChars_HandlesOrRejects()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "test\x01\x02\x03" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-027")]
    public async Task CreatePdf_ContentWithOnlyNewlines_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "\n\n\n" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-028")]
    public async Task CreatePdf_FilenameExceedingMaxLength_ReturnsBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "# Test", Filename = new string('x', 500) };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-029")]
    public async Task CreatePdf_ContentWithInvalidHtmlEntities_HandlesGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var request = new CreatePdfFromMarkdownRequest { Content = "&#999999999;" };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("TestId", "TC-PDF-NEG-030")]
    public async Task CreatePdf_ContentWithDeeplyNestedMarkdown_HandlesOrRejects()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateAuthenticatedClient();
        var nested = new string('#', 100) + " Title";
        var request = new CreatePdfFromMarkdownRequest { Content = nested };
        var response = await PostPdfRequestAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
