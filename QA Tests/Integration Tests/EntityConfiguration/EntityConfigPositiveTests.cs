/**
 * @fileoverview Positive integration tests for EntityConfiguration
 * Tests happy-path scenarios against actual API endpoints
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.EntityConfiguration;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "EntityConfiguration")]
[Trait("Component", "PositiveTests")]
public class EntityConfigPositiveTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/entity-configuration";

    public EntityConfigPositiveTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-EC-POS-001")]
    public async Task GetEntities_WithValidAuth_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entities");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-EC-POS-002")]
    public async Task GetEntityConfiguration_ForPartner_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/Partner");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-EC-POS-003")]
    public async Task GetEntityListView_ForPartner_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/Partner/list-view");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }
}
