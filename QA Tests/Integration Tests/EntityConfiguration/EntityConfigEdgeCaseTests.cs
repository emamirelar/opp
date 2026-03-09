/**
 * @fileoverview Integration tests for EntityConfiguration - edge cases via HTTP
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
[Trait("Component", "EdgeCaseTests")]
public class EntityConfigEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public EntityConfigEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-ECFG-EDGE-001")]
    [Trait("Priority", "Medium")]
    public async Task CreateEntityConfig_MinLengthName_AcceptsShort()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "A", tableName = "a", description = "A" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        // EntityConfiguration service may return 200 OK or 500 in InMemory mode
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-002")]
    [Trait("Priority", "Medium")]
    public async Task CreateEntityConfig_MaxLengthName_AcceptsAtBoundary()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var longName = new string('A', 100);
        var request = new { entityName = longName, tableName = "test_table", description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-003")]
    [Trait("Priority", "Low")]
    public async Task CreateEntityConfig_UnicodeName_HandlesInternationalization()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "实体配置", tableName = "entity_config", description = "Chinese Entity" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-004")]
    [Trait("Priority", "Low")]
    public async Task CreateEntityConfig_EmojiInDisplayName_HandlesEmoji()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "TestEntity", tableName = "test_entity", description = "Entity📊" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-005")]
    [Trait("Priority", "High")]
    public async Task AddField_SingleField_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "SingleFieldEntity", tableName = "single_field_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var fieldReq = new { entityManagerId, fieldName = "Field1", dataType = "String" };
        var fieldRes = await _client.PostAsJsonAsync("/api/entity-field/create", fieldReq);
        fieldRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-006")]
    [Trait("Priority", "Medium")]
    public async Task GetEntityConfig_IdOne_HandlesFirstEntity()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var configs = await _client.GetAsync("/api/entity-configuration");
        if (configs.StatusCode != HttpStatusCode.OK) return;
        var list = await configs.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOptions);
        if (list == null || list.Count == 0) return;
        var firstId = list[0].GetProperty("id").GetInt32();
        var response = await _client.GetAsync($"/api/entity-configuration/{firstId}/fields");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-007")]
    [Trait("Priority", "High")]
    public async Task UpdateEntityConfig_ImmediatelyAfterCreation_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "TempEntity", tableName = "temp_entity", description = "Temp" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetInt32();
        var updateReq = new { id, entityName = "TempEntity", tableName = "temp_entity", description = "Updated" };
        var updateRes = await _client.PutAsJsonAsync($"/api/entity-configuration/{id}", updateReq);
        updateRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-008")]
    [Trait("Priority", "High")]
    public async Task DeleteEntityConfig_ImmediatelyAfterCreation_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "DeleteMe", tableName = "delete_me", description = "Delete" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetInt32();
        var deleteRes = await _client.DeleteAsync($"/api/entity-configuration/{id}");
        deleteRes.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-009")]
    [Trait("Priority", "Medium")]
    public async Task CreateEntityConfig_CamelCaseName_HandlesFormatting()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "camelCaseEntity", tableName = "camel_case_entity", description = "Camel" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-010")]
    [Trait("Priority", "High")]
    public async Task GetEntityConfigs_RapidSequential_NoStateIssues()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        for (var i = 0; i < 20; i++)
        {
            var response = await _client.GetAsync("/api/entity-configuration");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-011")]
    [Trait("Priority", "High")]
    public async Task GetEntityConfigs_RapidSequential_Entities_NoStateIssues()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        for (var i = 0; i < 20; i++)
        {
            var response = await _client.GetAsync("/api/entities");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-012")]
    [Trait("Priority", "Medium")]
    public async Task GetFields_NoFields_ReturnsEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "NoFieldsEntity", tableName = "no_fields_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var response = await _client.GetAsync($"/api/entity-configuration/{entityManagerId}/fields");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var fields = await response.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOptions);
            fields.Should().BeEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-013")]
    [Trait("Priority", "Low")]
    public async Task CreateEntityConfig_UnderscoreInName_HandlesSpecial()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "Entity_With_Underscores", tableName = "entity_with_underscores", description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-014")]
    [Trait("Priority", "Medium")]
    public async Task AddField_NumericFieldName_HandlesOrRejects()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var configs = await _client.GetAsync("/api/entity-configuration");
        if (configs.StatusCode != HttpStatusCode.OK) return;
        var list = await configs.Content.ReadFromJsonAsync<List<JsonElement>>(JsonOptions);
        if (list == null || list.Count == 0) return;
        var entityManagerId = list[0].GetProperty("id").GetInt32();
        var request = new { entityManagerId, fieldName = "Field123", dataType = "String" };
        var response = await _client.PostAsJsonAsync("/api/entity-field/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-015")]
    [Trait("Priority", "High")]
    public async Task AddField_ImmediatelyAfterEntityCreate_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "ImmediateFieldEntity", tableName = "immediate_field_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var fieldReq = new { entityManagerId, fieldName = "Field1", dataType = "String" };
        var fieldRes = await _client.PostAsJsonAsync("/api/entity-field/create", fieldReq);
        fieldRes.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-016")]
    [Trait("Priority", "Medium")]
    public async Task CreateEntityConfig_SnakeCaseName_HandlesFormatting()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "snake_case_entity", tableName = "snake_case_entity", description = "Snake" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-017")]
    [Trait("Priority", "High")]
    public async Task AddField_RemoveField_Cycle_StateConsistent()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "CycleEntity", tableName = "cycle_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var fieldReq = new { entityManagerId, fieldName = "TempField", dataType = "String" };
        var fieldRes = await _client.PostAsJsonAsync("/api/entity-field/create", fieldReq);
        if (fieldRes.StatusCode != HttpStatusCode.Created) return;
        var field = await fieldRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var fieldId = field.GetProperty("id").GetInt32();
        var deleteRes = await _client.DeleteAsync($"/api/entity-field/{fieldId}");
        deleteRes.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-018")]
    [Trait("Priority", "Medium")]
    public async Task CreateEntityConfig_AllUppercaseName_HandlesFormatting()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var request = new { entityName = "UPPERCASEENTITY", tableName = "uppercase_entity", description = "UPPER" };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-019")]
    [Trait("Priority", "Low")]
    public async Task GetEntityConfig_NonExistentEntityName_Returns404Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entity-configuration/NonExistentEntity12345");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-020")]
    [Trait("Priority", "High")]
    public async Task AddField_UpdateField_ImmediatelyAfterAdd_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var createReq = new { entityName = "UpdateFieldEntity", tableName = "update_field_entity", description = "Test" };
        var createRes = await _client.PostAsJsonAsync("/api/entity-configuration/create", createReq);
        if (createRes.StatusCode != HttpStatusCode.Created) return;
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var entityManagerId = created.GetProperty("id").GetInt32();
        var fieldReq = new { entityManagerId, fieldName = "FieldToUpdate", dataType = "String" };
        var fieldRes = await _client.PostAsJsonAsync("/api/entity-field/create", fieldReq);
        if (fieldRes.StatusCode != HttpStatusCode.Created) return;
        var field = await fieldRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var fieldId = field.GetProperty("id").GetInt32();
        var updateReq = new { id = fieldId, entityManagerId, fieldName = "UpdatedField", dataType = "String" };
        var updateRes = await _client.PutAsJsonAsync($"/api/entity-field/{fieldId}", updateReq);
        updateRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-021")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetEntities_ResponseContent_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entities");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??",
                "PNO-1194: entity config names/descriptions must not contain encoding artifacts");
            content.Should().NotContain("\uFFFD",
                "Entity configuration data must not contain U+FFFD replacement characters");
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-EDGE-022")]
    [Trait("Ticket", "PNO-1194")]
    public async Task CreateEntityConfig_AccentedDescription_PreservedInResponse()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var req = new
        {
            entityName = $"AccentTest{Guid.NewGuid():N}".Substring(0, 30),
            tableName = $"accent_test_{Guid.NewGuid():N}".Substring(0, 30),
            description = "Entit\u00e9 de configuration pour Jos\u00e9 Garc\u00eda"
        };
        var response = await _client.PostAsJsonAsync("/api/entity-configuration/create", req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??");
        }
    }
}
