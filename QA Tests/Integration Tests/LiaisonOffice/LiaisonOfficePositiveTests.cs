/**
 * @fileoverview Positive integration tests for LiaisonOffice
 * Tests happy-path scenarios against actual API endpoints
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

namespace UNOPS.PAO.Tests.Integration.LiaisonOffice;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "LiaisonOffice")]
[Trait("Component", "PositiveTests")]
public class LiaisonOfficePositiveTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/LiaisonOffice";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public LiaisonOfficePositiveTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-LO-POS-001")]
    public async Task GetLiaisonOffices_WithValidAuth_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-LO-POS-002")]
    public async Task GetLiaisonOffice_ById1_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/1");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-LO-POS-003")]
    public async Task GetLiaisonOfficeSearch_WithValidTerm_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/search?term=office");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }
}
