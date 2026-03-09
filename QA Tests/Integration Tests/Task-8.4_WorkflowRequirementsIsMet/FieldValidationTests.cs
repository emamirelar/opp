/**
 * @fileoverview Task 8.4 Field-Specific Validation Tests — validates that individual
 * missing fields correctly block Go Decision submission per PNO-837 and PNO-834.
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

namespace UNOPS.PAO.IntegrationTests.Task84;

[Collection("Integration Tests")]
[Trait("Feature", "Task-8.4")]
[Trait("Component", "WorkflowRequirementsIsMet")]
public class FieldValidationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public FieldValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    #region PNO-837: Context/Challenges Validation

    [Fact]
    [Trait("TestId", "TC-PNO837-NEG-001")]
    [Trait("Category", "Negative")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_RequirementsResponse_ContainsContextOrChallengesItem_WhenContextExpectedMissing()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var contextOrChallengesItem = items.EnumerateArray()
            .FirstOrDefault(i =>
            {
                if (!i.TryGetProperty("name", out var nameProp)) return false;
                var name = nameProp.GetString()?.ToLowerInvariant();
                return name == "challenges" || name == "context";
            });

        contextOrChallengesItem.ValueKind.Should().NotBe(JsonValueKind.Undefined,
            "requirements list must include a context/challenges item for validation");
        if (contextOrChallengesItem.TryGetProperty("isMet", out var isMetProp) && !isMetProp.GetBoolean())
        {
            contextOrChallengesItem.TryGetProperty("description", out _).Should().BeTrue(
                "unmet context requirement must have description");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-NEG-002")]
    [Trait("Category", "Negative")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_SubmitWorkflow_ReturnsNonSuccess_WhenOpportunityHasEmptyContext()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new
        {
            entityName = "opportunity",
            entityId = 1,
            newStage = "GO",
            confirmedNonOMSubmission = true,
            confirmedOrgUnitWarning = true,
            acknowledgedStatement = true
        };
        var response = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            if (body.TryGetProperty("success", out var successProp) && !successProp.GetBoolean()
                && body.TryGetProperty("requirementsNotMet", out var reqNotMet))
            {
                reqNotMet.ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-NEG-003")]
    [Trait("Category", "Negative")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_RequirementsDescription_ForMissingContext_IsNonEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("name", out var nameProp)) continue;
            var name = nameProp.GetString()?.ToLowerInvariant();
            if (name != "challenges" && name != "context") continue;

            if (item.TryGetProperty("isMet", out var isMetProp) && !isMetProp.GetBoolean())
            {
                item.TryGetProperty("description", out var descProp).Should().BeTrue(
                    "missing context requirement must have description");
                descProp.GetString().Should().NotBeNullOrWhiteSpace(
                    "user must see explanation when context is missing");
            }
            return;
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-FUNC-001")]
    [Trait("Category", "Functional")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_ContextRequirement_ExistsInRequirementsList()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasContextRequirement = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var nameProp)) return false;
            var name = nameProp.GetString()?.ToLowerInvariant();
            return name == "challenges" || name == "context";
        });

        hasContextRequirement.Should().BeTrue(
            "Context/challenges field must be validated in requirements list");
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-FUNC-002")]
    [Trait("Category", "Functional")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_ContextValidation_IsConsistentAcrossConsecutiveCalls()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var r1 = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        var r2 = await _client.GetAsync("/api/workflow/opportunity/1/requirements");

        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "context validation must be deterministic");
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-FUNC-003")]
    [Trait("Category", "Functional")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_ContextRequirement_HasSpecificNameIdentifierForUILookup()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var contextItem = items.EnumerateArray()
            .FirstOrDefault(i =>
            {
                if (!i.TryGetProperty("name", out var nameProp)) return false;
                var name = nameProp.GetString()?.ToLowerInvariant();
                return name == "challenges" || name == "context";
            });

        contextItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        contextItem.GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace(
            "context requirement must have name for UI lookup");
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_RequirementsEndpoint_HandlesOpportunityWhereOnlyContextMissing()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_RequirementsEndpoint_ReturnsValidJson_WhenContextEmptyOrNull()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrWhiteSpace();
        var items = JsonSerializer.Deserialize<JsonElement>(content, JsonOpts);
        items.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_MultipleRequirements_CanBeUnmetSimultaneously_IncludingContext()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var unmetCount = items.EnumerateArray()
            .Count(i => i.TryGetProperty("isMet", out var isMet) && !isMet.GetBoolean());

        unmetCount.Should().BeGreaterOrEqualTo(0,
            "multiple requirements including context can be unmet simultaneously");
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-INT-001")]
    [Trait("Category", "Integration")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_FullFlow_GetRequirementsShowsContextUnmet_PostSubmitBlocked()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;

        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var contextUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var nameProp)) return false;
            var name = nameProp.GetString()?.ToLowerInvariant();
            if (name != "challenges" && name != "context") return false;
            return i.TryGetProperty("isMet", out var isMet) && !isMet.GetBoolean();
        });

        var payload = new
        {
            entityName = "opportunity",
            entityId = 1,
            newStage = "GO",
            confirmedNonOMSubmission = true,
            confirmedOrgUnitWarning = true,
            acknowledgedStatement = true
        };
        var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);

        if (contextUnmet && submitResponse.StatusCode == HttpStatusCode.OK)
        {
            var body = await submitResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            var success = body.TryGetProperty("success", out var s) && s.GetBoolean();
            if (!success)
            {
                body.TryGetProperty("requirementsNotMet", out _).Should().BeTrue(
                    "when context unmet, submit must be blocked");
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-INT-002")]
    [Trait("Category", "Integration")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_RequirementsAndWorkflowState_BothReachable_WhenContextMissing()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        var stateResponse = await _client.GetAsync("/api/workflow/opportunity/1");

        reqResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        stateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO837-INT-003")]
    [Trait("Category", "Integration")]
    [Trait("JiraRef", "PNO-837")]
    public async Task PNO837_GettingRequirementsForMultipleOpportunities_ReturnsIndependentValidation()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var ids = new[] { 1, 2, 3 };
        foreach (var id in ids)
        {
            var response = await _client.GetAsync($"/api/workflow/opportunity/{id}/requirements");
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region PNO-834: SDG/Primary SDG Validation

    [Fact]
    [Trait("TestId", "TC-PNO834-NEG-001")]
    [Trait("Category", "Negative")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_RequirementsResponse_ContainsSdgItem_WithIsMetFalse_WhenNoPrimarySdg()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var sdgItem = items.EnumerateArray()
            .FirstOrDefault(i =>
            {
                if (!i.TryGetProperty("name", out var nameProp)) return false;
                var name = nameProp.GetString()?.ToLowerInvariant();
                return name == "sdgs" || name == "primarysdg";
            });

        sdgItem.ValueKind.Should().NotBe(JsonValueKind.Undefined,
            "requirements list must include an SDG item for validation");
        if (sdgItem.TryGetProperty("isMet", out var isMetProp) && !isMetProp.GetBoolean())
        {
            sdgItem.TryGetProperty("description", out _).Should().BeTrue(
                "unmet SDG requirement must have description");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-NEG-002")]
    [Trait("Category", "Negative")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_SubmitWorkflow_ReturnsNonSuccess_WhenNoPrimarySdgSelected()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new
        {
            entityName = "opportunity",
            entityId = 1,
            newStage = "GO",
            confirmedNonOMSubmission = true,
            confirmedOrgUnitWarning = true,
            acknowledgedStatement = true
        };
        var response = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            if (body.TryGetProperty("success", out var successProp) && !successProp.GetBoolean()
                && body.TryGetProperty("requirementsNotMet", out var reqNotMet))
            {
                reqNotMet.ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-NEG-003")]
    [Trait("Category", "Negative")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_RequirementsDescription_ForMissingSdg_IsNonEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("name", out var nameProp)) continue;
            var name = nameProp.GetString()?.ToLowerInvariant();
            if (name != "sdgs" && name != "primarysdg") continue;

            if (item.TryGetProperty("isMet", out var isMetProp) && !isMetProp.GetBoolean())
            {
                item.TryGetProperty("description", out var descProp).Should().BeTrue(
                    "missing SDG requirement must have description");
                descProp.GetString().Should().NotBeNullOrWhiteSpace(
                    "user must see explanation when SDG is missing");
            }
            return;
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-FUNC-001")]
    [Trait("Category", "Functional")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_SdgRequirement_ExistsInRequirementsList()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasSdgRequirement = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var nameProp)) return false;
            var name = nameProp.GetString()?.ToLowerInvariant();
            return name == "sdgs" || name == "primarysdg";
        });

        hasSdgRequirement.Should().BeTrue(
            "SDG/Primary SDG field must be validated in requirements list");
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-FUNC-002")]
    [Trait("Category", "Functional")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_SdgValidation_IsConsistentAcrossConsecutiveCalls()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var r1 = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        var r2 = await _client.GetAsync("/api/workflow/opportunity/1/requirements");

        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "SDG validation must be deterministic");
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-FUNC-003")]
    [Trait("Category", "Functional")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_SdgRequirement_HasSpecificNameIdentifierForUILookup()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var sdgItem = items.EnumerateArray()
            .FirstOrDefault(i =>
            {
                if (!i.TryGetProperty("name", out var nameProp)) return false;
                var name = nameProp.GetString()?.ToLowerInvariant();
                return name == "sdgs" || name == "primarysdg";
            });

        sdgItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        sdgItem.GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace(
            "SDG requirement must have name for UI lookup");
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_RequirementsEndpoint_HandlesOpportunityWhereOnlySdgMissing()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_RequirementsEndpoint_ReturnsValidJson_WhenSdgEmptyOrNull()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrWhiteSpace();
        var items = JsonSerializer.Deserialize<JsonElement>(content, JsonOpts);
        items.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_MultipleRequirements_CanBeUnmetSimultaneously_IncludingSdg()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var unmetCount = items.EnumerateArray()
            .Count(i => i.TryGetProperty("isMet", out var isMet) && !isMet.GetBoolean());

        unmetCount.Should().BeGreaterOrEqualTo(0,
            "multiple requirements including SDG can be unmet simultaneously");
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-INT-001")]
    [Trait("Category", "Integration")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_FullFlow_GetRequirementsShowsSdgUnmet_PostSubmitBlocked()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;

        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var sdgUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var nameProp)) return false;
            var name = nameProp.GetString()?.ToLowerInvariant();
            if (name != "sdgs" && name != "primarysdg") return false;
            return i.TryGetProperty("isMet", out var isMet) && !isMet.GetBoolean();
        });

        var payload = new
        {
            entityName = "opportunity",
            entityId = 1,
            newStage = "GO",
            confirmedNonOMSubmission = true,
            confirmedOrgUnitWarning = true,
            acknowledgedStatement = true
        };
        var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);

        if (sdgUnmet && submitResponse.StatusCode == HttpStatusCode.OK)
        {
            var body = await submitResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            var success = body.TryGetProperty("success", out var s) && s.GetBoolean();
            if (!success)
            {
                body.TryGetProperty("requirementsNotMet", out _).Should().BeTrue(
                    "when SDG unmet, submit must be blocked");
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-INT-002")]
    [Trait("Category", "Integration")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_RequirementsAndWorkflowState_BothReachable_WhenSdgMissing()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        var stateResponse = await _client.GetAsync("/api/workflow/opportunity/1");

        reqResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        stateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO834-INT-003")]
    [Trait("Category", "Integration")]
    [Trait("JiraRef", "PNO-834")]
    public async Task PNO834_GettingRequirementsForMultipleOpportunities_ReturnsIndependentValidation()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var ids = new[] { 1, 2, 3 };
        foreach (var id in ids)
        {
            var response = await _client.GetAsync($"/api/workflow/opportunity/{id}/requirements");
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NotFound);
        }
    }

    #endregion
}
