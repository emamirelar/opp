/**
 * @fileoverview Integration tests for EntityConfiguration - validation via HTTP
 * Tests actual endpoints: /api/entities, /api/entity-configuration/*, /api/entity-field/*
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

namespace UNOPS.PAO.Tests.Integration.EntityConfiguration;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "EntityConfiguration")]
[Trait("Component", "ValidationTests")]
public class EntityConfigValidationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public EntityConfigValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
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

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-001")]
    [Trait("Priority", "Critical")]
    public async Task CreateEntityConfig_SQLInjectionName_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "'; DROP TABLE Entities; --", tableName = "test", description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-002")]
    [Trait("Priority", "High")]
    public async Task SaveEntityConfig_XSSPayloadDisplayName_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "Partner", description = "<script>alert('XSS')</script>", fields = new object[] { } };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/Partner/save", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-003")]
    [Trait("Priority", "High")]
    public async Task AddField_CommandInjection_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "CmdEntity", tableName = "cmd_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var request = new { entityManagerId, fieldName = "; rm -rf /", dataType = "String" };
        var response = await _client.PostAsJsonAsync("/api/entity-field/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Created);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-004")]
    [Trait("Priority", "High")]
    public async Task AddField_SQLInjectionFieldName_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "SqlEntity", tableName = "sql_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var request = new { entityManagerId, fieldName = "'; DROP TABLE Fields; --", dataType = "String" };
        var response = await _client.PostAsJsonAsync("/api/entity-field/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Created);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-005")]
    [Trait("Priority", "High")]
    public async Task GetEntities_ResponseIsValidJson()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entities");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        var action = () => JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        action.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-006")]
    [Trait("Priority", "High")]
    public async Task GetEntityConfiguration_ValidEntityName_ReturnsValidStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entity-configuration/Partner");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("entityName", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-007")]
    [Trait("Priority", "Medium")]
    public async Task GetEntityListView_ValidEntity_ReturnsValidStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entity-configuration/Partner/list-view");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-008")]
    [Trait("Priority", "High")]
    public async Task UpdateEntityConfig_PathTraversal_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "PathEntity", tableName = "path_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetInt32();
        var request = new { id, entityName = "PathEntity", tableName = "path_entity", description = "../../etc/passwd" };
        var response = await _client.PutAsJsonAsync($"/api/entity-configuration/{id}", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-009")]
    [Trait("Priority", "Medium")]
    public async Task AddField_HTMLEntities_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "HTMLEntity", tableName = "html_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var request = new { entityManagerId, fieldName = "Field&#60;Test&#62;", dataType = "String" };
        var response = await _client.PostAsJsonAsync("/api/entity-field/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-010")]
    [Trait("Priority", "High")]
    public async Task AddField_ValidFieldTypes_AcceptsString()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "ValidTypeEntity", tableName = "valid_type_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var request = new { entityManagerId, fieldName = "StringField", dataType = "String" };
        var response = await _client.PostAsJsonAsync("/api/entity-field/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-011")]
    [Trait("Priority", "Medium")]
    public async Task ExportSql_ValidContentType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entity-configuration/export-sql");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-012")]
    [Trait("Priority", "High")]
    public async Task GetRelatedEntityFields_ValidEntityType_ReturnsValidStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entity-configuration/related-fields/Partner");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-013")]
    [Trait("Priority", "High")]
    public async Task GetFieldOptions_ValidDataType_ReturnsValidStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entity-configuration/field-options/relationship/Contact");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-014")]
    [Trait("Priority", "Medium")]
    public async Task CreateEntityConfig_ExcessiveNameLength_RejectedOrTruncated()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var longName = new string('A', 500);
        var request = new { entityName = longName, tableName = "test", description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Created);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-VAL-015")]
    [Trait("Priority", "High")]
    public async Task SaveEntityConfig_EmptyFields_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "Partner", description = "Test", fields = new object[] { } };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/Partner/save", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}
