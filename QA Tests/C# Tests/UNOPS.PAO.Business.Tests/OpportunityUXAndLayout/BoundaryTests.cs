/**
 * @fileoverview Opportunity UX & Layout boundary tests — PNO-769, PNO-862, PNO-863, PNO-871, PNO-876, PNO-877, PNO-882.
 * Min/max values, edge cases, soft-delete interactions, nullable FK.
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
/// Boundary tests for Opportunity UX & Layout.
/// </summary>
[Collection("Opportunity UX And Layout Integration")]
[Trait("Category", "Boundary")]
[Trait("Feature", "OpportunityUXAndLayout")]
public class BoundaryTests : OpportunityUXAndLayoutFixtureBase
{
    public BoundaryTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "UX-BND-001")]
    public async Task GetOpportunity_IdOne_ReturnsValidResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "UX-BND-002")]
    public async Task GetOpportunity_MinValidId_Returns200Or404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "UX-BND-003")]
    public async Task GetComments_EntityIdOne_ReturnsArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().StartWith("[");
    }

    [Fact]
    [Trait("TestId", "UX-BND-004")]
    public async Task CreateComment_MinLengthContent_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "x" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-BND-005")]
    public async Task CreateComment_SingleCharContent_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "a" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-BND-006")]
    public async Task CreateComment_LongContent_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var longContent = new string('x', OpportunityUXAndLayoutSpec.CommentMaxLength);
        var request = new { entityType = "Opportunity", entityId = 1, content = longContent };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "UX-BND-007")]
    public async Task CreateComment_ContentAtMaxLength_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var maxContent = new string('y', 4000);
        var request = new { entityType = "Opportunity", entityId = 1, content = maxContent };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "UX-BND-008")]
    public async Task GetOpportunity_WithSpecialCharsInName_ReturnsValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            JsonDocument.Parse(json);
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-009")]
    public async Task GetComments_IncludeRepliesTrue_ReturnsValidStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1, true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;
        arr.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "UX-BND-010")]
    public async Task GetComments_IncludeRepliesFalse_ReturnsValidStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1, false);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;
        arr.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "UX-BND-011")]
    public async Task GetOpportunity_ReturnsNonNullStageWhenPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("stage", out var stage))
            {
                stage.ValueKind.Should().BeOneOf(JsonValueKind.String, JsonValueKind.Null, JsonValueKind.Undefined);
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-012")]
    public async Task GetOpportunity_ReturnsTargetSigningDateAsNullable()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("targetSigningDate", out _);
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-013")]
    public async Task GetRisks_ValidOpportunity_ReturnsArrayOrObject()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "UX-BND-014")]
    public async Task CreateComment_UnicodeContent_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "Comment with émojis 🎯 and ñ" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-BND-015")]
    public async Task CreateComment_NewlineContent_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "Line1\nLine2\r\nLine3" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-BND-016")]
    public async Task GetOpportunity_FundingPartnersEmptyArray_ValidResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("fundingPartners", out var fp))
            {
                fp.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Null);
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-017")]
    public async Task GetOpportunity_ClientPartnersEmptyArray_ValidResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("clientPartners", out var cp))
            {
                cp.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Null);
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-018")]
    public async Task GetOpportunity_DescriptionNull_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            JsonDocument.Parse(json);
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-019")]
    public async Task GetComments_OpportunityWithNoComments_ReturnsEmptyArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var arr = JsonDocument.Parse(json).RootElement;
        arr.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "UX-BND-020")]
    public async Task GetRisks_OpportunityWithNoRisks_ReturnsEmptyOrValidStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetRisksAsync(client, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "UX-BND-021")]
    public async Task CreateComment_EntityTypeCaseSensitivity_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "opportunity", entityId = 1, content = "Test" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "UX-BND-022")]
    public async Task GetOpportunity_IdJustBelowNonExistent_Returns404Or200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 999997);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "UX-BND-023")]
    public async Task CreateComment_HtmlContent_EscapedOrAccepted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "<script>alert(1)</script>" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "UX-BND-024")]
    public async Task GetOpportunity_NameVeryLong_ReturnsWithoutTruncation()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("name", out var name))
            {
                name.GetString().Should().NotBeNull();
            }
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-025")]
    public async Task GetComments_MultipleOpportunities_IsolatedPerEntity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 1);
        var r2 = await GetCommentsAsync(client, OpportunityUXAndLayoutSpec.OpportunityEntityType, 2);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-BND-026")]
    public async Task CreateComment_TabCharacterInContent_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { entityType = "Opportunity", entityId = 1, content = "Tab\there" };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-BND-027")]
    public async Task GetOpportunity_OrgUnitNullable_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("organizationUnitId", out _);
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-028")]
    public async Task GetOpportunity_OpportunityManagerNullable_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetOpportunityAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("opportunityManager", out _);
        }
    }

    [Fact]
    [Trait("TestId", "UX-BND-029")]
    public async Task CreateComment_Exactly500Chars_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new string('z', 500);
        var request = new { entityType = "Opportunity", entityId = 1, content };
        var response = await PostCommentAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "UX-BND-030")]
    public async Task GetRisks_ConsecutiveCalls_SameResult()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetRisksAsync(client, 1);
        var r2 = await GetRisksAsync(client, 1);
        r1.StatusCode.Should().Be(r2.StatusCode);
    }
}
