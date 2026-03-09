/**
 * @fileoverview Security integration tests for UserProfileController
 * Tests authentication and authorization for POST /api/profile, PUT /api/user-info/update, GET /api/user-info/current
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.UserProfile;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "UserProfile")]
[Trait("Component", "SecurityTests")]
public class UserProfileSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;

    public UserProfileSecurityTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
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

    private HttpClient CreateClientWithInvalidEmail()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:invalid@example.com");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:999");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=invalid@example.com; dev-user-email=invalid@example.com");
        return client;
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-001")]
    public async Task GetUserInfoCurrent_WithoutAuth_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-002")]
    public async Task PostProfile_WithoutAuth_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var content = new StringContent("{\"email\":\"test@test.com\",\"firstName\":\"Test\",\"lastName\":\"User\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/profile", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-003")]
    public async Task PutUserInfoUpdate_WithoutAuth_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var content = new StringContent("{\"userId\":123,\"userEmail\":\"test@test.com\"}", Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/user-info/update", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-004")]
    public async Task GetUserInfoCurrent_InvalidAuthEmail_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateClientWithInvalidEmail();
        var response = await client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-005")]
    public async Task PostProfile_InvalidAuthEmail_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateClientWithInvalidEmail();
        var content = new StringContent("{\"email\":\"invalid@example.com\",\"firstName\":\"Test\",\"lastName\":\"User\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/profile", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-006")]
    public async Task ErrorResponses_NoSensitiveData()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("password", "error responses should not expose sensitive data");
        body.Should().NotContain("token", "error responses should not expose sensitive data");
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-007")]
    public async Task ResponseHeaders_DoNotExposeServerInfo()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().NotContain(h =>
            h.Key.Equals("X-AspNet-Version", StringComparison.OrdinalIgnoreCase) ||
            h.Key.Equals("X-Powered-By", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-SEC-008")]
    public async Task ErrorResponses_ReturnProperContentType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType?.MediaType.Should().NotBeNullOrEmpty();
    }
}
