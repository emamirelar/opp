/**
 * @fileoverview Partner/Contact/Logo positive tests — PNO-148, PNO-797, PNO-933.
 * Happy path scenarios for logo display, contacts page load, org unit mapping.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

/// <summary>
/// Positive tests for Partner, Contact and Logo features.
/// Requirements: PNO-148, PNO-797, PNO-933.
/// </summary>
[Collection("Partner Contact Logo Integration")]
[Trait("Category", "Positive")]
[Trait("Feature", "PartnerContactLogo")]
public class PositiveTests : PartnerContactLogoFixtureBase
{
    public PositiveTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PCL-POS-001")]
    [Trait("JIRA", "PNO-797")]
    public async Task GetContacts_WithValidPagination_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-POS-002")]
    [Trait("JIRA", "PNO-797")]
    public async Task GetContacts_FirstPage_ReturnsRecordsOrEmpty()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
        json.Should().Contain("totalCount");
    }

    [Fact]
    [Trait("TestId", "PCL-POS-003")]
    [Trait("JIRA", "PNO-797")]
    public async Task GetContacts_NoDuplicateKeyError_ResponseIsValid()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactsAsync(client, 1, 5);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("An item with the same key has already been added");
    }

    [Fact]
    [Trait("TestId", "PCL-POS-004")]
    [Trait("JIRA", "PNO-148")]
    public async Task GetPartner_WithLogoUrl_ReturnsPartnerWithLogoField()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("logoUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-POS-005")]
    [Trait("JIRA", "PNO-148")]
    public async Task GetContact_WithProfilePictureUrl_ReturnsContactWithPhotoField()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("profilePictureUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-POS-006")]
    [Trait("JIRA", "PNO-148")]
    public async Task GetPartners_ReturnsListWithLogoUrlField()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnersAsync(client, 1, 10);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("logoUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-POS-007")]
    [Trait("JIRA", "PNO-933")]
    public async Task GetContact_WithOrgUnitRelationships_ReturnsOrganizationHierarchyIds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("organizationHierarchyIds");
    }

    [Fact]
    [Trait("TestId", "PCL-POS-008")]
    [Trait("JIRA", "PNO-797")]
    public async Task GetContacts_MultiplePages_EachReturns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await GetContactsAsync(client, 1, 5);
        var r2 = await GetContactsAsync(client, 2, 5);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "PCL-POS-009")]
    [Trait("JIRA", "PNO-148")]
    public async Task GetPartner_DetailsPage_IncludesLogoAndPartnerData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("name");
        json.Should().Contain("logoUrl");
    }

    [Fact]
    [Trait("TestId", "PCL-POS-010")]
    [Trait("JIRA", "PNO-933")]
    public async Task GetContact_WithOrgUnit_IncludesOrgUnitInResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("organizationHierarchyIds");
    }
}
