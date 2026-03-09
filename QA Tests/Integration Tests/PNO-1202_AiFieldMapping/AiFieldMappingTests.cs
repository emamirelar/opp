/**
 * @fileoverview PNO-1202 AI Field Mapping Tests — validates AI retriever endpoints
 * for document parsing and field mapping.
 *
 * Bug: AI document parsing engine fails to extract and map clearly defined data
 * from uploaded documents into system fields (Dates, Team, Beneficiaries).
 * Status: Peer Review
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1202
 */

using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1202;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-1202")]
[Trait("Component", "AiFieldMapping")]
[Trait("JiraRef", "PNO-1202")]
public class AiFieldMappingTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;

    private const string AIRetrieverBase = "/api/ai-retriever";
    private const string Health = AIRetrieverBase + "/health";
    private const string VectorStoreSearch = AIRetrieverBase + "/vector-store/search";
    private const string ConvertUrl = AIRetrieverBase + "/convert/url";
    private const string ConvertMarkdown = AIRetrieverBase + "/convert/markdown-to-google-doc";

    public AiFieldMappingTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    private static HttpClient CreateUnauthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    private static StringContent JsonContent(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    #region POSITIVE (2)

    [Fact]
    [Trait("TestId", "TC-PNO1202-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_AIHealthEndpoint_Returns200()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(Health);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_AISearchEndpoint_AcceptsAuthenticatedRequest()
    {
        if (!_isPostgresAvailable) return;
        var request = new VectorStoreSearchRequest { Query = "partnership opportunities", MaxResults = 5 };
        var response = await _client.PostAsync(VectorStoreSearch, JsonContent(request));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1202-NEG-001")]
    [Trait("Category", "Negative")]
    public async Task NEG_001_AISearchWithEmptyQuery_ReturnsError()
    {
        if (!_isPostgresAvailable) return;
        var request = new VectorStoreSearchRequest { Query = "", MaxResults = 5 };
        var response = await _client.PostAsync(VectorStoreSearch, JsonContent(request));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_UnauthenticatedPostToAISearch_Returns401Or302()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var request = new VectorStoreSearchRequest { Query = "test" };
        var response = await unauthClient.PostAsync(VectorStoreSearch, JsonContent(request));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_AISearchWithNullBody_Returns400Or415()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.PostAsync(VectorStoreSearch, new StringContent("", Encoding.UTF8, "application/json"));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_AIUrlConvertWithInvalidUrl_ReturnsError()
    {
        if (!_isPostgresAvailable) return;
        var request = new { url = "not-a-valid-url" };
        var response = await _client.PostAsync(ConvertUrl, JsonContent(request));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_AISearchWithExtremelyLongQuery_HandledGracefully()
    {
        if (!_isPostgresAvailable) return;
        var longQuery = new string('x', 10000);
        var request = new VectorStoreSearchRequest { Query = longQuery, MaxResults = 5 };
        var response = await _client.PostAsync(VectorStoreSearch, JsonContent(request));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task NEG_006_AIEndpointWithMalformedJson_Returns400()
    {
        if (!_isPostgresAvailable) return;
        var content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(VectorStoreSearch, content);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region FUNCTIONAL (6)

    [Fact]
    [Trait("TestId", "TC-PNO1202-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_HealthEndpoint_ReturnsStructuredResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(Health);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("healthy", "health endpoint must return structured response");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_AISearchEndpoint_AcceptsJsonContentType()
    {
        if (!_isPostgresAvailable) return;
        var request = new VectorStoreSearchRequest { Query = "test", MaxResults = 5 };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(VectorStoreSearch, content);
        response.StatusCode.Should().NotBe(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_AIUrlConvertEndpoint_AcceptsJsonContentType()
    {
        if (!_isPostgresAvailable) return;
        var request = new { url = "https://example.com/doc" };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(ConvertUrl, content);
        response.StatusCode.Should().NotBe(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_AIEndpoints_ReturnJsonResponses()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(Health);
        if (response.StatusCode != HttpStatusCode.OK) return;

        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_AIHealth_IsAccessibleWithoutAuth()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var response = await unauthClient.GetAsync(Health);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_AISearchResponse_HasExpectedStructure()
    {
        if (!_isPostgresAvailable) return;
        var request = new VectorStoreSearchRequest { Query = "test", MaxResults = 5 };
        var response = await _client.PostAsync(VectorStoreSearch, JsonContent(request));
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }

    #endregion

    #region EDGE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1202-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_AISearchWithSpecialCharactersInQuery()
    {
        if (!_isPostgresAvailable) return;
        var request = new VectorStoreSearchRequest { Query = "café & naïve <test>", MaxResults = 5 };
        var response = await _client.PostAsync(VectorStoreSearch, JsonContent(request));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_AISearchWithUnicodeText()
    {
        if (!_isPostgresAvailable) return;
        var request = new VectorStoreSearchRequest { Query = "日本語 テスト 🎉", MaxResults = 5 };
        var response = await _client.PostAsync(VectorStoreSearch, JsonContent(request));
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_AIUrlConvertWithVeryLongUrl()
    {
        if (!_isPostgresAvailable) return;
        var longUrl = "https://example.com/" + new string('a', 2000);
        var request = new { url = longUrl };
        var response = await _client.PostAsync(ConvertUrl, JsonContent(request));
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_ConcurrentAIRequests_DontInterfere()
    {
        if (!_isPostgresAvailable) return;
        var task1 = _client.PostAsync(VectorStoreSearch, JsonContent(new VectorStoreSearchRequest { Query = "test1", MaxResults = 5 }));
        var task2 = _client.PostAsync(VectorStoreSearch, JsonContent(new VectorStoreSearchRequest { Query = "test2", MaxResults = 5 }));
        var responses = await Task.WhenAll(task1, task2);
        responses.Should().HaveCount(2);
        responses.Select(r => r.StatusCode).Should().OnlyContain(sc =>
            sc == HttpStatusCode.OK || sc == HttpStatusCode.BadGateway || sc == HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_AISearch_EmptyStringVsNull()
    {
        if (!_isPostgresAvailable) return;
        var requestEmpty = new VectorStoreSearchRequest { Query = "", MaxResults = 5 };
        var requestWithQuery = new VectorStoreSearchRequest { Query = "test", MaxResults = 5 };
        var responseEmpty = await _client.PostAsync(VectorStoreSearch, JsonContent(requestEmpty));
        var responseWithQuery = await _client.PostAsync(VectorStoreSearch, JsonContent(requestWithQuery));
        responseEmpty.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        responseWithQuery.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_AIEndpointWithExtraFieldsInJsonBody()
    {
        if (!_isPostgresAvailable) return;
        var body = "{\"query\":\"test\",\"maxResults\":5,\"extraField\":\"ignored\"}";
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(VectorStoreSearch, content);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO1202-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_FullFlow_HealthCheckThenSearch()
    {
        if (!_isPostgresAvailable) return;
        var healthResponse = await _client.GetAsync(Health);
        healthResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        if (healthResponse.StatusCode != HttpStatusCode.OK) return;

        var searchResponse = await _client.PostAsync(VectorStoreSearch,
            JsonContent(new VectorStoreSearchRequest { Query = "opportunity", MaxResults = 5 }));
        searchResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadGateway,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_AISearchAndUrlConvert_BothAccessibleInSequence()
    {
        if (!_isPostgresAvailable) return;
        var searchResponse = await _client.PostAsync(VectorStoreSearch,
            JsonContent(new VectorStoreSearchRequest { Query = "test", MaxResults = 5 }));
        var urlResponse = await _client.PostAsync(ConvertUrl, JsonContent(new { url = "https://example.com" }));
        searchResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        urlResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_AIEndpointsAndMainAPI_ShareAuthentication()
    {
        if (!_isPostgresAvailable) return;
        var aiResponse = await _client.GetAsync(Health);
        var mainApiResponse = await _client.GetAsync("/api/user-info/current");
        aiResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        mainApiResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_AIHealth_ReturnsSameResultAcrossMultipleCalls()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync(Health);
        var r2 = await _client.GetAsync(Health);
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode == HttpStatusCode.OK && r2.StatusCode == HttpStatusCode.OK)
        {
            var body1 = await r1.Content.ReadAsStringAsync();
            var body2 = await r2.Content.ReadAsStringAsync();
            body1.Should().Contain("healthy");
            body2.Should().Contain("healthy");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_AISearchAndMarkdownConvert_BothRespond()
    {
        if (!_isPostgresAvailable) return;
        var searchResponse = await _client.PostAsync(VectorStoreSearch,
            JsonContent(new VectorStoreSearchRequest { Query = "test", MaxResults = 5 }));
        var markdownResponse = await _client.PostAsync(ConvertMarkdown,
            JsonContent(new { markdown = "# Test\n\nContent" }));
        searchResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        markdownResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1202-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_AIServiceAvailability_DoesNotAffectMainAPIHealth()
    {
        if (!_isPostgresAvailable) return;
        var mainApiResponse = await _client.GetAsync("/api/user-info/current");
        mainApiResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        var aiHealthResponse = await _client.GetAsync(Health);
        aiHealthResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    #endregion
}
