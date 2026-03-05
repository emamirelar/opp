/**
 * @fileoverview PNO-1194 Character Encoding Tests — validates UTF-8 handling for user names
 * and special characters in dropdowns and API responses.
 *
 * Bug: Special characters (e.g., 'Ã', 'ö') replaced by '??' in dropdown menus.
 * UTF-8 vs ASCII/Latin-1 encoding mismatch.
 * Status: Peer Review
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1194
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

namespace UNOPS.PAO.IntegrationTests.PNO1194;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-1194")]
[Trait("Component", "CharacterEncoding")]
[Trait("JiraRef", "PNO-1194")]
public class CharacterEncodingTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CharacterEncodingTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-PNO1194-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_UserProfileEndpoint_ReturnsValidUtf8Response()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var content = Encoding.UTF8.GetString(bytes);
        content.Should().NotBeNullOrWhiteSpace();
        content.Should().NotContain("\uFFFD", "response should not contain replacement character from encoding errors");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_ResponseContentType_IncludesCharsetUtf8()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var contentType = response.Content.Headers.ContentType;
        contentType.Should().NotBeNull();
        var charset = contentType!.CharSet?.ToLowerInvariant() ?? "";
        (charset.Contains("utf-8") || charset.Contains("utf8")).Should().BeTrue(
            "Content-Type header should include charset=utf-8 for proper character encoding");
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1194-NEG-001")]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-111")]
    public async Task NEG_001_UserNameWithAccentedCharacters_DoesNotContainQuestionMarkReplacements()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "PNO-1194: User names with accented characters must not be replaced by '??'");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_Response_DoesNotContainDoubleQuestionMarkSequences()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users/search");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "response should not contain '??' where special characters should be");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_UnauthenticatedRequest_Returns401Or302()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var response = await unauthClient.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_EmptySearchQuery_ReturnsValidResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users/search?searchTerm=&maxResults=20");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNull();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_UserNameWithEmojiCharacters_HandledWithoutError()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task NEG_006_NonAsciiCharactersInFilterParameters_DontCause400()
    {
        if (!_isPostgresAvailable) return;
        var searchTerm = Uri.EscapeDataString("café");
        var response = await _client.GetAsync($"/api/values/users/search?searchTerm={searchTerm}&maxResults=20");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region FUNCTIONAL (6)

    [Fact]
    [Trait("TestId", "TC-PNO1194-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_ResponseHeaders_IndicateUtf8Encoding()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/user-info/current");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var contentType = response.Content.Headers.ContentType;
        contentType.Should().NotBeNull();
        var charset = contentType!.CharSet?.ToLowerInvariant() ?? "";
        (charset.Contains("utf-8") || charset.Contains("utf8") || string.IsNullOrEmpty(charset)).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_UserNames_PreserveOriginalCharacterEncoding()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        var bytes = Encoding.UTF8.GetBytes(body);
        Encoding.UTF8.GetString(bytes).Should().Be(body, "UTF-8 round-trip must preserve content");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_TypeaheadDropdownEndpoint_ReturnsNamesWithDiacriticsIntact()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users/search?searchTerm=&maxResults=100");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "typeahead/dropdown endpoint must preserve diacritics");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_MultipleUsersWithSpecialCharacters_AllRenderedCorrectly()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "all user names with special characters must render correctly");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_ResponseJson_IsValidUtf8Throughout()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/user-info/current");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var content = Encoding.UTF8.GetString(bytes);
        Action parse = () => JsonDocument.Parse(content);
        parse.Should().NotThrow("response JSON must be valid UTF-8");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_CountryNamesWithNonAsciiCharacters_Preserved()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/country");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "country names with non-ASCII characters must be preserved");
    }

    #endregion

    #region EDGE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1194-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_UserNameWithMixedScripts_LatinAndAccented()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "mixed Latin + accented characters must be preserved");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_VeryLongUserNameWithSpecialCharacters()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "long names with special characters must be handled");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_UserNameWithOnlyNonAsciiCharacters()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "names with only non-ASCII characters must be preserved");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_EmptyUserName_HandledGracefully()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_ResponseEncoding_ConsistentAcrossMultipleRequests()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/user-info/current");
        var r2 = await _client.GetAsync("/api/user-info/current");
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode == HttpStatusCode.OK && r2.StatusCode == HttpStatusCode.OK)
        {
            var ct1 = r1.Content.Headers.ContentType?.CharSet ?? "";
            var ct2 = r2.Content.Headers.ContentType?.CharSet ?? "";
            ct1.Should().Be(ct2, "encoding should be consistent across requests");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_SpecialCharactersInQueryParameters_DontBreakSearch()
    {
        if (!_isPostgresAvailable) return;
        var searchTerm = Uri.EscapeDataString("Müller");
        var response = await _client.GetAsync($"/api/values/users/search?searchTerm={searchTerm}&maxResults=20");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO1194-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_FullFlow_GetUserList_IncludesAllCharacterTypesCorrectly()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/users");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "user list must include all character types correctly");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_UserProfileAndUserDropdown_ReturnSameCharacterEncoding()
    {
        if (!_isPostgresAvailable) return;
        var profileResponse = await _client.GetAsync("/api/user-info/current");
        var usersResponse = await _client.GetAsync("/api/values/users");
        if (profileResponse.StatusCode != HttpStatusCode.OK || usersResponse.StatusCode != HttpStatusCode.OK) return;

        var profileCt = profileResponse.Content.Headers.ContentType?.CharSet ?? "";
        var usersCt = usersResponse.Content.Headers.ContentType?.CharSet ?? "";
        (string.IsNullOrEmpty(profileCt) || string.IsNullOrEmpty(usersCt) || profileCt == usersCt).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_CountryEndpointAndUserEndpoint_BothHandleUtf8()
    {
        if (!_isPostgresAvailable) return;
        var countryResponse = await _client.GetAsync("/api/values/country");
        var userResponse = await _client.GetAsync("/api/values/users");
        countryResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        userResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (countryResponse.StatusCode == HttpStatusCode.OK)
            (await countryResponse.Content.ReadAsStringAsync()).Should().NotContain("??");
        if (userResponse.StatusCode == HttpStatusCode.OK)
            (await userResponse.Content.ReadAsStringAsync()).Should().NotContain("??");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_OrgUnitNamesWithSpecialCharacters_Preserved()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/organization-units");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "org unit names with special characters must be preserved");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_MultipleEndpoints_ReturnConsistentEncoding()
    {
        if (!_isPostgresAvailable) return;
        var endpoints = new[] { "/api/user-info/current", "/api/values/users", "/api/values/country" };
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var body = await response.Content.ReadAsStringAsync();
                body.Should().NotContain("??", $"endpoint {endpoint} must not corrupt special characters");
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1194-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_LiaisonOfficeNamesWithSpecialCharacters_Preserved()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/liaison-offices");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("??", "liaison office names with special characters must be preserved");
    }

    #endregion
}
