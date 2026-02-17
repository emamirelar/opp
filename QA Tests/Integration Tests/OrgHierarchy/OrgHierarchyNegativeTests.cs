/**
 * @fileoverview Negative integration tests for OrganizationHierarchyController
 * Tests invalid inputs and error handling against actual API: /api/organizationhierarchy/*
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
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

namespace UNOPS.PAO.Tests.Integration.OrgHierarchy;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "OrgHierarchy")]
[Trait("Component", "NegativeTests")]
public class OrgHierarchyNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/organizationhierarchy";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public OrgHierarchyNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-001")]
    public async Task GetNonExistentRoute_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/non-existent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-002")]
    public async Task GetInvalidSubRoute_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/invalid-sub-route");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-003")]
    public async Task PostList_InsteadOfGet_Returns405()
    {
        var response = await _client.PostAsync(BaseUrl, null);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-004")]
    public async Task GetSearch_InsteadOfPost_Returns405()
    {
        var response = await _client.GetAsync($"{BaseUrl}/search");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-005")]
    public async Task PutList_InsteadOfGet_Returns405()
    {
        var response = await _client.PutAsync(BaseUrl, null);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-006")]
    public async Task DeleteList_InsteadOfGet_Returns405()
    {
        var response = await _client.DeleteAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-007")]
    public async Task PostSearch_InvalidJsonBody_Returns400()
    {
        var content = new StringContent("not valid json", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-NEG-008")]
    public async Task GetById_InvalidIdFormat_Returns400Or404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/invalid");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
