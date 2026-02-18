/**
 * @fileoverview Edge case integration tests for UserProfileController
 * Tests boundary conditions against actual API: POST /api/profile, PUT /api/user-info/update, GET /api/user-info/current
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.UserProfile;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "UserProfile")]
[Trait("Component", "EdgeCaseTests")]
public class UserProfileEdgeCaseTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserProfileEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-001")]
    public async Task GetUserInfoCurrent_ReturnsCurrentUserInfo()
    {
        var response = await _client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("userInfoWithOrgSettings", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-002")]
    public async Task GetUserInfoCurrent_CalledMultipleTimes_ReturnsSameData()
    {
        var response1 = await _client.GetAsync("/api/user-info/current");
        var response2 = await _client.GetAsync("/api/user-info/current");
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        var json1 = await response1.Content.ReadAsStringAsync();
        var json2 = await response2.Content.ReadAsStringAsync();
        json1.Should().Be(json2);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-003")]
    public async Task PostProfile_WithEmptyBody_HandlesGracefully()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/profile", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-004")]
    public async Task PostProfile_WithMinimalValidData_ReturnsSuccess()
    {
        var body = new { email = "testuser@unops.org", firstName = "Test", lastName = "User" };
        var response = await _client.PostAsJsonAsync("/api/profile", body, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-005")]
    public async Task PutUserInfoUpdate_WithEmptyBody_HandlesGracefully()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/user-info/update", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-006")]
    public async Task PutUserInfoUpdate_WithMinimalValidData_ReturnsSuccess()
    {
        var body = new
        {
            userId = 123,
            userEmail = "testuser@unops.org",
            firstName = "Test",
            lastName = "User"
        };
        var response = await _client.PutAsJsonAsync("/api/user-info/update", body, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-007")]
    public async Task GetUserInfoCurrent_WithExtraQueryParams_IgnoresThem()
    {
        var response = await _client.GetAsync("/api/user-info/current?foo=bar&baz=qux");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-008")]
    public async Task PostProfile_WithWhitespaceOnlyFields_HandlesGracefully()
    {
        var body = new { email = "testuser@unops.org", firstName = "   ", lastName = "   " };
        var response = await _client.PostAsJsonAsync("/api/profile", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-009")]
    public async Task GetUserInfoCurrent_ResponseTime_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/user-info/current");
        sw.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-010")]
    public async Task GetUserInfoCurrent_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-011")]
    public async Task PostProfile_WithMaxLengthFields_HandlesGracefully()
    {
        var longName = new string('A', 500);
        var body = new { email = "testuser@unops.org", firstName = longName, lastName = longName };
        var response = await _client.PostAsJsonAsync("/api/profile", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-EDGE-012")]
    public async Task PutUserInfoUpdate_WithSpecialCharactersInName_HandlesCorrectly()
    {
        var body = new
        {
            userId = 123,
            userEmail = "testuser@unops.org",
            firstName = "José-María",
            lastName = "O'Brien-Smith"
        };
        var response = await _client.PutAsJsonAsync("/api/user-info/update", body, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
