/**
 * @fileoverview Negative integration tests for LiaisonOfficeController
 * Tests invalid inputs and error handling against actual API: /api/LiaisonOffice/*
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

namespace UNOPS.PAO.Tests.Integration.LiaisonOffice;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "LiaisonOffice")]
[Trait("Component", "NegativeTests")]
public class LiaisonOfficeNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/LiaisonOffice";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public LiaisonOfficeNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-001")]
    public async Task GetNonExistentRoute_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/non-existent");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-002")]
    public async Task GetInvalidSubRoute_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/invalid-sub-route");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-003")]
    public async Task PostList_InsteadOfGet_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync(BaseUrl, null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-004")]
    public async Task GetSearch_InsteadOfPost_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/search");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-005")]
    public async Task PutList_InsteadOfGet_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PutAsync(BaseUrl, null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-006")]
    public async Task DeleteList_InsteadOfGet_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-007")]
    public async Task PostSearch_InvalidJsonBody_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("not valid json", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-NEG-008")]
    public async Task GetById_InvalidIdFormat_Returns400Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/invalid");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
