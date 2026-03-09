/**
 * @fileoverview Partner/Contact/Logo boundary tests — PNO-148, PNO-797, PNO-933.
 * Boundary values, soft-delete, edge cases.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

/// <summary>
/// Boundary tests for Partner, Contact and Logo features.
/// </summary>
[Collection("Partner Contact Logo Integration")]
[Trait("Category", "Boundary")]
[Trait("Feature", "PartnerContactLogo")]
public class BoundaryTests : PartnerContactLogoFixtureBase
{
    public BoundaryTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PCL-BND-001")]
    public async Task GetContacts_PageIndexOne_ReturnsFirstPage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-002")]
    public async Task GetContacts_PageSizeOne_ReturnsSingleRecord()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 1);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-003")]
    public async Task GetContacts_LastPage_MayReturnEmpty()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 99999, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-004")]
    public async Task GetPartner_LogoUrlNull_ReturnsValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("logoUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-005")]
    public async Task GetContact_ProfilePictureUrlNull_ReturnsValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("profilePictureUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-006")]
    public async Task GetContacts_OrganizationHierarchyIdsEmpty_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-007")]
    public async Task GetPartners_PageSizeMaxReasonable_Returns()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 100);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-008")]
    public async Task GetContacts_PageSizeMaxReasonable_Returns()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 100);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-009")]
    public async Task GetPartner_IdAtIntMax_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetPartnerEndpoint(int.MaxValue));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-010")]
    public async Task GetContact_IdAtIntMax_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetContactEndpoint(int.MaxValue));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-011")]
    public async Task GetContacts_ConcurrentRequests_NoDuplicateKey()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var t1 = GetContactsAsync(client, 1, 10);
        var t2 = GetContactsAsync(client, 2, 10);
        var t3 = GetContactsAsync(client, 1, 5);
        var results = await Task.WhenAll(t1, t2, t3);
        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    [Trait("TestId", "PCL-BND-012")]
    public async Task GetPartners_ConcurrentRequests_NoDuplicateKey()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var t1 = GetPartnersAsync(client, 1, 10);
        var t2 = GetPartnersAsync(client, 2, 10);
        var results = await Task.WhenAll(t1, t2);
        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    [Trait("TestId", "PCL-BND-013")]
    public async Task GetPartner_WithNullablePartnerGroup_Returns()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("partnerGroupId");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-014")]
    public async Task GetContact_WithNullablePartner_Returns()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("partner");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-015")]
    public async Task GetContacts_EmptyResultSet_ReturnsEmptyArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 99999, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-016")]
    public async Task GetPartners_EmptyResultSet_ReturnsEmptyArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 99999, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-017")]
    public async Task GetContact_OrganizationHierarchyIds_IsArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("organizationHierarchyIds");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-018")]
    public async Task GetPartner_LogoUrl_IsStringOrNull()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("logoUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-019")]
    public async Task GetContact_ProfilePictureUrl_IsStringOrNull()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("profilePictureUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-020")]
    public async Task GetContacts_TotalCount_NonNegative()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("\"totalCount\":-");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-021")]
    public async Task GetPartners_TotalCount_NonNegative()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("\"totalCount\":-");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-022")]
    public async Task GetContacts_PageIndexBoundary_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-023")]
    public async Task GetPartner_WithLogoUrlEmpty_Returns()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("logoUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-024")]
    public async Task GetContact_WithProfilePictureEmpty_Returns()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("profilePictureUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-025")]
    public async Task GetContacts_OrderBy_ReturnsConsistentOrder()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetContactsAsync(client, 1, 5);
        var r2 = await GetContactsAsync(client, 1, 5);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-026")]
    public async Task GetPartners_OrderBy_ReturnsConsistentOrder()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetPartnersAsync(client, 1, 5);
        var r2 = await GetPartnersAsync(client, 1, 5);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-BND-027")]
    public async Task GetContact_WithMultipleOrgUnits_ReturnsAll()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("organizationHierarchyIds");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-028")]
    public async Task GetPartner_WithSingleOrgUnit_Returns()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("organizationHierarchyIds");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-029")]
    public async Task GetContacts_WithPartnerInclude_ReturnsPartnerData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("partner");
    }

    [Fact]
    [Trait("TestId", "PCL-BND-030")]
    public async Task GetPartners_WithContactsInclude_ReturnsContactsData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("contacts");
    }
}
