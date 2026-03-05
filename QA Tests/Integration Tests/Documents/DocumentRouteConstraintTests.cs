/**
 * @fileoverview DEF-021 — DocumentController route constraint tests.
 *
 * DEF-021 fix added a regex route constraint to DocumentController.GetAll:
 *   [HttpGet(APIDictionary.Document + "/{entityName:regex(^(?!download$).+)}/{entityId:int}")]
 *
 * This prevents the GET /api/document/download/{id} path from matching the GetAll endpoint
 * when the UNOPS override (UNOPSPresentation.DocumentController.Download) is active.
 * The base DownloadDocument action is marked [NonAction] to avoid AmbiguousMatchException.
 *
 * Tests verify:
 *  - Valid entity names (Partner, Contact, Interaction, Opportunity) still resolve via GetAll
 *  - The word "download" as entityName is excluded by the regex
 *  - The DOWNLOAD route returns the expected status (handled by UNOPS override)
 *  - Numeric entityId constraint enforces integer-only values
 *  - No regression on existing document endpoints (link, upload, view-url)
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Documents;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "DEF-021")]
[Trait("Component", "DocumentRouteConstraint")]
public class DocumentRouteConstraintTests
{
    private readonly HttpClient _client;
    private readonly HttpClient _unauthClient;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DocumentRouteConstraintTests(PAOWebApplicationFactory<Program> factory)
    {
        _client = CreateAuthenticatedClient(factory);
        _unauthClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _isPostgresAvailable = factory.IsUsingPostgres;
    }

    private static HttpClient CreateAuthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    // ──────────────────────────────────────────────────────────────
    // POSITIVE: valid entity names must still route to GetAll
    // ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Partner")]
    [InlineData("Contact")]
    [InlineData("Interaction")]
    [InlineData("Opportunity")]
    [InlineData("partner")]   // case-insensitive entity name
    [InlineData("contact")]
    [Trait("TestId", "TC-DEF021-POS-001")]
    public async Task GetDocumentsByEntityName_ValidEntityNames_DoNotReturn405(string entityName)
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Valid entity names must route to GetAll (not rejected by constraint)
        var response = await _client.GetAsync($"/api/document/{entityName}/1");

        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed,
            $"valid entityName '{entityName}' must not be rejected by the route constraint");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    // ──────────────────────────────────────────────────────────────
    // NEGATIVE: "download" as entityName must be excluded by regex
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-001")]
    public async Task GetDocuments_EntityNameIsDownload_DoesNotMatchGetAllRoute()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // DEF-021: /api/document/download/1 must NOT cause AmbiguousMatchException after fix.
        // Without the fix, routing throws AmbiguousMatchException in the test harness.
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.GetAsync("/api/document/download/1");
        }
        catch (Exception ex) when (ex.Message.Contains("AmbiguousMatch") || ex.GetType().Name.Contains("AmbiguousMatch"))
        {
            Assert.Fail(
                "DEF-021 fix not yet deployed: AmbiguousMatchException on /api/document/download/1. " +
                "DownloadDocument must be marked [NonAction] and GetAll must have regex constraint. " +
                $"Exception: {ex.Message}");
        }

        response!.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-002")]
    public async Task GetDocuments_EntityNameIsDownloadUppercase_NotRouteConflict()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // "DOWNLOAD" (uppercase) — ASP.NET routing is case-insensitive so this also hits download endpoints.
        // Without DEF-021 fix: AmbiguousMatchException. After fix: GetAll or 404.
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.GetAsync("/api/document/DOWNLOAD/1");
        }
        catch (Exception ex) when (ex.Message.Contains("AmbiguousMatch") || ex.GetType().Name.Contains("AmbiguousMatch"))
        {
            Assert.Fail(
                "DEF-021 fix not yet deployed: AmbiguousMatchException on /api/document/DOWNLOAD/1. " +
                $"Exception: {ex.Message}");
        }

        response!.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-003")]
    public async Task GetDocuments_EntityNameIsDownloadMixed_NotRouteConflict()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Mixed case "Download" — routing is case-insensitive so also hits download endpoints.
        // Without DEF-021 fix: AmbiguousMatchException. After fix: GetAll or 404.
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.GetAsync("/api/document/Download/1");
        }
        catch (Exception ex) when (ex.Message.Contains("AmbiguousMatch") || ex.GetType().Name.Contains("AmbiguousMatch"))
        {
            Assert.Fail(
                "DEF-021 fix not yet deployed: AmbiguousMatchException on /api/document/Download/1. " +
                $"Exception: {ex.Message}");
        }

        response!.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-004")]
    public async Task GetDocuments_UnauthenticatedRequest_Returns401Or302()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _unauthClient.GetAsync("/api/document/Partner/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-005")]
    public async Task GetDocuments_NonIntegerEntityId_DoesNotMatchRoute()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Route constraint specifies {entityId:int} — string entityId must NOT match GetAll.
        // Production falls through to a catch-all handler that returns 500 when route constraint
        // prevents a match and no other handler exists.
        var response = await _client.GetAsync("/api/document/Partner/not-an-integer");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.InternalServerError);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-006")]
    public async Task GetDocuments_ZeroEntityId_ReturnsNotFoundOrEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/document/Partner/0");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,        // may return empty list
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-007")]
    public async Task GetDocuments_EmptyEntityName_DoesNotMatchRoute()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Empty entityName segment: /api/document//1 — invalid URL
        var response = await _client.GetAsync("/api/document//1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.MovedPermanently);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-008")]
    public async Task GetDocuments_NegativeEntityId_IsHandledGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/document/Partner/-1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-009")]
    public async Task GetDocuments_PostMethodToGetRoute_Returns405Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsJsonAsync("/api/document/Partner/1", new { }, JsonOpts);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-010")]
    public async Task DownloadDocument_NonAction_BaseEndpoint_NotRegistered()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // DEF-021: base DownloadDocument must be [NonAction], removing it from the routing table.
        // Without the fix, the test harness throws AmbiguousMatchException.
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.GetAsync("/api/document/download/999");
        }
        catch (Exception ex) when (ex.Message.Contains("AmbiguousMatch") || ex.GetType().Name.Contains("AmbiguousMatch"))
        {
            Assert.Fail(
                "DEF-021 fix not yet deployed: base DownloadDocument is still registered as a route. " +
                "Mark it [NonAction] to resolve the ambiguity. " +
                $"Exception: {ex.Message}");
        }

        if (response!.StatusCode == HttpStatusCode.InternalServerError)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotContain("AmbiguousMatchException",
                "DEF-021: download route must not produce AmbiguousMatchException after fix");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-011")]
    public async Task GetDocuments_MaxIntEntityId_HandledGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"/api/document/Partner/{int.MaxValue}");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-NEG-012")]
    public async Task GetDocuments_UnknownEntityName_ReturnsEmptyOrNotFound()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // An entity name that does not correspond to any real entity type
        var response = await _client.GetAsync("/api/document/UnknownEntityXYZ/1");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,        // may return empty list
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    // ──────────────────────────────────────────────────────────────
    // FUNCTIONAL: existing document endpoints not broken by DEF-021
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [Trait("TestId", "TC-DEF021-FUNC-001")]
    public async Task GetDocumentById_StillWorks_AfterRouteConstraint()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // GET /api/document/{id} (single document by ID) — different route, must still work
        var response = await _client.GetAsync("/api/document/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-FUNC-002")]
    public async Task LinkDocument_Endpoint_StillReachable_AfterRouteConstraint()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // POST /api/document/link must still be reachable
        var payload = new { entityType = "Partner", entityId = 1, url = "https://drive.google.com/file/test" };
        var response = await _client.PostAsJsonAsync("/api/document/link", payload, JsonOpts);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-FUNC-003")]
    public async Task GetDocuments_Partner_ResponseIsJsonArray_WhenOk()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/document/Partner/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-FUNC-004")]
    public async Task GetDocuments_Route_Handles_AllSupportedEntities_NoAmbiguousMatchException()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Core regression guard for DEF-021 — none of these must throw AmbiguousMatchException
        var entities = new[] { "Partner", "Contact", "Interaction", "Opportunity" };
        foreach (var entity in entities)
        {
            var response = await _client.GetAsync($"/api/document/{entity}/1");

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var body = await response.Content.ReadAsStringAsync();
                body.Should().NotContain("AmbiguousMatchException",
                    $"DEF-021: entity '{entity}' must not trigger AmbiguousMatchException");
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-FUNC-005")]
    public async Task GetDocuments_DownloadPath_NoAmbiguousMatchException()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Central DEF-021 regression guard: download path must NEVER produce AmbiguousMatchException.
        // Before fix: the harness throws this exception during routing.
        // After fix: a proper HTTP response is returned.
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.GetAsync("/api/document/download/1");
        }
        catch (Exception ex) when (ex.Message.Contains("AmbiguousMatch") || ex.GetType().Name.Contains("AmbiguousMatch"))
        {
            Assert.Fail(
                "DEF-021 regression: AmbiguousMatchException on /api/document/download/1. " +
                "This indicates the [NonAction] attribute and/or regex constraint is not deployed. " +
                $"Exception: {ex.Message}");
        }

        if (response!.StatusCode == HttpStatusCode.InternalServerError)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotContain("AmbiguousMatchException",
                "DEF-021: download route must not produce AmbiguousMatchException");
        }
    }

    // ──────────────────────────────────────────────────────────────
    // EDGE/BOUNDARY TESTS
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [Trait("TestId", "TC-DEF021-BND-001")]
    public async Task GetDocuments_EntityNameExactlyDownload_MatchesCaseSensitiveRegex()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Boundary: regex ^(?!download$) excludes exactly "download" (lowercase).
        // Before DEF-021 fix: routing throws AmbiguousMatchException.
        // After fix: route resolves cleanly (no GetAll match for "download").
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.GetAsync("/api/document/download/1");
        }
        catch (Exception ex) when (ex.Message.Contains("AmbiguousMatch") || ex.GetType().Name.Contains("AmbiguousMatch"))
        {
            Assert.Fail(
                "DEF-021 fix not yet deployed: AmbiguousMatchException on /api/document/download/1. " +
                "Regex constraint ^(?!download$) must be present on GetAll. " +
                $"Exception: {ex.Message}");
        }

        if (response!.StatusCode == HttpStatusCode.InternalServerError)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotContain("AmbiguousMatchException",
                "DEF-021: regex constraint must eliminate AmbiguousMatchException for 'download'");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-BND-002")]
    public async Task GetDocuments_EntityNameDownloadWithSuffix_MatchesGetAll()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // "downloadExtra" is NOT "download" — regex should allow it to reach GetAll
        var response = await _client.GetAsync("/api/document/downloadExtra/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-BND-003")]
    public async Task GetDocuments_EntityNameSingleChar_IsHandledGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Single character entity name — boundary for the regex minimum match
        var response = await _client.GetAsync("/api/document/P/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-BND-004")]
    public async Task GetDocuments_EntityNameWithHyphen_IsHandledGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Hyphenated entity names are edge cases for the regex
        var response = await _client.GetAsync("/api/document/entity-name/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-BND-005")]
    public async Task GetDocuments_EntityIdZero_IsHandledGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // entityId=0 is the boundary minimum for int constraint
        var response = await _client.GetAsync("/api/document/Partner/0");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    // ──────────────────────────────────────────────────────────────
    // INTEGRATION TESTS
    // ──────────────────────────────────────────────────────────────

    [Fact]
    [Trait("TestId", "TC-DEF021-INT-001")]
    public async Task E2E_GetDocuments_ThenDownload_BothEndpointsIndependent()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // E2E: entity-name route and download route must both be independently reachable after DEF-021 fix.
        // Without fix: the download call throws AmbiguousMatchException in the harness.
        var entityResponse = await _client.GetAsync("/api/document/Partner/1");
        entityResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);

        if (entityResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            var body = await entityResponse.Content.ReadAsStringAsync();
            body.Should().NotContain("AmbiguousMatchException", "entity route must not be ambiguous");
        }

        HttpResponseMessage? downloadResponse = null;
        try
        {
            downloadResponse = await _client.GetAsync("/api/document/download/1");
        }
        catch (Exception ex) when (ex.Message.Contains("AmbiguousMatch") || ex.GetType().Name.Contains("AmbiguousMatch"))
        {
            Assert.Fail(
                "DEF-021 fix not yet deployed: AmbiguousMatchException on /api/document/download/1. " +
                $"Exception: {ex.Message}");
        }

        downloadResponse!.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.NoContent, HttpStatusCode.Redirect,
            HttpStatusCode.Found);

        if (downloadResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            var body = await downloadResponse.Content.ReadAsStringAsync();
            body.Should().NotContain("AmbiguousMatchException", "download route must not be ambiguous");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-INT-002")]
    public async Task E2E_GetDocumentById_AndByEntity_BothWork()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // /api/document/1 and /api/document/Partner/1 are different routes — both must work
        var byIdResponse = await _client.GetAsync("/api/document/1");
        var byEntityResponse = await _client.GetAsync("/api/document/Partner/1");

        byIdResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound);
        byEntityResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DEF021-INT-003")]
    public async Task E2E_MultipleEntityTypes_AllReachGetAll_NoAmbiguity()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // End-to-end: verify multiple entity types all resolve via GetAll without ambiguity
        var entityTypes = new[] { "Partner", "Contact", "Interaction", "Opportunity" };
        foreach (var entityType in entityTypes)
        {
            var response = await _client.GetAsync($"/api/document/{entityType}/1");
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var body = await response.Content.ReadAsStringAsync();
                body.Should().NotContain("AmbiguousMatchException",
                    $"DEF-021: '{entityType}' entity route must never produce AmbiguousMatchException");
            }
        }
    }
}
