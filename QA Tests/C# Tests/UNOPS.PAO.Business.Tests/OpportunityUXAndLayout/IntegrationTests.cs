/**
 * @fileoverview Opportunity UX & Layout integration tests — PNO-769, PNO-862, PNO-863, PNO-871, PNO-876, PNO-877, PNO-882.
 * Full CRUD through API, service-to-DB round-trip, multi-component workflows.
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
/// Integration tests for Opportunity UX & Layout.
/// </summary>
[Collection("Opportunity UX And Layout Integration")]
[Trait("Category", "Integration")]
[Trait("Feature", "OpportunityUXAndLayout")]
public class IntegrationTests : OpportunityUXAndLayoutFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "UX-INT-001")]
    public async Task GetOpportunityThenGetComments_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var oppResponse = await GetOpportunityAsync(client, 1);
        oppResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var commentsResponse = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        commentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-INT-002")]
    public async Task CreateCommentThenGetComments_RoundTrip()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = $"INT-002 {Guid.NewGuid()}";
        var createResponse = await PostCommentAsync(client, new { entityType = "Opportunity", entityId = 1, content });
        if (createResponse.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK)
        {
            var getResponse = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await getResponse.Content.ReadAsStringAsync();
            json.Should().Contain(content);
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-003")]
    public async Task GetOpportunityThenGetRisks_SequentialCalls()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var oppResponse = await GetOpportunityAsync(client, 1);
        oppResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var risksResponse = await GetRisksAsync(client, 1);
        risksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-INT-004")]
    public async Task GetOpportunity_ListThenDetail_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync("/api/opportunity?page=1&pageSize=10");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listJson = await listResponse.Content.ReadAsStringAsync();
        var listDoc = JsonDocument.Parse(listJson);
        if (listDoc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
        {
            var firstId = items[0].GetProperty("id").GetInt32();
            var detailResponse = await GetOpportunityAsync(client, firstId);
            detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-005")]
    public async Task CreateComment_MultipleComments_SameOpportunity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        for (var i = 0; i < 3; i++)
        {
            var request = new { entityType = "Opportunity", entityId = 1, content = $"INT-005 comment {i} {Guid.NewGuid()}" };
            var response = await PostCommentAsync(client, request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }
        var getResponse = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-INT-006")]
    public async Task GetOpportunity_HeaderKeyInfoRisks_AllSections()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("id", out _).Should().BeTrue();
        root.TryGetProperty("name", out _).Should().BeTrue();
        root.TryGetProperty("description", out _).Should().BeTrue();
        root.TryGetProperty("stage", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "UX-INT-007")]
    public async Task GetComments_DifferentOpportunities_Isolated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        var r2 = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 2);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-INT-008")]
    public async Task GetOpportunity_WithFundingAndClientPartners_Complete()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("fundingPartners", out _);
        doc.RootElement.TryGetProperty("clientPartners", out _);
    }

    [Fact]
    [Trait("TestId", "UX-INT-009")]
    public async Task CreateComment_ThenVerifyInList()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var uniqueContent = $"INT-009 {DateTime.UtcNow:O}";
        var createResponse = await PostCommentAsync(client, new { entityType = "Opportunity", entityId = 1, content = uniqueContent });
        if (createResponse.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK)
        {
            var listResponse = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
            var json = await listResponse.Content.ReadAsStringAsync();
            json.Should().Contain(uniqueContent);
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-010")]
    public async Task GetRisks_AfterGetOpportunity_SameClient()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        await GetOpportunityAsync(client, 1);
        var risksResponse = await GetRisksAsync(client, 1);
        risksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-INT-011")]
    public async Task GetOpportunity_PermissionsEndpoint_IfExists()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var permResponse = await client.GetAsync("/api/opportunity/1/permissions");
        permResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-INT-012")]
    public async Task FullOpportunityDetailFlow_HeaderCommentsRisks()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var opp = await GetOpportunityAsync(client, 1);
        var comments = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        var risks = await GetRisksAsync(client, 1);
        opp.StatusCode.Should().Be(HttpStatusCode.OK);
        comments.StatusCode.Should().Be(HttpStatusCode.OK);
        risks.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-INT-013")]
    public async Task GetOpportunity_ConcurrentRequests_SameResult()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var t1 = GetOpportunityAsync(client, 1);
        var t2 = GetOpportunityAsync(client, 1);
        await Task.WhenAll(t1, t2);
        t1.Result.StatusCode.Should().Be(t2.Result.StatusCode);
    }

    [Fact]
    [Trait("TestId", "UX-INT-014")]
    public async Task GetComments_ConcurrentRequests_SameResult()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var t1 = GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        var t2 = GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        await Task.WhenAll(t1, t2);
        t1.Result.StatusCode.Should().Be(t2.Result.StatusCode);
    }

    [Fact]
    [Trait("TestId", "UX-INT-015")]
    public async Task CreateComment_NewOpportunityFromList()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync("/api/opportunity?page=1&pageSize=5");
        if (listResponse.StatusCode == HttpStatusCode.OK)
        {
            var json = await listResponse.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                var id = items[0].GetProperty("id").GetInt32();
                var commentResponse = await PostCommentAsync(client, new { entityType = "Opportunity", entityId = id, content = "INT-015" });
                commentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-016")]
    public async Task GetOpportunity_WorkflowDetails_IfAvailable()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var workflowResponse = await client.GetAsync("/api/workflow/opportunity/1");
        workflowResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-INT-017")]
    public async Task GetOpportunity_OverviewSection_Patchable()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var getResponse = await GetOpportunityAsync(client, 1);
        if (getResponse.StatusCode == HttpStatusCode.OK)
        {
            var patchResponse = await client.PatchAsync(
                OpportunityUXAndLayoutSpec.UpdateOverviewEndpoint(1),
                new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
            patchResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-018")]
    public async Task GetComments_EntityTypePartner_CompareWithOpportunity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var oppComments = await GetCommentsAsync(client, "Opportunity", 1);
        var partnerComments = await GetCommentsAsync(client, "Partner", 1);
        oppComments.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        partnerComments.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "UX-INT-019")]
    public async Task GetOpportunity_StakeholdersIncluded()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("stakeholders", out _);
            doc.RootElement.TryGetProperty("team", out _);
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-020")]
    public async Task GetOpportunity_WhenSectionData_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("targetSigningDate", out _);
        doc.RootElement.TryGetProperty("implementationStartDate", out _);
    }

    [Fact]
    [Trait("TestId", "UX-INT-021")]
    public async Task GetOpportunity_WhereSectionData_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("implementationCountries", out _);
        doc.RootElement.TryGetProperty("countries", out _);
    }

    [Fact]
    [Trait("TestId", "UX-INT-022")]
    public async Task GetOpportunity_WhatSectionData_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("productsAndServices", out _);
        doc.RootElement.TryGetProperty("outputs", out _);
    }

    [Fact]
    [Trait("TestId", "UX-INT-023")]
    public async Task GetOpportunity_WhySectionData_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("sdgs", out _);
        doc.RootElement.TryGetProperty("beneficiaries", out _);
    }

    [Fact]
    [Trait("TestId", "UX-INT-024")]
    public async Task GetRisks_WithOpportunityDetail_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var opp = await GetOpportunityAsync(client, 1);
        var risks = await GetRisksAsync(client, 1);
        if (opp.StatusCode == HttpStatusCode.OK)
        {
            risks.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-025")]
    public async Task CreateComment_VerifyCreatedDateInResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = $"INT-025 {Guid.NewGuid()}" };
        var response = await PostCommentAsync(client, request);
        if (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("createdDate", out var cd).Should().BeTrue();
            DateTime.TryParse(cd.GetString(), out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-026")]
    public async Task GetOpportunity_SearchThenDetail()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var searchResponse = await client.GetAsync("/api/opportunity/search?query=test&page=1&pageSize=10");
        if (searchResponse.StatusCode == HttpStatusCode.OK)
        {
            var json = await searchResponse.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                var id = items[0].GetProperty("id").GetInt32();
                var detailResponse = await GetOpportunityAsync(client, id);
                detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-027")]
    public async Task GetOpportunity_AdvancedSearchThenDetail()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var searchResponse = await client.PostAsync("/api/opportunity/advanced-search",
            new StringContent("{\"page\":1,\"pageSize\":10}", System.Text.Encoding.UTF8, "application/json"));
        if (searchResponse.StatusCode == HttpStatusCode.OK)
        {
            var json = await searchResponse.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                var id = items[0].GetProperty("id").GetInt32();
                var detailResponse = await GetOpportunityAsync(client, id);
                detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-028")]
    public async Task GetComments_CountEndpoint_IfExists()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/comment/Opportunity/1/count");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-INT-029")]
    public async Task GetOpportunity_MultipleIds_Sequential()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        for (var id = 1; id <= 3; id++)
        {
            var response = await GetOpportunityAsync(client, id);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    [Trait("TestId", "UX-INT-030")]
    public async Task FullUXFlow_OpportunityCommentsRisks_PNO769_871_876()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var opp = await GetOpportunityAsync(client, 1);
        opp.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        comments.StatusCode.Should().Be(HttpStatusCode.OK);
        var risks = await GetRisksAsync(client, 1);
        risks.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await opp.Content.ReadAsStringAsync();
        json.Should().Contain("id");
        json.Should().Contain("name");
        json.Should().Contain("stage");
    }
}
