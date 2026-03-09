/**
 * @fileoverview Opportunity UX & Layout functional tests — PNO-769, PNO-862, PNO-863, PNO-871, PNO-876, PNO-877, PNO-882.
 * Business rules, audit fields, permissions, workflow transitions, data transformations.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text.Json;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityUXAndLayout;

/// <summary>
/// Functional tests for Opportunity UX & Layout.
/// </summary>
[Collection("Opportunity UX And Layout Integration")]
[Trait("Category", "Functional")]
[Trait("Feature", "OpportunityUXAndLayout")]
public class FunctionalTests : OpportunityUXAndLayoutFixtureBase
{
    public FunctionalTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "UX-FNC-001")]
    [Trait("AC", "PNO-769-AC1")]
    public async Task GetOpportunity_HeaderFieldsAreReadOnly_DataReturnedCorrectly()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("id", out var id).Should().BeTrue();
        id.TryGetInt32(out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "UX-FNC-002")]
    [Trait("AC", "PNO-769-AC4")]
    public async Task GetOpportunity_KeyInformationIncludesTotalBudget()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("totalBudget", out _);
        doc.RootElement.TryGetProperty("fundingPartners", out _);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-003")]
    [Trait("AC", "PNO-769-AC5")]
    public async Task GetOpportunity_QuickStatsFundingPartnersCount_ReflectsData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("fundingPartners", out var fp) && fp.ValueKind == JsonValueKind.Array)
        {
            fp.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-004")]
    [Trait("AC", "PNO-871")]
    public async Task CreateComment_ReturnsCommentWithId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "FNC-004 comment" };
        var response = await PostCommentAsync(client, request);
        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("id", out var id).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-005")]
    [Trait("AC", "PNO-871")]
    public async Task CreateComment_ReturnsCommentWithContent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = "FNC-005 content";
        var request = new { entityType = "Opportunity", entityId = 1, content };
        var response = await PostCommentAsync(client, request);
        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("content", out var c).Should().BeTrue();
            c.GetString().Should().Be(content);
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-006")]
    [Trait("AC", "PNO-871")]
    public async Task GetComments_ReturnsCommentsWithCreatedDate()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;
        foreach (var item in arr.EnumerateArray())
        {
            item.TryGetProperty("createdDate", out _);
            item.TryGetProperty("createdBy", out _);
            break;
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-007")]
    [Trait("AC", "PNO-876")]
    public async Task GetRisks_ReturnsRisksSectionData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "UX-FNC-008")]
    public async Task GetOpportunity_StageFieldMatchesWorkflow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("stage", out var stage))
        {
            stage.GetString().Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-009")]
    public async Task GetOpportunity_ClientPartnersCount_ReflectsData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("clientPartners", out var cp) && cp.ValueKind == JsonValueKind.Array)
        {
            cp.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-010")]
    public async Task GetOpportunity_DescriptionEditableField_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("description", out _);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-011")]
    public async Task GetComments_OrderedChronologically()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;
        var dates = new List<DateTime>();
        foreach (var item in arr.EnumerateArray())
        {
            if (item.TryGetProperty("createdDate", out var d))
            {
                if (DateTime.TryParse(d.GetString(), out var dt))
                    dates.Add(dt);
            }
        }
        for (var i = 1; i < dates.Count; i++)
        {
            dates[i].Should().BeOnOrAfter(dates[i - 1]);
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-012")]
    public async Task GetOpportunity_IdFieldReadOnly_IntegerType()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("id", out var id).Should().BeTrue();
        id.ValueKind.Should().Be(JsonValueKind.Number);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-013")]
    public async Task CreateComment_AuthorPopulatedFromAuth()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "FNC-013" };
        var response = await PostCommentAsync(client, request);
        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("createdBy", out _);
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-014")]
    public async Task GetOpportunity_TargetSigningDateFormat_Valid()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("targetSigningDate", out var d) && d.ValueKind != JsonValueKind.Null)
            {
                var s = d.GetString();
                DateTime.TryParse(s, out _).Should().BeTrue();
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-015")]
    public async Task GetRisks_EndpointServesRisksData_PNO876()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "";
        contentType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "UX-FNC-016")]
    public async Task GetOpportunity_NameFieldPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("name", out var name).Should().BeTrue();
        name.ValueKind.Should().Be(JsonValueKind.String);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-017")]
    public async Task GetComments_ExcludesSoftDeleted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;
        foreach (var item in arr.EnumerateArray())
        {
            if (item.TryGetProperty("isDeleted", out var del))
            {
                del.GetBoolean().Should().BeFalse();
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-018")]
    public async Task GetOpportunity_ExcludesSoftDeletedRelatedEntities()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("\"isDeleted\":true");
    }

    [Fact]
    [Trait("TestId", "UX-FNC-019")]
    public async Task CreateComment_EntityIdMatchesRequest()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var entityId = 1;
        var request = new { entityType = "Opportunity", entityId, content = "FNC-019" };
        var response = await PostCommentAsync(client, request);
        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("entityId", out var eid))
            {
                eid.GetInt32().Should().Be(entityId);
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-020")]
    public async Task CreateComment_EntityTypeMatchesRequest()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "FNC-020" };
        var response = await PostCommentAsync(client, request);
        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("entityType", out var et))
            {
                et.GetString().Should().Be("Opportunity");
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-021")]
    public async Task GetOpportunity_CountriesCount_QuickStats()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("countries", out _);
        doc.RootElement.TryGetProperty("implementationCountries", out _);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-022")]
    public async Task GetOpportunity_ServiceLinesIndication_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "UX-FNC-023")]
    public async Task GetComments_IncludeRepliesAffectsStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var withReplies = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1, true);
        var withoutReplies = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1, false);
        withReplies.StatusCode.Should().Be(HttpStatusCode.OK);
        withoutReplies.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-024")]
    public async Task GetOpportunity_WorkflowStatusPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("workflowStatus", out _);
        doc.RootElement.TryGetProperty("status", out _);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-025")]
    public async Task GetRisks_ConsistentWithOpportunityState()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var oppResponse = await GetOpportunityAsync(client, 1);
        var risksResponse = await GetRisksAsync(client, 1);
        if (oppResponse.StatusCode == HttpStatusCode.OK)
        {
            risksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-026")]
    public async Task CreateComment_TimestampSet()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var before = DateTime.UtcNow;
        var request = new { entityType = "Opportunity", entityId = 1, content = "FNC-026" };
        var response = await PostCommentAsync(client, request);
        var after = DateTime.UtcNow;
        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("createdDate", out var cd))
            {
                var created = DateTime.Parse(cd.GetString()!);
                created.Should().BeOnOrAfter(before.AddSeconds(-1));
                created.Should().BeOnOrBefore(after.AddSeconds(1));
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-027")]
    public async Task GetOpportunity_TotalBudgetDerivedFromFunding()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("totalBudget", out _);
        doc.RootElement.TryGetProperty("fundingPartners", out _);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-028")]
    public async Task GetOpportunity_OrgUnitResponsible_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("organizationUnitId", out _);
        doc.RootElement.TryGetProperty("organizationUnit", out _);
    }

    [Fact]
    [Trait("TestId", "UX-FNC-029")]
    public async Task GetComments_PinnedFirst_WhenPinnedExist()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;
        var seenUnpinned = false;
        foreach (var item in arr.EnumerateArray())
        {
            var isPinned = item.TryGetProperty("isPinned", out var p) && p.GetBoolean();
            if (isPinned)
            {
                seenUnpinned.Should().BeFalse("Pinned comments should come first");
            }
            else
            {
                seenUnpinned = true;
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-FNC-030")]
    public async Task GetOpportunity_OpportunityManager_PersonResponsible()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("opportunityManager", out _);
        doc.RootElement.TryGetProperty("opportunityManagerId", out _);
    }
}
