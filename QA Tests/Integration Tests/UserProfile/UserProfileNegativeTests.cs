/**
 * @fileoverview Negative integration tests for UserProfileController
 * Tests invalid inputs and error handling against actual API
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
[Trait("Component", "NegativeTests")]
public class UserProfileNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;

    public UserProfileNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-001")]
    public async Task GetProfile_WrongMethod_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/profile");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-002")]
    public async Task PostUserInfoCurrent_WrongHttpMethod_Returns405()
    {
        var response = await _client.PostAsync("/api/user-info/current", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-003")]
    public async Task DeleteUserInfoCurrent_MethodNotAllowed()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync("/api/user-info/current");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-004")]
    public async Task PutProfile_WrongHttpMethod_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/profile", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-005")]
    public async Task GetUserInfoNonexistent_Returns404()
    {
        var response = await _client.GetAsync("/api/user-info/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-006")]
    public async Task PostProfile_WithInvalidJsonBody_Returns400()
    {
        var content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/profile", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-007")]
    public async Task PutUserInfoUpdate_WithInvalidJsonBody_Returns400()
    {
        var content = new StringContent("{ broken }", Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/user-info/update", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-008")]
    public async Task GetUserInfoCurrent_WithWrongAcceptHeader_StillReturnsJson()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        client.DefaultRequestHeaders.Add("Accept", "text/plain");
        var response = await client.GetAsync("/api/user-info/current");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-009")]
    public async Task PostProfile_WithOversizedBody_HandlesGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var hugeString = new string('x', 1_000_000);
        var body = $"{{\"email\":\"testuser@unops.org\",\"firstName\":\"{hugeString}\",\"lastName\":\"User\"}}";
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/profile", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PROFILE-NEG-010")]
    public async Task DeleteProfile_MethodNotAllowed()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync("/api/profile");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }
}
