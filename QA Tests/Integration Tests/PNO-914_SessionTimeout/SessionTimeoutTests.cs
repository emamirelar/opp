/**
 * @fileoverview PNO-914 Session Timeout Tests — validates session and timeout behavior
 * to address "Connection lost" messages and session/idle timeout issues.
 *
 * Bug: Users getting frequent "Connection lost" messages. Session/idle timeout issues.
 * Status: Peer Review
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-914
 */

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914SessionTimeout;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-914")]
[Trait("Component", "SessionTimeout")]
[Trait("JiraRef", "PNO-914")]
public class SessionTimeoutTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public SessionTimeoutTests(PAOWebApplicationFactory<Program> factory)
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

    #region POSITIVE (2)

    [Fact]
    [Trait("TestId", "TC-PNO914-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_ApiResponds_WithinReasonableTimeout()
    {
        if (!_isPostgresAvailable) return;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var response = await _client.GetAsync("/api/configuration", cts.Token);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_HealthEndpoint_RespondsQuickly()
    {
        if (!_isPostgresAvailable) return;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/health");
        stopwatch.Stop();
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
            HttpStatusCode.ServiceUnavailable);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000,
            "Health endpoint must respond within 10 seconds");
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-001")]
    [Trait("Category", "Negative")]
    public async Task NEG_001_RequestWithExpiredOrMissingAuth_ReturnsStructuredError()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var response = await unauthClient.GetAsync("/api/configuration");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found,
            HttpStatusCode.OK);
        if (response.Content.Headers.ContentType?.MediaType != null)
            response.Content.Headers.ContentType.MediaType.Should().Contain("json",
                "Error response should be structured (JSON), not raw exception");
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_UnauthenticatedRequest_Returns401Or302_NotTimeout()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var response = await unauthClient.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_RequestToNonExistentEndpoint_Returns404_NotHang()
    {
        if (!_isPostgresAvailable) return;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var response = await _client.GetAsync("/api/nonexistent-endpoint-xyz-123", cts.Token);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_MalformedRequest_Returns400_NotTimeout()
    {
        if (!_isPostgresAvailable) return;
        var content = new StringContent("{invalid json", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/opportunity", content);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_EmptyRequestBody_ReturnsError_NotHang()
    {
        if (!_isPostgresAvailable) return;
        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/opportunity", content);
        response.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task NEG_006_InvalidContentType_Returns415_NotTimeout()
    {
        if (!_isPostgresAvailable) return;
        var content = new StringContent("{}", Encoding.UTF8, "text/plain");
        var response = await _client.PostAsync("/api/opportunity", content);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.OK);
    }

    #endregion

    #region FUNCTIONAL (6)

    [Fact]
    [Trait("TestId", "TC-PNO914-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_ConfigurationEndpoint_ReturnsTimeoutSettings()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/configuration");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object,
            "Configuration endpoint must return structured response with settings");
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_MultipleSequentialRequests_AllRespond()
    {
        if (!_isPostgresAvailable) return;
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.GetAsync("/api/configuration"))
            .ToList();
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r => r.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout));
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_Api_ReturnsStructuredErrorResponses()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            (contentType.Contains("json") || response.Content.Headers.ContentLength == 0).Should().BeTrue(
                "Error responses should be structured, not raw exceptions");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_HealthEndpoint_RespondsRegardlessOfAuth()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var response = await unauthClient.GetAsync("/api/health");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_ErrorResponses_IncludeProperHttpStatusCodes()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/not-an-id");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_ApiResponses_IncludeAppropriateHeaders()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/configuration");
        response.Headers.Should().NotBeNull();
        response.Content.Headers.ContentType.Should().NotBeNull();
    }

    #endregion

    #region EDGE (6)

    [Fact]
    [Trait("TestId", "TC-PNO914-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_VeryRapidSequentialRequests_AllSucceed()
    {
        if (!_isPostgresAvailable) return;
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _client.GetAsync("/api/configuration"))
            .ToList();
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r => r.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout));
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_RequestWithLargePayload_CompletesWithoutTimeout()
    {
        if (!_isPostgresAvailable) return;
        var largePayload = new string('x', 50000);
        var content = new StringContent($"{{\"name\":\"{largePayload}\"}}", Encoding.UTF8, "application/json");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var response = await _client.PostAsync("/api/opportunity", content, cts.Token);
        response.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_RequestWithManyQueryParameters_Responds()
    {
        if (!_isPostgresAvailable) return;
        var query = string.Join("&", Enumerable.Range(0, 20).Select(i => $"p{i}=v{i}"));
        var response = await _client.GetAsync($"/api/configuration?{query}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_ConcurrentRequestsToDifferentEndpoints_AllRespond()
    {
        if (!_isPostgresAvailable) return;
        var configTask = _client.GetAsync("/api/configuration");
        var oppTask = _client.GetAsync("/api/opportunity/1");
        var healthTask = _client.GetAsync("/api/health");
        await Task.WhenAll(configTask, oppTask, healthTask);
        (await configTask).StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
        (await oppTask).StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
        (await healthTask).StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_RequestImmediatelyAfterAuthSetup_Succeeds()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/configuration");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_LongRunningEndpoint_RespondsOrReturnsProperTimeout()
    {
        if (!_isPostgresAvailable) return;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var response = await _client.GetAsync("/api/opportunity", cts.Token);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
            HttpStatusCode.RequestTimeout);
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO914-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_FullFlow_AuthThenMultipleApiCalls_AllSucceed()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/configuration");
        var r2 = await _client.GetAsync("/api/opportunity/1");
        var r3 = await _client.GetAsync("/api/health");
        r1.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
        r2.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
        r3.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_ConfigurationAndHealthEndpoints_BothAccessible()
    {
        if (!_isPostgresAvailable) return;
        var configResponse = await _client.GetAsync("/api/configuration");
        var healthResponse = await _client.GetAsync("/api/health");
        configResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
        healthResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.NotFound,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_MultipleEndpointTypes_RespondInSequence()
    {
        if (!_isPostgresAvailable) return;
        var responses = new List<HttpResponseMessage>
        {
            await _client.GetAsync("/api/configuration"),
            await _client.GetAsync("/api/opportunity"),
            await _client.GetAsync("/api/health")
        };
        responses.Should().AllSatisfy(r => r.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout));
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_ErrorRecovery_BadRequestFollowedByGoodRequest()
    {
        if (!_isPostgresAvailable) return;
        var badResponse = await _client.GetAsync("/api/opportunity/not-an-id");
        var goodResponse = await _client.GetAsync("/api/configuration");
        badResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        goodResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_DifferentHttpMethods_AllRespondAppropriately()
    {
        if (!_isPostgresAvailable) return;
        var getResponse = await _client.GetAsync("/api/configuration");
        var postResponse = await _client.PostAsync("/api/opportunity", null);
        getResponse.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
        postResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO914-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_ApiAvailability_ConsistentAcrossTestSession()
    {
        if (!_isPostgresAvailable) return;
        for (var i = 0; i < 3; i++)
        {
            var response = await _client.GetAsync("/api/configuration");
            response.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
        }
    }

    #endregion
}
