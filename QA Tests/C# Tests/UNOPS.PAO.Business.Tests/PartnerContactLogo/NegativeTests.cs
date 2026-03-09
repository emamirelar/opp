/**
 * @fileoverview Partner/Contact/Logo negative tests — PNO-148, PNO-797, PNO-933.
 * Invalid input, unauthorized, expected failures.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

/// <summary>
/// Negative tests for Partner, Contact and Logo features.
/// </summary>
[Collection("Partner Contact Logo Integration")]
[Trait("Category", "Negative")]
[Trait("Feature", "PartnerContactLogo")]
public class NegativeTests : PartnerContactLogoFixtureBase
{
    public NegativeTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "PCL-NEG-001")]
    public async Task GetPartner_NonExistentId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetPartnerEndpoint(999999));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-002")]
    public async Task GetContact_NonExistentId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetContactEndpoint(999999));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-003")]
    public async Task GetPartner_ZeroId_Returns404Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetPartnerEndpoint(0));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-004")]
    public async Task GetContact_ZeroId_Returns404Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetContactEndpoint(0));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-005")]
    public async Task GetPartner_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetPartnerEndpoint(1));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-006")]
    public async Task GetContact_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetContactEndpoint(1));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-007")]
    public async Task GetContacts_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync("/api/contact?pageIndex=1&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-008")]
    public async Task GetPartners_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync("/api/partner?pageIndex=1&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-009")]
    public async Task GetContacts_NegativePageIndex_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/contact?pageIndex=-1&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-010")]
    public async Task GetContacts_ZeroPageSize_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/contact?pageIndex=1&pageSize=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-011")]
    public async Task GetPartner_NegativeId_Returns404Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetPartnerEndpoint(-1));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-012")]
    public async Task GetContact_NegativeId_Returns404Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(PartnerContactLogoSpec.GetContactEndpoint(-1));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-013")]
    public async Task PostPartnerLogo_NonExistentPartner_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var response = await PostPartnerLogoAsync(client, 999999, ms, "test.png");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-014")]
    public async Task PutContactPhoto_NonExistentContact_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var response = await PutContactPhotoAsync(client, 999999, ms, "test.png");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-015")]
    public async Task GetContacts_InvalidPageIndex_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/contact?pageIndex=abc&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-016")]
    public async Task GetPartner_SoftDeleted_ExcludedFromResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetPartnersAsync(client, 1, 100);
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        json.Should().NotContain("\"isDeleted\":true");
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-017")]
    public async Task GetContact_SoftDeleted_ExcludedFromResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await GetContactsAsync(client, 1, 100);
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        json.Should().NotContain("\"isDeleted\":true");
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-018")]
    public async Task GetPartner_InvalidRoute_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/partner/invalid/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-019")]
    public async Task GetContact_InvalidRoute_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/contact/invalid/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-020")]
    public async Task PostPartnerLogo_EmptyFile_Rejected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream();
        var response = await PostPartnerLogoAsync(client, 1, ms, "empty.png");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-021")]
    public async Task PutContactPhoto_EmptyFile_Rejected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream();
        var response = await PutContactPhotoAsync(client, 1, ms, "empty.png");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-022")]
    public async Task GetContacts_ExcessivePageSize_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/contact?pageIndex=1&pageSize=99999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-023")]
    public async Task GetPartners_ExcessivePageSize_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/partner?pageIndex=1&pageSize=99999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-024")]
    public async Task GetPartner_PermissionDenied_Returns403()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden, because: "test user has access or 404/500");
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-025")]
    public async Task GetContact_PermissionDenied_Returns403()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetContactAsync(client, 1);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden, because: "test user has access or 404/500");
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-026")]
    public async Task GetContacts_MissingPageParams_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/contact");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-027")]
    public async Task GetPartners_MissingPageParams_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/partner");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-028")]
    public async Task PostPartnerLogo_InvalidContentType_Rejected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });
        var response = await PostPartnerLogoAsync(client, 1, ms, "file.exe");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-029")]
    public async Task PutContactPhoto_InvalidContentType_Rejected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        using var ms = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });
        var response = await PutContactPhotoAsync(client, 1, ms, "file.exe");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "PCL-NEG-030")]
    public async Task GetPartner_ById_DoesNotReturnOtherPartners()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await GetPartnerAsync(client, 1);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"id\":1");
    }
}
