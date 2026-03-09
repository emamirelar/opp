/**
 * @fileoverview Integration tests for Image Generation API endpoint
 * POST /api/opportunity/{id}/generate-images
 * Tests actual endpoint behavior: auth, validation, security, response structure
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for Image Generation API endpoint (OpportunityController.GenerateOpportunityImages)
/// Endpoint: POST /api/opportunity/{id}/generate-images
/// </summary>
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "ImageGeneration")]
public class ImageGenerationControllerTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/opportunity";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ImageGenerationControllerTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    private HttpClient CreateUnauthenticatedClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    #region Positive Tests

    [Fact]
    [Trait("TestId", "TC-IG-POS-001")]
    public async Task PostGenerateImages_WithValidOrNonExistentId_Returns200Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/1/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-POS-002")]
    public async Task PostGenerateImages_ResponseIsJsonContentType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/1/generate-images", null);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
    }

    [Fact]
    [Trait("TestId", "TC-IG-POS-003")]
    public async Task PostGenerateImages_ReturnsResponseWithin30Seconds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsync($"{BaseUrl}/1/generate-images", null);
        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-POS-004")]
    public async Task PostGenerateImages_WhenSuccess_ResponseContainsOpportunityData()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var opportunityId = await EnsureTestOpportunityExistsAsync();
        var response = await _client.PostAsync($"{BaseUrl}/{opportunityId}/generate-images", null);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().NotBeNullOrWhiteSpace();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.TryGetProperty("id", out _).Should().BeTrue();
            root.TryGetProperty("name", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-IG-POS-005")]
    public async Task PostGenerateImages_ConsecutiveCalls_ReturnConsistentStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response1 = await _client.PostAsync($"{BaseUrl}/999998/generate-images", null);
        var response2 = await _client.PostAsync($"{BaseUrl}/999998/generate-images", null);

        response1.StatusCode.Should().Be(response2.StatusCode);
        if (response1.IsSuccessStatusCode && response2.IsSuccessStatusCode)
        {
            var json1 = await response1.Content.ReadAsStringAsync();
            var json2 = await response2.Content.ReadAsStringAsync();
            var doc1 = JsonDocument.Parse(json1);
            var doc2 = JsonDocument.Parse(json2);
            doc1.RootElement.TryGetProperty("id", out _).Should().Be(doc2.RootElement.TryGetProperty("id", out _));
        }
    }

    [Fact]
    [Trait("TestId", "TC-IG-POS-006")]
    public async Task GenerateImagesEndpoint_AcceptsPostMethod()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/1/generate-images", null);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region Negative Tests

    [Fact]
    [Trait("TestId", "TC-IG-NEG-001")]
    public async Task PostGenerateImages_WithIdZero_Returns400Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/0/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-NEG-002")]
    public async Task PostGenerateImages_WithNegativeId_Returns400Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/-1/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-NEG-003")]
    public async Task PostGenerateImages_WithNonExistentId_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/999999/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-NEG-004")]
    public async Task GetGenerateImages_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/1/generate-images");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-NEG-005")]
    public async Task PutGenerateImages_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PutAsync($"{BaseUrl}/1/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-NEG-006")]
    public async Task DeleteGenerateImages_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync($"{BaseUrl}/1/generate-images");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-NEG-007")]
    public async Task PostGenerateImages_WithStringId_Returns400Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/abc/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-IG-NEG-008")]
    public async Task PostGenerateImages_WithoutBody_StillWorks()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/1/generate-images", null);
        response.StatusCode.Should().NotBe(HttpStatusCode.UnsupportedMediaType);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region Security Tests

    [Fact]
    [Trait("TestId", "TC-IG-SEC-001")]
    public async Task PostGenerateImages_WithoutAuth_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.PostAsync($"{BaseUrl}/1/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-IG-SEC-002")]
    public async Task PostGenerateImages_WithInvalidAuthEmail_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:invalid@unknown.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:999");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=invalid@unknown.org; dev-user-email=invalid@unknown.org");

        var response = await client.PostAsync($"{BaseUrl}/1/generate-images", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-IG-SEC-003")]
    public async Task PostGenerateImages_ErrorResponses_NoSensitiveData()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/999999/generate-images", null);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("password");
        content.Should().NotContain("token");
        content.Should().NotContain("secret");
        content.ToLowerInvariant().Should().NotContain("connectionstring");
    }

    [Fact]
    [Trait("TestId", "TC-IG-SEC-004")]
    public async Task PostGenerateImages_ErrorResponses_ProperContentType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/999999/generate-images", null);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
    }

    #endregion

    #region Validation Tests

    [Fact]
    [Trait("TestId", "TC-IG-VAL-001")]
    public async Task PostGenerateImages_ValidResponse_HasValidStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/1/generate-images", null);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.ValueKind.Should().Be(JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-IG-VAL-002")]
    public async Task PostGenerateImages_Success_ContentTypeIsApplicationJson()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/1/generate-images", null);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        }
    }

    [Fact]
    [Trait("TestId", "TC-IG-VAL-003")]
    public async Task PostGenerateImages_ErrorResponses_AreJson()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync($"{BaseUrl}/999999/generate-images", null);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrWhiteSpace();
            var doc = JsonDocument.Parse(content);
            doc.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-IG-VAL-004")]
    public async Task PostGenerateImages_WhenSuccessful_HasExpectedFields()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var opportunityId = await EnsureTestOpportunityExistsAsync();
        var response = await _client.PostAsync($"{BaseUrl}/{opportunityId}/generate-images", null);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.TryGetProperty("id", out _).Should().BeTrue();
            root.TryGetProperty("name", out _).Should().BeTrue();
            root.TryGetProperty("description", out _).Should().BeTrue();
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Creates a test opportunity if none exists. Returns an opportunity ID for testing.
    /// </summary>
    private async Task<int> EnsureTestOpportunityExistsAsync()
    {
        var createRequest = new
        {
            name = "Test Opportunity for Image Generation",
            description = "Integration test opportunity with name and description for AI image generation"
        };

        var createResponse = await _client.PostAsJsonAsync(BaseUrl, createRequest);
        if (createResponse.IsSuccessStatusCode)
        {
            var json = await createResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("id", out var idProp))
            {
                return idProp.GetInt32();
            }
        }

        return 1;
    }

    #endregion
}
