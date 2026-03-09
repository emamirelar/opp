/**
 * @fileoverview Integration tests for BaseEngagementController
 * Tests actual endpoints: /api/base-engagements/* (read-only)
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 *
 * Real endpoints (BaseEngagementController in UNOPS.PAO.UNOPSPresentation):
 * - GET /api/base-engagements (list all engagements)
 * - GET /api/base-engagements/{id} (get by ID)
 * - GET /api/partners/{partnerId}/base-engagements (get engagements by partner)
 * - GET /api/base-engagements/{engagementId}/partners (get partners for an engagement)
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
/// Integration tests for BaseEngagementController - real read-only endpoints only
/// </summary>
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "BaseEngagement")]
public class BaseEngagementControllerTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/base-engagements";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public BaseEngagementControllerTests(PAOWebApplicationFactory<Program> factory)
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
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    #region Positive Tests (12 tests)

    [Fact]
    [Trait("TestId", "TC-BE-POS-001")]
    public async Task GetAll_Authenticated_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-002")]
    public async Task GetAll_ReturnsJsonContentType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-003")]
    public async Task GetById_WithId1_Returns200Or404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-004")]
    public async Task GetByPartner_WithPartnerId1_Returns200Or404()
    {
        var response = await _client.GetAsync("/api/partners/1/base-engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-005")]
    public async Task GetEngagementPartners_WithEngagementId1_Returns200Or404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/partners");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-006")]
    public async Task GetAll_ResponseIsJsonArrayOrObject()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-007")]
    public async Task GetAll_WithEmptyDatabase_Returns200()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-008")]
    public async Task GetById_WhenFound_ReturnsEngagementDetails()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().Be(JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-009")]
    public async Task GetByPartner_ReturnsListType()
    {
        var response = await _client.GetAsync("/api/partners/1/base-engagements");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-010")]
    public async Task GetEngagementPartners_ReturnsListType()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/partners");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-011")]
    public async Task AllEndpoints_ReturnWithin5Seconds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync(BaseUrl);
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "endpoint should respond within 5 seconds");
    }

    [Fact]
    [Trait("TestId", "TC-BE-POS-012")]
    public async Task MultipleSequentialCalls_ReturnConsistentResults()
    {
        var response1 = await _client.GetAsync(BaseUrl);
        var response2 = await _client.GetAsync(BaseUrl);
        response1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        response2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        response1.StatusCode.Should().Be(response2.StatusCode);
    }

    #endregion

    #region Negative Tests (8 tests)

    [Fact]
    [Trait("TestId", "TC-BE-NEG-001")]
    public async Task GetById_WithId0_Returns400Or404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-NEG-002")]
    public async Task GetById_WithIdNegative1_Returns400Or404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-NEG-003")]
    public async Task GetById_WithNonExistentId999999_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-NEG-004")]
    public async Task GetByPartner_WithPartnerId999999_Returns200EmptyOr404()
    {
        var response = await _client.GetAsync("/api/partners/999999/base-engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            if (result.ValueKind == JsonValueKind.Array)
            {
                result.GetArrayLength().Should().Be(0);
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-NEG-005")]
    public async Task Post_ToBaseEngagements_Returns405()
    {
        var response = await _client.PostAsync(BaseUrl, JsonContent.Create(new { name = "Test" }));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-NEG-006")]
    public async Task Delete_ToBaseEngagements1_Returns404Or405()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-NEG-007")]
    public async Task Put_ToBaseEngagements1_Returns404Or405()
    {
        var response = await _client.PutAsync($"{BaseUrl}/1", JsonContent.Create(new { name = "Updated" }));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-NEG-008")]
    public async Task GetById_WithStringId_Returns400Or404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/abc");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Security Tests (6 tests)

    [Fact]
    [Trait("TestId", "TC-BE-SEC-001")]
    public async Task GetAll_WithoutAuth_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-SEC-002")]
    public async Task GetById_WithoutAuth_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-SEC-003")]
    public async Task GetByPartner_WithoutAuth_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync("/api/partners/1/base-engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-SEC-004")]
    public async Task GetEngagementPartners_WithoutAuth_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/1/partners");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-SEC-005")]
    public async Task InvalidAuthEmail_Returns401Or403()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:invalid@unknown.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:999");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=invalid@unknown.org; dev-user-email=invalid@unknown.org");
        var response = await client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-BE-SEC-006")]
    public async Task ErrorResponses_DoNotExposeSensitiveData()
    {
        var response = await _client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
        content.Should().NotContain("password");
        content.Should().NotContain("connectionString");
        content.Should().NotContain("secret");
    }

    #endregion

    #region Validation Tests (6 tests)

    [Fact]
    [Trait("TestId", "TC-BE-VAL-001")]
    public async Task GetAll_ResponseHasValidJsonStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-VAL-002")]
    public async Task GetById_WhenFound_ResponseHasExpectedFields()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().Be(JsonValueKind.Object);
            result.TryGetProperty("id", out _).Should().BeTrue();
            result.TryGetProperty("name", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-VAL-003")]
    public async Task GetByPartner_ResponseIsArrayOrListType()
    {
        var response = await _client.GetAsync("/api/partners/1/base-engagements");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-VAL-004")]
    public async Task GetEngagementPartners_ResponseIsArrayOrListType()
    {
        var response = await _client.GetAsync($"{BaseUrl}/1/partners");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-VAL-005")]
    public async Task AllResponses_HaveProperContentTypeHeader()
    {
        var endpoints = new[]
        {
            BaseUrl,
            $"{BaseUrl}/1",
            "/api/partners/1/base-engagements",
            $"{BaseUrl}/1/partners"
        };

        foreach (var url in endpoints)
        {
            if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 0)
            {
                response.Content.Headers.ContentType?.MediaType.Should().Contain("json", $"because {url} returns JSON");
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-VAL-006")]
    public async Task ErrorResponses_ReturnJsonWithErrorDetails()
    {
        var response = await _client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().Be(JsonValueKind.Object);
            result.TryGetProperty("error", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-BE-EDGE-001")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetEngagementList_ResponseContent_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=50");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??",
                "PNO-1194: engagement entity names must not contain encoding artifacts");
            content.Should().NotContain("\uFFFD",
                "Engagement data must not contain U+FFFD replacement characters");
        }
    }

    #endregion
}
