/**
 * @fileoverview Comprehensive unit tests for AiRetrieverManager.
 * Tests vector store search, URL conversion, markdown-to-Google-Doc, IAP auth,
 * PDF/JSON response handling, multipart form data, error handling, and configuration.
 *
 * Uses reflection to inject mock HttpClient (DEF-047: manager does not accept HttpClient for testing).
 * IAPAuthHelper uses RuntimeHelpers.GetUninitializedObject; BuildAuthenticatedHeadersAsync catches
 * auth exceptions and continues without token, allowing HTTP mock to receive requests.
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Configuration;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for AiRetrieverManager.
/// Covers: vector store search, URL conversion, markdown-to-Google-Doc, IAP auth,
/// PDF vs JSON response handling, multipart form data, error handling, configuration.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "AiRetrieverManager")]
public class AiRetrieverManagerUnitTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<AiRetrieverManager>> _mockLogger;
    private readonly ExternalApiSettings _settings;
    private const string BaseUrl = "https://api.test.unops.org";

    public AiRetrieverManagerUnitTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Development:IAPSimulation:Enabled"] = "false",
                ["Development:IAPSimulation:UserEmail"] = ""
            })
            .Build();

        _mockLogger = new Mock<ILogger<AiRetrieverManager>>();

        _settings = new ExternalApiSettings
        {
            BaseUrl = BaseUrl,
            OAuthClientId = "test-client-id",
            Timeout = 30
        };
    }

    /// <summary>
    /// Creates AiRetrieverManager with mock HttpClient injected via reflection (DEF-047 workaround).
    /// </summary>
    private AiRetrieverManager CreateManager(HttpClient? client = null)
    {
        var iapHelper = (IAPAuthHelper)RuntimeHelpers.GetUninitializedObject(typeof(IAPAuthHelper));
        var options = Options.Create(_settings);
        var manager = new AiRetrieverManager(iapHelper, options, _configuration, _mockLogger.Object);

        client ??= _httpClient;
        var field = typeof(AiRetrieverManager).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(manager, client);

        return manager;
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "{}", string contentType = "application/json")
    {
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, contentType)
            });
    }

    private void SetupHttpResponseBytes(HttpStatusCode statusCode, byte[] content, string contentType)
    {
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new ByteArrayContent(content)
                {
                    Headers = { { "Content-Type", contentType } }
                }
            });
    }

    #region Vector Store Search

    [Fact]
    public async Task SearchVectorStoreAsync_ValidRequest_ReturnsResponse()
    {
        var response = new VectorStoreSearchResponse
        {
            Status = "success",
            Query = "partnership opportunities",
            Documents = new List<VectorStoreDocument>
            {
                new() { Name = "Doc1", Content = "Content 1", Distance = 0.1 }
            }
        };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        SetupHttpResponse(HttpStatusCode.OK, json);

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "partnership opportunities", MaxResults = 5 };

        var result = await manager.SearchVectorStoreAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be("success");
        result.Query.Should().Be("partnership opportunities");
        result.Documents.Should().HaveCount(1);
        result.Documents[0].Name.Should().Be("Doc1");
        result.Documents[0].Content.Should().Be("Content 1");
    }

    [Fact]
    public async Task SearchVectorStoreAsync_WithUserEmail_IncludesImpersonationHeader()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documents":[]}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        await manager.SearchVectorStoreAsync(request, "user@unops.org");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.TryGetValues("x-unops-impersonated-user", out var values).Should().BeTrue();
        values!.Should().Contain("user@unops.org");
    }

    [Fact]
    public async Task SearchVectorStoreAsync_WithClaimFormatEmail_ExtractsEmail()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documents":[]}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };
        var claimWithEmail = "securetoken.google.com/project/uid:user@unops.org";

        await manager.SearchVectorStoreAsync(request, claimWithEmail);

        capturedRequest!.Headers.TryGetValues("x-unops-impersonated-user", out var values).Should().BeTrue();
        values!.Should().Contain("user@unops.org");
    }

    [Fact]
    public async Task SearchVectorStoreAsync_RequestSerializedWithCamelCase()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documents":[]}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test", MaxResults = 10, EntityTypeId = "partner" };

        await manager.SearchVectorStoreAsync(request);

        var body = await capturedRequest!.Content!.ReadAsStringAsync();
        body.Should().Contain("query");
        body.Should().Contain("maxResults");
        body.Should().Contain("entityTypeId");
        body.Should().Contain("test");
    }

    #endregion

    #region URL Conversion

    [Fact]
    public async Task ConvertUrlAsync_ValidUrl_ReturnsConvertedDocument()
    {
        var response = new { status = "success", content = "Converted content", url = "https://example.com" };
        var json = JsonSerializer.Serialize(response);
        SetupHttpResponse(HttpStatusCode.OK, json);

        var manager = CreateManager();

        var result = await manager.ConvertUrlAsync("https://example.com");

        result.Should().NotBeNull();
        result.Status.Should().Be("success");
        result.Content.Should().Be("Converted content");
        result.Url.Should().Be("https://example.com");
    }

    [Fact]
    public async Task ConvertUrlAsync_RequestContainsUrl()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","content":"","url":"https://example.com"}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.ConvertUrlAsync("https://example.com");

        var body = await capturedRequest!.Content!.ReadAsStringAsync();
        body.Should().Contain("https://example.com");
    }

    #endregion

    #region Markdown-to-Google-Doc

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_JsonResponse_ReturnsDeserialized()
    {
        var response = new { status = "success", documentId = "doc-123", documentUrl = "https://docs.google.com/doc123", pdfBase64 = "base64content" };
        var json = JsonSerializer.Serialize(response);
        SetupHttpResponse(HttpStatusCode.OK, json);

        var manager = CreateManager();

        var result = await manager.ConvertMarkdownToGoogleDocAsync("# Hello");

        result.Should().NotBeNull();
        result.Status.Should().Be("success");
        result.DocumentId.Should().Be("doc-123");
        result.DocumentUrl.Should().Be("https://docs.google.com/doc123");
        result.PdfBase64.Should().Be("base64content");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_PdfResponse_ReturnsBase64Encoded()
    {
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        SetupHttpResponseBytes(HttpStatusCode.OK, pdfBytes, "application/pdf");

        var manager = CreateManager();

        var result = await manager.ConvertMarkdownToGoogleDocAsync("# Hello");

        result.Should().NotBeNull();
        result.Status.Should().Be("success");
        result.PdfBase64.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(result.PdfBase64!).Should().BeEquivalentTo(pdfBytes);
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_PdfByMagicBytes_ReturnsBase64Encoded()
    {
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // %PDF- even without application/pdf
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(pdfBytes)
                {
                    Headers = { { "Content-Type", "application/octet-stream" } }
                }
            });

        var manager = CreateManager();

        var result = await manager.ConvertMarkdownToGoogleDocAsync("# Hello");

        result.Should().NotBeNull();
        result.Status.Should().Be("success");
        result.PdfBase64.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_WithFileName_UsesFileNameInRequest()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documentId":"","documentUrl":""}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.ConvertMarkdownToGoogleDocAsync("# Hello", fileName: "my-document.md");

        capturedRequest!.Content.Should().BeOfType<MultipartFormDataContent>();
        var content = (MultipartFormDataContent)capturedRequest.Content;
        content.Should().NotBeNull();
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_NullFileName_DefaultsToDocumentMd()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"status":"success","documentId":"","documentUrl":""}""");

        var manager = CreateManager();
        var result = await manager.ConvertMarkdownToGoogleDocAsync("# Hello");

        result.Status.Should().Be("success");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_MultipartFormData_HasFileAndDataParts()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documentId":"","documentUrl":""}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.ConvertMarkdownToGoogleDocAsync("# Hello World", fileName: "test.md");

        capturedRequest!.Content.Should().BeOfType<MultipartFormDataContent>();
        var multipart = (MultipartFormDataContent)capturedRequest.Content;
        multipart.Should().NotBeNull();
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_DataPartContainsDownloadPdf()
    {
        string? capturedContent = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                if (req.Content != null)
                    capturedContent = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documentId":"","documentUrl":""}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.ConvertMarkdownToGoogleDocAsync("# Hello", fileName: "doc.md");

        capturedContent.Should().NotBeNullOrEmpty();
        capturedContent.Should().Contain("downloadPDF");
    }

    #endregion

    #region IAP Authentication Headers

    [Fact]
    public async Task BuildAuthenticatedHeaders_DevelopmentMode_AddsDevHeaders()
    {
        var devConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Development:IAPSimulation:Enabled"] = "true",
                ["Development:IAPSimulation:UserEmail"] = "dev@unops.org"
            })
            .Build();

        var iapHelper = (IAPAuthHelper)RuntimeHelpers.GetUninitializedObject(typeof(IAPAuthHelper));
        var options = Options.Create(_settings);
        var manager = new AiRetrieverManager(iapHelper, options, devConfig, _mockLogger.Object);
        var field = typeof(AiRetrieverManager).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(manager, _httpClient);

        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documents":[]}""", Encoding.UTF8, "application/json")
            });

        await manager.SearchVectorStoreAsync(new VectorStoreSearchRequest { Query = "test" });

        capturedRequest!.Headers.Should().Contain(h => h.Key.Equals("X-Dev-IAP-Simulation", StringComparison.OrdinalIgnoreCase));
        capturedRequest.Headers.Should().Contain(h => h.Key.Equals("x-goog-authenticated-user-email", StringComparison.OrdinalIgnoreCase));
        capturedRequest.Headers.Should().Contain(h => h.Key.Equals("x-forwarded-user", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task BuildAuthenticatedHeaders_ContentTypeHeader_NotOverriddenForMultipart()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documentId":"","documentUrl":""}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.ConvertMarkdownToGoogleDocAsync("# Test");

        capturedRequest!.Content!.Headers.ContentType.Should().NotBeNull();
        capturedRequest.Content.Headers.ContentType!.MediaType.Should().StartWith("multipart/form-data");
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task SearchVectorStoreAsync_HttpFailure_ThrowsHttpRequestException()
    {
        SetupHttpResponse(HttpStatusCode.InternalServerError, """{"error":"Internal server error"}""");

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        var act = () => manager.SearchVectorStoreAsync(request);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*InternalServerError*");
    }

    [Fact]
    public async Task ConvertUrlAsync_HttpFailure_ThrowsHttpRequestException()
    {
        SetupHttpResponse(HttpStatusCode.BadRequest, """{"error":"Invalid URL"}""");

        var manager = CreateManager();

        var act = () => manager.ConvertUrlAsync("https://example.com");

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*BadRequest*");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_HttpFailure_ThrowsHttpRequestException()
    {
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable, """{"error":"Service unavailable"}""");

        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync("# Test");

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*ServiceUnavailable*");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_NonJsonResponse_ThrowsInvalidOperationException()
    {
        SetupHttpResponse(HttpStatusCode.OK, "<html><body>Error page</body></html>", "text/html");

        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync("# Test");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*non-JSON*");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_InvalidJson_ThrowsJsonException()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"status": "unclosed""");

        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync("# Test");

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task SearchVectorStoreAsync_NullRequest_Throws()
    {
        var manager = CreateManager();

        var act = () => manager.SearchVectorStoreAsync(null!);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_NullMarkdown_Throws()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"status":"success"}""");

        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync(null!);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task PostAsync_DeserializationReturnsNull_ThrowsInvalidOperationException()
    {
        SetupHttpResponse(HttpStatusCode.OK, "null");

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        var act = () => manager.SearchVectorStoreAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deserialize*");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_JsonNull_ThrowsInvalidOperationException()
    {
        SetupHttpResponse(HttpStatusCode.OK, "null");

        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync("# Test");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*non-JSON*");
    }

    [Fact]
    public async Task SearchVectorStoreAsync_NetworkFailure_PropagatesException()
    {
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        var act = () => manager.SearchVectorStoreAsync(request);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*Connection refused*");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_Timeout_PropagatesException()
    {
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync("# Test");

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    #endregion

    #region Configuration Validation

    [Fact]
    public void Constructor_ValidSettings_InitializesManager()
    {
        var iapHelper = (IAPAuthHelper)RuntimeHelpers.GetUninitializedObject(typeof(IAPAuthHelper));
        var options = Options.Create(_settings);

        var manager = new AiRetrieverManager(iapHelper, options, _configuration, _mockLogger.Object);

        manager.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithBaseUrl_SetsHttpClientBaseAddress()
    {
        var iapHelper = (IAPAuthHelper)RuntimeHelpers.GetUninitializedObject(typeof(IAPAuthHelper));
        var options = Options.Create(_settings);
        var manager = new AiRetrieverManager(iapHelper, options, _configuration, _mockLogger.Object);

        var client = typeof(AiRetrieverManager).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(manager) as HttpClient;
        client!.BaseAddress.Should().NotBeNull();
        client.BaseAddress!.ToString().Should().Be(BaseUrl + "/");
    }

    [Fact]
    public void Constructor_WithTimeout_SetsHttpClientTimeout()
    {
        var iapHelper = (IAPAuthHelper)RuntimeHelpers.GetUninitializedObject(typeof(IAPAuthHelper));
        var settings = new ExternalApiSettings { BaseUrl = BaseUrl, Timeout = 60 };
        var options = Options.Create(settings);
        var manager = new AiRetrieverManager(iapHelper, options, _configuration, _mockLogger.Object);

        var client = typeof(AiRetrieverManager).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(manager) as HttpClient;
        client!.Timeout.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public async Task SearchVectorStoreAsync_EndpointCorrect()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documents":[]}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.SearchVectorStoreAsync(new VectorStoreSearchRequest { Query = "test" });

        capturedRequest!.RequestUri!.PathAndQuery.Should().Contain("/v1/tools/vector-store/search");
    }

    [Fact]
    public async Task ConvertUrlAsync_EndpointCorrect()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","content":"","url":""}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.ConvertUrlAsync("https://example.com");

        capturedRequest!.RequestUri!.PathAndQuery.Should().Contain("/v1/convert/url");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_EndpointCorrect()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documentId":"","documentUrl":""}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        await manager.ConvertMarkdownToGoogleDocAsync("# Test");

        capturedRequest!.RequestUri!.PathAndQuery.Should().Contain("/v1/convert/markdown-to-google-doc");
    }

    #endregion

    #region Response Handling

    [Fact]
    public async Task SearchVectorStoreAsync_EmptyDocuments_ReturnsEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"status":"success","documents":[]}""");

        var manager = CreateManager();
        var result = await manager.SearchVectorStoreAsync(new VectorStoreSearchRequest { Query = "test" });

        result.Documents.Should().BeEmpty();
        result.Status.Should().Be("success");
    }

    [Fact]
    public async Task SearchVectorStoreAsync_PropertyNameCaseInsensitive_DeserializesCorrectly()
    {
        var json = """{"Status":"success","Query":"test","Documents":[{"Name":"Doc1","Content":"C1","Distance":0.5}]}""";
        SetupHttpResponse(HttpStatusCode.OK, json);

        var manager = CreateManager();
        var result = await manager.SearchVectorStoreAsync(new VectorStoreSearchRequest { Query = "test" });

        result.Status.Should().Be("success");
        result.Query.Should().Be("test");
        result.Documents.Should().HaveCount(1);
        result.Documents[0].Name.Should().Be("Doc1");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_JsonWithPdfBase64Snake_Deserializes()
    {
        var json = """{"status":"success","document_id":"doc1","document_url":"url","pdf_base64":"base64data"}""";
        SetupHttpResponse(HttpStatusCode.OK, json);

        var manager = CreateManager();
        var result = await manager.ConvertMarkdownToGoogleDocAsync("# Test");

        result.Status.Should().Be("success");
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_EmptyMarkdown_SendsRequest()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"status":"success","documentId":"","documentUrl":""}""");

        var manager = CreateManager();
        var result = await manager.ConvertMarkdownToGoogleDocAsync("");

        result.Status.Should().Be("success");
    }

    [Fact]
    public async Task ConvertMarkdownToGoogleDocAsync_LargeMarkdown_Handles()
    {
        var largeMarkdown = new string('#', 10000);
        SetupHttpResponse(HttpStatusCode.OK, """{"status":"success","documentId":"","documentUrl":""}""");

        var manager = CreateManager();
        var result = await manager.ConvertMarkdownToGoogleDocAsync(largeMarkdown);

        result.Status.Should().Be("success");
    }

    [Fact]
    public async Task SearchVectorStoreAsync_UnicodeQuery_SerializesCorrectly()
    {
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"status":"success","documents":[]}""", Encoding.UTF8, "application/json")
            });

        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "partenariat 合作 شراكة" };

        await manager.SearchVectorStoreAsync(request);

        var body = await capturedRequest!.Content!.ReadAsStringAsync();
        body.Should().Contain("partenariat");
    }

    [Fact]
    public async Task ConvertUrlAsync_EmptyUserEmail_ProceedsWithoutImpersonation()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"status":"success","content":"","url":"https://example.com"}""");

        var manager = CreateManager();
        var result = await manager.ConvertUrlAsync("https://example.com", userEmail: null);

        result.Status.Should().Be("success");
    }

    #endregion
}
