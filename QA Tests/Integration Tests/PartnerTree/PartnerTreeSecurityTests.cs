/**
 * @fileoverview Integration tests for PartnerTreeController - security via HTTP
 * Tests unauthenticated access against real partner tree endpoints
 * @author UNOPS Opportunity+ Test Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.PartnerTree;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "PartnerTree")]
[Trait("Component", "SecurityTests")]
public class PartnerTreeSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerTreeSecurityTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = CreateAuthenticatedClient(factory);
    }

    private static HttpClient CreateAuthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    private static HttpClient CreateUnauthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-001")]
    public async Task GetPartnerTree_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/partner-tree");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-002")]
    public async Task GetPartnerTreeById_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/partner-tree/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-003")]
    public async Task PostPartnerTree_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var body = new { name = "Test", code = "T", description = "D", type = "Category" };
        var response = await client.PostAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-004")]
    public async Task PutPartnerTree_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var body = new[] { new { id = 1, name = "X", code = "X", description = "X", type = "Category" } };
        var response = await client.PutAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-005")]
    public async Task DeletePartnerTree_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.DeleteAsync("/api/partner-tree/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-006")]
    public async Task GetPartnerTreePermissions_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/partner-tree/1/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-007")]
    public async Task GetPartnerTreeStructure_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/partner-tree-structure");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-008")]
    public async Task GetCategoriesSummary_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/partner-tree/categories-summary");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-009")]
    public async Task GetGroupsSummary_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/partner-tree/groups-summary");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-SEC-010")]
    public async Task GetCategorizationOverview_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/partner-tree/categorization-overview");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }
}
