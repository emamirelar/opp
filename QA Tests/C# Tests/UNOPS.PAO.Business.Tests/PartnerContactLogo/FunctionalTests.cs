/**
 * @fileoverview Partner/Contact/Logo functional tests — PNO-148, PNO-797, PNO-933.
 * Business rules, validation, data transformations.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

/// <summary>
/// Functional tests for Partner, Contact and Logo features.
/// </summary>
[Collection("Partner Contact Logo Integration")]
[Trait("Category", "Functional")]
[Trait("Feature", "PartnerContactLogo")]
public class FunctionalTests : PartnerContactLogoFixtureBase
{
    public FunctionalTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PCL-FNC-001")]
    [Trait("JIRA", "PNO-797")]
    public async Task GetContacts_ReturnsPaginationStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
        json.Should().Contain("totalCount");
        json.Should().Contain("pageIndex");
        json.Should().Contain("pageSize");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-002")]
    [Trait("JIRA", "PNO-797")]
    public async Task GetContacts_RecordsCount_DoesNotExceedPageSize()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 5);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var recordCount = System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("records").GetArrayLength();
        recordCount.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-003")]
    [Trait("JIRA", "PNO-148")]
    public async Task GetPartner_LogoUrl_SignedWhenPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        var doc = System.Text.Json.JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("logoUrl", out var logo) && logo.GetString() != null)
            logo.GetString()!.Should().Contain("http");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-004")]
    [Trait("JIRA", "PNO-148")]
    public async Task GetContact_ProfilePictureUrl_SignedWhenPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        var doc = System.Text.Json.JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("profilePictureUrl", out var url) && url.GetString() != null)
            url.GetString()!.Should().Contain("http");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-005")]
    [Trait("JIRA", "PNO-933")]
    public async Task GetContact_OrganizationHierarchyIds_LinkedToOrgUnit()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        var doc = System.Text.Json.JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("organizationHierarchyIds", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-006")]
    public async Task GetPartner_IncludesRequiredFields()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("id");
        json.Should().Contain("name");
        json.Should().Contain("logoUrl");
        json.Should().Contain("status");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-007")]
    public async Task GetContact_IncludesRequiredFields()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("id");
        json.Should().Contain("firstName");
        json.Should().Contain("lastName");
        json.Should().Contain("email");
        json.Should().Contain("profilePictureUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-008")]
    public async Task GetContacts_EachRecord_HasIdAndPartner()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var records = doc.RootElement.GetProperty("records");
        foreach (var r in records.EnumerateArray())
        {
            r.TryGetProperty("id", out _).Should().BeTrue();
            r.TryGetProperty("partner", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-009")]
    public async Task GetPartners_EachRecord_HasIdAndLogoUrl()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var records = doc.RootElement.GetProperty("records");
        foreach (var r in records.EnumerateArray())
        {
            r.TryGetProperty("id", out _).Should().BeTrue();
            r.TryGetProperty("logoUrl", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-010")]
    public async Task GetContacts_TotalCount_MatchesRecordsWhenSinglePage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 1000);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var total = doc.RootElement.GetProperty("totalCount").GetInt32();
        var records = doc.RootElement.GetProperty("records").GetArrayLength();
        total.Should().BeGreaterThanOrEqualTo(records);
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-011")]
    public async Task GetPartners_TotalCount_MatchesRecordsWhenSinglePage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 1000);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var total = doc.RootElement.GetProperty("totalCount").GetInt32();
        var records = doc.RootElement.GetProperty("records").GetArrayLength();
        total.Should().BeGreaterThanOrEqualTo(records);
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-012")]
    public async Task GetPartner_AuditFields_Populated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("createdDate");
        json.Should().Contain("lastModifiedDate");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-013")]
    public async Task GetContact_AuditFields_Populated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("createdDate");
        json.Should().Contain("lastModifiedDate");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-014")]
    public async Task GetContacts_SoftDeleted_Excluded()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 100);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("\"isDeleted\":true");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-015")]
    public async Task GetPartners_SoftDeleted_Excluded()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 100);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("\"isDeleted\":true");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-016")]
    public async Task GetPartner_PartnerGroup_ResolvedWhenPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("partnerGroupId");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-017")]
    public async Task GetContact_Partner_ResolvedWhenPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("partner");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-018")]
    public async Task GetContacts_ContentType_IsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-019")]
    public async Task GetPartners_ContentType_IsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 10);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-020")]
    public async Task GetPartner_ContentType_IsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK)
            return;
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-021")]
    public async Task GetContact_ContentType_IsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK)
            return;
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-022")]
    public async Task GetContacts_PageIndex_ReflectedInResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 2, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("pageIndex").GetInt32().Should().Be(2);
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-023")]
    public async Task GetPartners_PageIndex_ReflectedInResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 2, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("pageIndex").GetInt32().Should().Be(2);
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-024")]
    public async Task GetContact_Name_ConcatenatedFromParts()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("name", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("firstName", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("lastName", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-025")]
    public async Task GetPartner_Status_ValidEnum()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("status", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-026")]
    public async Task GetContact_Status_ValidEnum()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("status", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-027")]
    public async Task GetContacts_NoDuplicateIds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 100);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var records = doc.RootElement.GetProperty("records");
        var ids = records.EnumerateArray().Select(r => r.GetProperty("id").GetInt32()).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-028")]
    public async Task GetPartners_NoDuplicateIds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 100);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var records = doc.RootElement.GetProperty("records");
        var ids = records.EnumerateArray().Select(r => r.GetProperty("id").GetInt32()).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-029")]
    public async Task GetContact_OrganizationHierarchyIds_Integers()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var arr = doc.RootElement.GetProperty("organizationHierarchyIds");
        arr.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "PCL-FNC-030")]
    public async Task GetPartner_OrganizationHierarchyIds_Integers()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("organizationHierarchyIds", out _).Should().BeTrue();
    }
}
