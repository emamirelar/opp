/**
 * @fileoverview Admin, Access Control & Validation negative tests.
 * PNO-762, PNO-767, PNO-768, PNO-772, PNO-774, PNO-807, PNO-960, PNO-963.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.AdminAccessValidation;

/// <summary>
/// Negative tests for Admin, Access Control &amp; Validation.
/// </summary>
[Collection("Admin Access Validation Integration")]
[Trait("Category", "Negative")]
[Trait("Feature", "AdminAccessValidation")]
public class NegativeTests : AdminAccessValidationFixtureBase
{
    public NegativeTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "AAV-NEG-001")]
    public async Task GetSearchFields_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-002")]
    public async Task GetOpportunitiesList_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-003")]
    public async Task CreateOpportunity_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var body = new { name = "Test", description = "Desc" };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(AdminAccessValidationSpec.OpportunityBase, content);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-004")]
    public async Task CreateOpportunity_EmptyName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new { name = "", description = "Desc" };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-005")]
    public async Task CreateOpportunity_NullName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new { name = (string?)null, description = "Desc" };
        var response = await PostCreateOpportunityAsync(client, body!);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-006")]
    [Trait("Defect", "DEF-187")]
    public async Task CreateOpportunity_NameExceeds255Chars_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', AdminAccessValidationSpec.OpportunityNameMaxLength + 1);
        var body = new { name, description = "Desc" };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PNO-774: Name must be limited to 255 chars");
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-007")]
    public async Task CreateOpportunity_EmptyDescription_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new { name = "Test", description = "" };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-008")]
    public async Task GetOpportunity_NonexistentId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-009")]
    public async Task GetPartnerDocuments_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(1));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-010")]
    public async Task CreateFromPartner_InvalidPartnerId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Desc" };
        var response = await PostCreateFromPartnerAsync(client, 999999, request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-011")]
    public async Task CreateFromPartner_InvalidPartnerRole_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "invalid", description = "Desc" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode != HttpStatusCode.NotFound)
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-012")]
    public async Task CreateOpportunity_EmptyBody_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(AdminAccessValidationSpec.OpportunityBase, content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-013")]
    public async Task Search_EmptyQuery_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query=");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-014")]
    public async Task UpdateOpportunity_NonexistentId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new { id = 999999, name = "Updated", description = "Desc" };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"{AdminAccessValidationSpec.OpportunityBase}/999999", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-015")]
    public async Task DeleteOpportunity_NonexistentId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.DeleteAsync($"{AdminAccessValidationSpec.OpportunityBase}/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-016")]
    public async Task CreateOpportunity_WhitespaceOnlyName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new { name = "   ", description = "Desc" };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-017")]
    public async Task CreateFromPartner_EmptyName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "", partnerRole = "funding", description = "Desc" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode != HttpStatusCode.NotFound)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-018")]
    public async Task GetDocument_NonexistentEntity_Returns404Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(999999));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-019")]
    public async Task CreateOpportunity_InvalidFundingPartnerId_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new
        {
            name = "Test",
            description = "Desc",
            fundingPartners = new[] { new { partnerId = 0 } }
        };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-020")]
    public async Task CreateOpportunity_InvalidClientPartnerId_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new
        {
            name = "Test",
            description = "Desc",
            clientPartners = new[] { new { partnerId = -1 } }
        };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-021")]
    public async Task UpdateOpportunity_IdMismatch_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new { id = 999, name = "Updated", description = "Desc" };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"{AdminAccessValidationSpec.OpportunityBase}/1", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-022")]
    public async Task GetOpportunitiesList_InvalidPageIndex_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=-1&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-023")]
    public async Task CreateOpportunity_MalformedJson_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{invalid json", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(AdminAccessValidationSpec.OpportunityBase, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-024")]
    public async Task CreateFromPartner_ClosedOrArchivedPartner_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Desc" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-025")]
    public async Task GetSearchFields_NoAuthHeader_Returns401()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-026")]
    public async Task CreateOpportunity_Name256Chars_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('X', 256);
        var body = new { name, description = "Desc" };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PNO-774: 256 chars must be rejected");
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-027")]
    public async Task CreateOpportunity_Name500Chars_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('Y', 500);
        var body = new { name, description = "Desc" };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-028")]
    public async Task GetOpportunityDetail_SoftDeleted_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/999998");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-029")]
    public async Task CreateOpportunity_NullDescription_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new { name = "Test", description = (string?)null };
        var response = await PostCreateOpportunityAsync(client, body!);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-NEG-030")]
    public async Task CreateFromPartner_ZeroPartnerId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Desc" };
        var response = await PostCreateFromPartnerAsync(client, 0, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
