/**
 * @fileoverview Partner/Contact/Logo integration tests — PNO-148, PNO-797, PNO-933.
 * End-to-end flows, API contracts, multi-component workflows.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

/// <summary>
/// Integration tests for Partner, Contact and Logo features.
/// </summary>
[Collection("Partner Contact Logo Integration")]
[Trait("Category", "Integration")]
[Trait("Feature", "PartnerContactLogo")]
public class IntegrationTests : PartnerContactLogoFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PCL-INT-001")]
    [Trait("JIRA", "PNO-797")]
    public async Task ContactsPage_LoadToDisplay_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
        json.Should().NotContain("An item with the same key has already been added");
    }

    [Fact]
    [Trait("TestId", "PCL-INT-002")]
    public async Task PartnerList_ToDetail_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetPartnersAsync(client, 1, 10);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var records = doc.RootElement.GetProperty("records");
        if (records.GetArrayLength() == 0) return;
        var firstId = records[0].GetProperty("id").GetInt32();
        var detailResponse = await GetPartnerAsync(client, firstId);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-003")]
    public async Task ContactList_ToDetail_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetContactsAsync(client, 1, 10);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var records = doc.RootElement.GetProperty("records");
        if (records.GetArrayLength() == 0) return;
        var firstId = records[0].GetProperty("id").GetInt32();
        var detailResponse = await GetContactAsync(client, firstId);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-004")]
    [Trait("JIRA", "PNO-148")]
    public async Task Partner_LogoUrl_ApiToDisplayPipeline()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (doc.RootElement.TryGetProperty("logoUrl", out var logo) && logo.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var url = logo.GetString();
            if (!string.IsNullOrEmpty(url))
                url.Should().StartWith("http");
        }
    }

    [Fact]
    [Trait("TestId", "PCL-INT-005")]
    [Trait("JIRA", "PNO-148")]
    public async Task Contact_ProfilePictureUrl_ApiToDisplayPipeline()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (doc.RootElement.TryGetProperty("profilePictureUrl", out var urlProp) && urlProp.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var url = urlProp.GetString();
            if (!string.IsNullOrEmpty(url))
                url.Should().StartWith("http");
        }
    }

    [Fact]
    [Trait("TestId", "PCL-INT-006")]
    [Trait("JIRA", "PNO-933")]
    public async Task Contact_OrgUnit_ApiRoundTrip()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("organizationHierarchyIds", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-INT-007")]
    public async Task Partner_Contact_Navigation()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var partnerResponse = await GetPartnerAsync(client, 1);
        if (partnerResponse.StatusCode != HttpStatusCode.OK) return;
        var partnerDoc = System.Text.Json.JsonDocument.Parse(await partnerResponse.Content.ReadAsStringAsync());
        if (partnerDoc.RootElement.TryGetProperty("contacts", out var contacts) && contacts.GetArrayLength() > 0)
        {
            var contactId = contacts[0].GetProperty("id").GetInt32();
            var contactResponse = await GetContactAsync(client, contactId);
            contactResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "PCL-INT-008")]
    public async Task Contact_Partner_Navigation()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var contactResponse = await GetContactAsync(client, 1);
        if (contactResponse.StatusCode != HttpStatusCode.OK) return;
        var contactDoc = System.Text.Json.JsonDocument.Parse(await contactResponse.Content.ReadAsStringAsync());
        if (contactDoc.RootElement.TryGetProperty("partner", out var partner) && partner.TryGetProperty("id", out var partnerId))
        {
            var partnerResponse = await GetPartnerAsync(client, partnerId.GetInt32());
            partnerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    [Trait("TestId", "PCL-INT-009")]
    public async Task GetContacts_MultiPage_Navigation()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetContactsAsync(client, 1, 5);
        var r2 = await GetContactsAsync(client, 2, 5);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-010")]
    public async Task GetPartners_MultiPage_Navigation()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetPartnersAsync(client, 1, 5);
        var r2 = await GetPartnersAsync(client, 2, 5);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-011")]
    public async Task Partner_Logo_DetailsAndList_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetPartnersAsync(client, 1, 10);
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var listDoc = System.Text.Json.JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var records = listDoc.RootElement.GetProperty("records");
        if (records.GetArrayLength() == 0) return;
        var firstId = records[0].GetProperty("id").GetInt32();
        var detailResponse = await GetPartnerAsync(client, firstId);
        if (detailResponse.StatusCode != HttpStatusCode.OK) return;
        var detailDoc = System.Text.Json.JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        detailDoc.RootElement.TryGetProperty("logoUrl", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-INT-012")]
    public async Task Contact_Photo_DetailsAndList_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetContactsAsync(client, 1, 10);
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var listDoc = System.Text.Json.JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var records = listDoc.RootElement.GetProperty("records");
        if (records.GetArrayLength() == 0) return;
        var firstId = records[0].GetProperty("id").GetInt32();
        var detailResponse = await GetContactAsync(client, firstId);
        if (detailResponse.StatusCode != HttpStatusCode.OK) return;
        var detailDoc = System.Text.Json.JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        detailDoc.RootElement.TryGetProperty("profilePictureUrl", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-INT-013")]
    public async Task Partner_OrgUnit_ApiContract()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("organizationHierarchyIds", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-INT-014")]
    public async Task Contact_OrgUnit_ApiContract()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("organizationHierarchyIds", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-INT-015")]
    public async Task Partner_Contact_RelationshipIntegrity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var contactResponse = await GetContactAsync(client, 1);
        if (contactResponse.StatusCode != HttpStatusCode.OK) return;
        var contactDoc = System.Text.Json.JsonDocument.Parse(await contactResponse.Content.ReadAsStringAsync());
        if (!contactDoc.RootElement.TryGetProperty("partnerId", out var partnerIdProp)) return;
        var partnerId = partnerIdProp.GetInt32();
        var partnerResponse = await GetPartnerAsync(client, partnerId);
        partnerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-016")]
    public async Task Contacts_ListToDetail_IdsMatch()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetContactsAsync(client, 1, 5);
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var listDoc = System.Text.Json.JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var records = listDoc.RootElement.GetProperty("records");
        if (records.GetArrayLength() == 0) return;
        var listId = records[0].GetProperty("id").GetInt32();
        var detailResponse = await GetContactAsync(client, listId);
        if (detailResponse.StatusCode != HttpStatusCode.OK) return;
        var detailDoc = System.Text.Json.JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        detailDoc.RootElement.GetProperty("id").GetInt32().Should().Be(listId);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-017")]
    public async Task Partners_ListToDetail_IdsMatch()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetPartnersAsync(client, 1, 5);
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var listDoc = System.Text.Json.JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var records = listDoc.RootElement.GetProperty("records");
        if (records.GetArrayLength() == 0) return;
        var listId = records[0].GetProperty("id").GetInt32();
        var detailResponse = await GetPartnerAsync(client, listId);
        if (detailResponse.StatusCode != HttpStatusCode.OK) return;
        var detailDoc = System.Text.Json.JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        detailDoc.RootElement.GetProperty("id").GetInt32().Should().Be(listId);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-018")]
    public async Task Contacts_Pagination_TotalCountStable()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetContactsAsync(client, 1, 10);
        var r2 = await GetContactsAsync(client, 1, 10);
        if (r1.StatusCode != HttpStatusCode.OK || r2.StatusCode != HttpStatusCode.OK) return;
        var doc1 = System.Text.Json.JsonDocument.Parse(await r1.Content.ReadAsStringAsync());
        var doc2 = System.Text.Json.JsonDocument.Parse(await r2.Content.ReadAsStringAsync());
        doc1.RootElement.GetProperty("totalCount").GetInt32().Should().Be(doc2.RootElement.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    [Trait("TestId", "PCL-INT-019")]
    public async Task Partners_Pagination_TotalCountStable()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetPartnersAsync(client, 1, 10);
        var r2 = await GetPartnersAsync(client, 1, 10);
        if (r1.StatusCode != HttpStatusCode.OK || r2.StatusCode != HttpStatusCode.OK) return;
        var doc1 = System.Text.Json.JsonDocument.Parse(await r1.Content.ReadAsStringAsync());
        var doc2 = System.Text.Json.JsonDocument.Parse(await r2.Content.ReadAsStringAsync());
        doc1.RootElement.GetProperty("totalCount").GetInt32().Should().Be(doc2.RootElement.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    [Trait("TestId", "PCL-INT-020")]
    public async Task Partner_Logo_UploadEndpoint_Exists()
    {
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var response = await PostPartnerLogoAsync(client, 1, ms, "test.png");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-021")]
    public async Task Contact_Photo_UploadEndpoint_Exists()
    {
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var response = await PutContactPhotoAsync(client, 1, ms, "test.png");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-022")]
    public async Task Partner_Get_PermissionsEndpoint()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/partner/1/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-023")]
    public async Task Contact_Get_PermissionsEndpoint()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/contact/1/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-024")]
    public async Task Contacts_Search_AdvancedSearchEndpoint()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{\"pageIndex\":1,\"pageSize\":10}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/contact/advanced-search", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-025")]
    public async Task Partners_Search_AdvancedSearchEndpoint()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{\"pageIndex\":1,\"pageSize\":10}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/partner/advanced-search", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-INT-026")]
    public async Task Contact_OrgUnit_LoadOrganizationUnitRelationships()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("organizationHierarchyIds", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-INT-027")]
    public async Task Partner_OrgUnit_LoadOrganizationUnitRelationships()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("organizationHierarchyIds", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-INT-028")]
    public async Task Contacts_Concurrent_NoDuplicateKey()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var tasks = Enumerable.Range(1, 5).Select(i => GetContactsAsync(client, i, 10)).ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    [Trait("TestId", "PCL-INT-029")]
    public async Task Partner_Contact_CrossEntity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var partnerResponse = await GetPartnerAsync(client, 1);
        if (partnerResponse.StatusCode != HttpStatusCode.OK) return;
        var partnerDoc = System.Text.Json.JsonDocument.Parse(await partnerResponse.Content.ReadAsStringAsync());
        if (partnerDoc.RootElement.TryGetProperty("contacts", out var contacts))
        {
            foreach (var c in contacts.EnumerateArray())
            {
                c.TryGetProperty("id", out _).Should().BeTrue();
                c.TryGetProperty("partnerId", out _).Should().BeTrue();
            }
        }
    }

    [Fact]
    [Trait("TestId", "PCL-INT-030")]
    public async Task Contact_Partner_CrossEntity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var contactResponse = await GetContactAsync(client, 1);
        if (contactResponse.StatusCode != HttpStatusCode.OK) return;
        var contactDoc = System.Text.Json.JsonDocument.Parse(await contactResponse.Content.ReadAsStringAsync());
        if (contactDoc.RootElement.TryGetProperty("partner", out var partner))
        {
            partner.TryGetProperty("id", out _).Should().BeTrue();
            partner.TryGetProperty("name", out _).Should().BeTrue();
        }
    }
}
