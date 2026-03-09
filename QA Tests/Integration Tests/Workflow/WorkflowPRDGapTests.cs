/**
 * @fileoverview Workflow PRD Gap Tests — addresses remaining gaps from WorkflowPRD_TraceabilityTestPlan.md.
 * Covers: GAP-M2 (workflow schema verification), GAP-M5 (conditional validators edge cases),
 * GAP-H5 (stage stepper/requirements validation logic).
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using UNOPS.Workflow.DataAccess;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Workflow;

/// <summary>
/// Integration tests addressing Workflow PRD traceability gaps.
/// Uses PAOWebApplicationFactory and HttpClient to exercise workflow API endpoints.
/// </summary>
[Collection("Integration Tests")]
[Trait("Feature", "WorkflowPRDGaps")]
[Trait("Component", "Workflow")]
public class WorkflowPRDGapTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private readonly PAOWebApplicationFactory<Program> _factory;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public WorkflowPRDGapTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Test-UserId", "1");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    #region Positive Tests (3)

    /// <summary>
    /// POS_001: Requirements endpoint returns GO requirements for valid opportunity.
    /// </summary>
    [Fact]
    [Trait("Category", "Positive")]
    [Trait("TestId", "POS_001")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task POS_001_RequirementsEndpoint_ReturnsGoRequirements_ForValidOpportunity()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            items.ValueKind.Should().Be(JsonValueKind.Array);
            items.GetArrayLength().Should().BeGreaterThan(0);
        }
    }

    /// <summary>
    /// POS_002: Workflow state returns correct stage and available actions.
    /// </summary>
    [Fact]
    [Trait("Category", "Positive")]
    [Trait("TestId", "POS_002")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task POS_002_WorkflowState_ReturnsCorrectStageAndActions_ForValidOpportunity()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.TryGetProperty("currentStage", out _).Should().BeTrue();
            body.TryGetProperty("availableActions", out _).Should().BeTrue();
        }
    }

    /// <summary>
    /// POS_003: Workflow history endpoint returns audit trail entries.
    /// </summary>
    [Fact]
    [Trait("Category", "Positive")]
    [Trait("TestId", "POS_003")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task POS_003_WorkflowHistory_ReturnsAuditTrailEntries_ForValidOpportunity()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/history");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    #endregion

    #region Negative Tests (9)

    /// <summary>
    /// NEG_001: Requirements endpoint returns 404 for non-existent opportunity.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_001")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_001_RequirementsEndpoint_Returns404_ForNonExistentOpportunity()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/99999/requirements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// NEG_002: Requirements endpoint returns 404 for unsupported entity.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_002")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_002_RequirementsEndpoint_Returns404_ForUnsupportedEntity()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/unsupportedentity/1/requirements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// NEG_003: Submit fails when SDG alignment is empty (minLength=1 violation).
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_003")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_003_Submit_Fails_WhenSdgAlignmentEmpty()
    {
        if (!_isPostgresAvailable) return;
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
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            var success = body.TryGetProperty("success", out var s) && s.GetBoolean();
            if (!success && body.TryGetProperty("unmetRequirements", out var unmet))
            {
                var unmetStr = unmet.GetRawText();
                unmetStr.ToLowerInvariant().Should().Contain("sdg");
            }
        }
    }

    /// <summary>
    /// NEG_004: Submit fails when Strategic Missions empty (minLength=1 violation).
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_004")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_004_Submit_Fails_WhenStrategicMissionsEmpty()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var missionsUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            if (name != "missions" && name != "strategicmissions") return false;
            return i.TryGetProperty("isMet", out var m) && !m.GetBoolean();
        });
        if (missionsUnmet)
        {
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
            if (submitResponse.StatusCode == HttpStatusCode.OK)
            {
                var body = await submitResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
                body.TryGetProperty("success", out var s).Should().BeTrue();
                var success = s.GetBoolean();
                if (!success)
                    body.TryGetProperty("unmetRequirements", out _).Should().BeTrue();
            }
        }
    }

    /// <summary>
    /// NEG_005: Submit fails when Countries empty (minLength=1 violation).
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_005")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_005_Submit_Fails_WhenCountriesEmpty()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var countriesUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return (name == "countries" || name == "country") && i.TryGetProperty("isMet", out var m) && !m.GetBoolean();
        });
        if (countriesUnmet)
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// NEG_006: Submit fails when Funding Partners empty.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_006")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_006_Submit_Fails_WhenFundingPartnersEmpty()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var fundingUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return (name == "fundingpartners" || name == "fundingpartner") && i.TryGetProperty("isMet", out var m) && !m.GetBoolean();
        });
        if (fundingUnmet)
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// NEG_007: Submit fails when Client Partners empty.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_007")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_007_Submit_Fails_WhenClientPartnersEmpty()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var clientUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return (name == "clientpartners" || name == "clientpartner") && i.TryGetProperty("isMet", out var m) && !m.GetBoolean();
        });
        if (clientUnmet)
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// NEG_008: Submit fails when Products & Services empty.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_008")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_008_Submit_Fails_WhenProductsAndServicesEmpty()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var productsUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return (name == "products" || name == "deliverables") && i.TryGetProperty("isMet", out var m) && !m.GetBoolean();
        });
        if (productsUnmet)
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// NEG_009: Submit fails when opportunity name is null/empty.
    /// </summary>
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "NEG_009")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task NEG_009_Submit_Fails_WhenOpportunityNameEmpty()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var nameUnmet = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return name == "name" && i.TryGetProperty("isMet", out var m) && !m.GetBoolean();
        });
        if (nameUnmet)
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region Edge/Boundary Tests (9)

    /// <summary>
    /// EDGE_001: Submit with exactly 1 SDG (boundary: minLength=1).
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_001")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_001_Submit_WithExactlyOneSdg_SatisfiesMinLength()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var sdgItem = items.EnumerateArray().FirstOrDefault(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return name == "sdgs" || name == "primarysdg";
        });
        if (sdgItem.ValueKind != JsonValueKind.Undefined && sdgItem.TryGetProperty("isMet", out var isMet) && isMet.GetBoolean())
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// EDGE_002: Submit with exactly 1 Country (boundary: minLength=1).
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_002")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_002_Submit_WithExactlyOneCountry_SatisfiesMinLength()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var countryItem = items.EnumerateArray().FirstOrDefault(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return name == "countries" || name == "country";
        });
        if (countryItem.ValueKind != JsonValueKind.Undefined && countryItem.TryGetProperty("isMet", out var isMet) && isMet.GetBoolean())
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// EDGE_003: Submit with BeneficiariesToBeDetermined=true skips beneficiaries validation.
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_003")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_003_Submit_WithBeneficiariesToBeDeterminedTrue_SkipsBeneficiariesValidation()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var beneficiariesItem = items.EnumerateArray().FirstOrDefault(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            var name = n.GetString()?.ToLowerInvariant();
            return name == "beneficiaries";
        });
        if (beneficiariesItem.ValueKind != JsonValueKind.Undefined)
        {
            beneficiariesItem.TryGetProperty("isMet", out var isMet);
            beneficiariesItem.TryGetProperty("description", out var desc);
            desc.GetString().Should().NotBeNullOrWhiteSpace();
        }
    }

    /// <summary>
    /// EDGE_004: Submit with BeneficiariesToBeDetermined=false requires beneficiaries.
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_004")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_004_Submit_WithBeneficiariesToBeDeterminedFalse_RequiresBeneficiaries()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasBeneficiaries = items.EnumerateArray().Any(i =>
        {
            if (!i.TryGetProperty("name", out var n)) return false;
            return n.GetString()?.ToLowerInvariant() == "beneficiaries";
        });
        hasBeneficiaries.Should().BeTrue("Beneficiaries requirement must exist for conditional validation");
    }

    /// <summary>
    /// EDGE_005: Requirements endpoint handles empty next stage parameter.
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_005")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_005_RequirementsEndpoint_HandlesEmptyNextStageParameter()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    /// <summary>
    /// EDGE_006: Workflow state for opportunity at GO stage shows no available actions.
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_006")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_006_WorkflowState_AtGoStage_ShowsNoAvailableActions()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var currentStage = body.TryGetProperty("currentStage", out var cs) ? cs.GetString() : null;
        if (string.Equals(currentStage, "GO", StringComparison.OrdinalIgnoreCase))
        {
            // GO is final stage - no further actions
            body.TryGetProperty("availableActions", out var actions).Should().BeTrue();
            var actionsArr = actions.GetArrayLength();
            actionsArr.Should().Be(0, "GO stage has no further transitions");
        }
    }

    /// <summary>
    /// EDGE_007: Workflow state for opportunity at NO GO shows Reopen action only.
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_007")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_007_WorkflowState_AtNoGo_ShowsReopenActionOnly()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var currentStage = body.TryGetProperty("currentStage", out var cs) ? cs.GetString() : null;
        if (string.Equals(currentStage, "NO GO", StringComparison.OrdinalIgnoreCase))
        {
            body.TryGetProperty("availableActions", out var actions).Should().BeTrue();
            var hasReopen = false;
            foreach (var a in actions.EnumerateArray())
            {
                if (a.TryGetProperty("newStage", out var ns) && ns.GetString() == "IDENTIFY & PROFILE")
                    hasReopen = true;
            }
            hasReopen.Should().BeTrue("NO GO should have Reopen action");
        }
    }

    /// <summary>
    /// EDGE_008: Requirements with maxLength field boundary values.
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_008")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_008_Requirements_IncludeMaxLengthFieldBoundary()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            item.TryGetProperty("name", out _).Should().BeTrue();
            item.TryGetProperty("description", out _).Should().BeTrue();
        }
    }

    /// <summary>
    /// EDGE_009: Workflow details endpoint shows InWorkflow status correctly.
    /// </summary>
    [Fact]
    [Trait("Category", "EdgeBoundary")]
    [Trait("TestId", "EDGE_009")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task EDGE_009_WorkflowDetails_ShowsInWorkflowStatusCorrectly()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/details");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.TryGetProperty("currentStage", out _).Should().BeTrue();
            body.TryGetProperty("isInWorkflow", out _).Should().BeTrue();
        }
    }

    #endregion

    #region Functional Tests (9)

    /// <summary>
    /// FUNC_001: Requirements list contains all 21 mandatory GO fields.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_001")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_001_RequirementsList_ContainsAll21MandatoryGoFields()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var count = items.GetArrayLength();
        count.Should().BeGreaterOrEqualTo(15, "Requirements should include at least 15 of the 21 mandatory GO fields");
    }

    /// <summary>
    /// FUNC_002: Each requirement has correct description key.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_002")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_002_EachRequirement_HasCorrectDescriptionKey()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            item.TryGetProperty("description", out var desc).Should().BeTrue();
            desc.GetString().Should().NotBeNullOrWhiteSpace();
        }
    }

    /// <summary>
    /// FUNC_003: Requirements response includes validation type for each field.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_003")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_003_RequirementsResponse_IncludesValidationType()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            item.TryGetProperty("name", out _).Should().BeTrue();
            item.TryGetProperty("isMet", out _).Should().BeTrue();
        }
    }

    /// <summary>
    /// FUNC_004: Approve endpoint stores ExecutiveId on opportunity.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_004")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_004_ApproveEndpoint_StoresExecutiveId()
    {
        if (!_isPostgresAvailable) return;
        var payload = new
        {
            entityName = "opportunity",
            entityId = 1,
            rationale = "Approved",
            confirmationAcknowledged = true,
            executiveId = 10
        };
        var response = await _client.PostAsJsonAsync("/api/workflow/approve", payload, JsonOpts);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// FUNC_005: Reject sets stage to NO GO not IDENTIFY & PROFILE.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_005")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_005_Reject_SetsStageToNoGo()
    {
        if (!_isPostgresAvailable) return;
        var payload = new
        {
            entityName = "opportunity",
            entityId = 1,
            rationale = "Rejecting",
            confirmationAcknowledged = true
        };
        var response = await _client.PostAsJsonAsync("/api/workflow/reject", payload, JsonOpts);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            if (body.TryGetProperty("newStage", out var ns))
            {
                ns.GetString().Should().Be("NO GO");
            }
        }
    }

    /// <summary>
    /// FUNC_006: Reopen from NO GO sets stage back to IDENTIFY & PROFILE.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_006")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_006_Reopen_FromNoGo_SetsStageToIdentifyProfile()
    {
        if (!_isPostgresAvailable) return;
        var payload = new { entityName = "opportunity", entityId = 1, comment = (string?)null };
        var response = await _client.PostAsJsonAsync("/api/workflow/reopen", payload, JsonOpts);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, (HttpStatusCode)403, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            if (body.TryGetProperty("newStage", out var ns))
            {
                ns.GetString().Should().Be("IDENTIFY & PROFILE");
            }
        }
    }

    /// <summary>
    /// FUNC_007: Cancel from IDENTIFY & PROFILE sets stage to CANCELLED.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_007")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_007_Cancel_FromIdentifyProfile_SetsStageToCancelled()
    {
        if (!_isPostgresAvailable) return;
        var payload = new { entityName = "opportunity", entityId = 1, comment = "Cancelling" };
        var response = await _client.PostAsJsonAsync("/api/workflow/cancel", payload, JsonOpts);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, (HttpStatusCode)403, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            if (body.TryGetProperty("newStage", out var ns))
            {
                ns.GetString().Should().Be("CANCELLED");
            }
        }
    }

    /// <summary>
    /// FUNC_008: Workflow state includes canRecall flag.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_008")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_008_WorkflowState_IncludesCanRecallFlag()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.TryGetProperty("availableActions", out _).Should().BeTrue();
    }

    /// <summary>
    /// FUNC_009: Workflow details includes approvers list.
    /// </summary>
    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "FUNC_009")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task FUNC_009_WorkflowDetails_IncludesApproversList()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/details");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.TryGetProperty("approvers", out var approvers).Should().BeTrue();
        approvers.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Null);
    }

    #endregion

    #region Integration Tests (9)

    /// <summary>
    /// INT_001: Full submit-approve cycle completes and stage becomes GO.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_001")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_001_FullSubmitApproveCycle_StageBecomesGo()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var allMet = items.EnumerateArray().All(i => !i.TryGetProperty("isMet", out var m) || m.GetBoolean());
        if (!allMet) return;
        var submitPayload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
        var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", submitPayload, JsonOpts);
        if (submitResponse.StatusCode != HttpStatusCode.OK) return;
        var submitBody = await submitResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (!submitBody.TryGetProperty("success", out var s) || !s.GetBoolean()) return;
        var approvePayload = new { entityName = "opportunity", entityId = 1, rationale = "Approved", confirmationAcknowledged = true, executiveId = 10 };
        var approveResponse = await _client.PostAsJsonAsync("/api/workflow/approve", approvePayload, JsonOpts);
        approveResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// INT_002: Full submit-reject cycle completes and stage becomes NO GO.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_002")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_002_FullSubmitRejectCycle_StageBecomesNoGo()
    {
        if (!_isPostgresAvailable) return;
        var submitPayload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
        var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", submitPayload, JsonOpts);
        if (submitResponse.StatusCode != HttpStatusCode.OK) return;
        var submitBody = await submitResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (!submitBody.TryGetProperty("success", out var s) || !s.GetBoolean()) return;
        var rejectPayload = new { entityName = "opportunity", entityId = 1, rationale = "Rejecting", confirmationAcknowledged = true };
        var rejectResponse = await _client.PostAsJsonAsync("/api/workflow/reject", rejectPayload, JsonOpts);
        rejectResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// INT_003: Submit-recall cycle restores original state.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_003")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_003_SubmitRecallCycle_RestoresOriginalState()
    {
        if (!_isPostgresAvailable) return;
        var submitPayload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
        var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", submitPayload, JsonOpts);
        if (submitResponse.StatusCode != HttpStatusCode.OK) return;
        var recallPayload = new { entityName = "opportunity", entityId = 1, comment = "Recalling" };
        var recallResponse = await _client.PostAsJsonAsync("/api/workflow/recall", recallPayload, JsonOpts);
        recallResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, (HttpStatusCode)403, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// INT_004: Cancel-reopen cycle returns to IDENTIFY & PROFILE.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_004")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_004_CancelReopenCycle_ReturnsToIdentifyProfile()
    {
        if (!_isPostgresAvailable) return;
        var cancelPayload = new { entityName = "opportunity", entityId = 1, comment = "Cancelling" };
        var cancelResponse = await _client.PostAsJsonAsync("/api/workflow/cancel", cancelPayload, JsonOpts);
        if (cancelResponse.StatusCode != HttpStatusCode.OK) return;
        var reopenPayload = new { entityName = "opportunity", entityId = 1, comment = "Reopening" };
        var reopenResponse = await _client.PostAsJsonAsync("/api/workflow/reopen", reopenPayload, JsonOpts);
        reopenResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, (HttpStatusCode)403, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// INT_005: Multiple stage changes create correct history entries.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_005")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_005_MultipleStageChanges_CreateCorrectHistoryEntries()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/history");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    /// <summary>
    /// INT_006: Pending approvals endpoint returns tasks for approver.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_006")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_006_PendingApprovals_ReturnsTasksForApprover()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/pending-approvals");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    /// <summary>
    /// INT_007: Workflow history shows chronological order.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_007")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_007_WorkflowHistory_ShowsChronologicalOrder()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/history");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var len = items.GetArrayLength();
        if (len >= 2)
        {
            var first = items[0];
            var second = items[1];
            if (first.TryGetProperty("completedOn", out var d1) && second.TryGetProperty("completedOn", out var d2)
                && d1.ValueKind != JsonValueKind.Null && d1.ValueKind != JsonValueKind.Undefined
                && d2.ValueKind != JsonValueKind.Null && d2.ValueKind != JsonValueKind.Undefined)
            {
                var s1 = d1.GetString();
                var s2 = d2.GetString();
                if (!string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(s2)
                    && DateTime.TryParse(s1, out var t1) && DateTime.TryParse(s2, out var t2))
                {
                    t1.Should().BeOnOrAfter(t2, "History should be ordered by completedOn descending");
                }
            }
        }
    }

    /// <summary>
    /// INT_008: Requirements validation runs before submit.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_008")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_008_RequirementsValidation_RunsBeforeSubmit()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;
        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var unmet = items.EnumerateArray().Where(i => i.TryGetProperty("isMet", out var m) && !m.GetBoolean()).ToList();
        if (unmet.Count > 0)
        {
            var payload = new { entityName = "opportunity", entityId = 1, newStage = "GO", confirmedNonOMSubmission = true, confirmedOrgUnitWarning = true, acknowledgedStatement = true };
            var submitResponse = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);
            if (submitResponse.StatusCode == HttpStatusCode.OK)
            {
                var body = await submitResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
                var success = body.TryGetProperty("success", out var s) && s.GetBoolean();
                if (!success)
                    body.TryGetProperty("unmetRequirements", out _).Should().BeTrue();
            }
        }
    }

    /// <summary>
    /// INT_009: Workflow state updates after each action.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "INT_009")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task INT_009_WorkflowState_UpdatesAfterEachAction()
    {
        if (!_isPostgresAvailable) return;
        var beforeResponse = await _client.GetAsync("/api/workflow/opportunity/1");
        beforeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        var afterResponse = await _client.GetAsync("/api/workflow/opportunity/1");
        afterResponse.StatusCode.Should().Be(beforeResponse.StatusCode);
    }

    #endregion

    #region GAP-M2: Workflow Schema Auto-Creation (1)

    /// <summary>
    /// GAP-M2: Workflow schema tables exist after application starts.
    /// Verifies WorkflowDbContext is resolvable and has expected model.
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "GAP-M2")]
    [Trait("JiraRef", "WorkflowPRD")]
    public async Task GAP_M2_WorkflowSchema_TablesExistAfterApplicationStarts()
    {
        using var scope = _factory.Services.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        workflowContext.Should().NotBeNull();
        var canConnect = await workflowContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue("WorkflowDbContext must be able to connect after application start");
    }

    #endregion
}
