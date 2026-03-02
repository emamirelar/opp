/**
 * @fileoverview Integration tests for EntityConfiguration - security via HTTP
 * Tests actual endpoints: /api/entities, /api/entity-configuration/*, /api/entity-field/*
 * @author UNOPS Opportunity+ Test Team
 */

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.EntityConfiguration;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "EntityConfiguration")]
[Trait("Component", "SecurityTests")]
public class EntityConfigSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EntityConfigSecurityTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-ECFG-SEC-001")]
    [Trait("Priority", "Critical")]
    public async Task GetEntities_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/entities");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-002")]
    [Trait("Priority", "Critical")]
    public async Task GetEntityConfiguration_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/entity-configuration/Partner");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-003")]
    [Trait("Priority", "Critical")]
    public async Task GetAllEntityConfigurations_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/entity-configuration");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-004")]
    [Trait("Priority", "Critical")]
    public async Task CreateEntityConfiguration_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var request = new { entityName = "Test", tableName = "test", description = "Test" };
        var response = await client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-005")]
    [Trait("Priority", "Critical")]
    public async Task SaveEntityConfiguration_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var request = new { entityName = "Partner", description = "Test", fields = new object[] { } };
        var response = await client.PostAsJsonAsync("/api/entity-configuration/Partner/save", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-006")]
    [Trait("Priority", "High")]
    public async Task GetRelatedEntityFields_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/entity-configuration/related-fields/Partner");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-007")]
    [Trait("Priority", "High")]
    public async Task GetFieldOptions_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/entity-configuration/field-options/relationship/Contact");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-008")]
    [Trait("Priority", "High")]
    public async Task ExportSql_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/entity-configuration/export-sql");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-009")]
    [Trait("Priority", "Critical")]
    public async Task DeleteEntityConfiguration_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.DeleteAsync("/api/entity-configuration/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-SEC-010")]
    [Trait("Priority", "High")]
    public async Task GetEntityFields_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/entity-configuration/1/fields");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
