/**
 * @fileoverview Partner/Contact/Logo test fixture base for API integration tests.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

/// <summary>
/// Base fixture for Partner/Contact/Logo API integration tests.
/// </summary>
public abstract class PartnerContactLogoFixtureBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    protected PartnerContactLogoFixtureBase(PAOWebApplicationFactory<Program> factory)
    {
        Factory = factory;
    }

    protected HttpClient CreateAuthenticatedClient()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    protected HttpClient CreateUnauthenticatedClient()
    {
        return Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    protected async Task<HttpResponseMessage> GetPartnerAsync(HttpClient client, int id)
    {
        return await client.GetAsync(PartnerContactLogoSpec.GetPartnerEndpoint(id));
    }

    protected async Task<HttpResponseMessage> GetPartnersAsync(HttpClient client, int pageIndex = 1, int pageSize = 10)
    {
        return await client.GetAsync($"/api/partner?pageIndex={pageIndex}&pageSize={pageSize}");
    }

    protected async Task<HttpResponseMessage> GetContactAsync(HttpClient client, int id)
    {
        return await client.GetAsync(PartnerContactLogoSpec.GetContactEndpoint(id));
    }

    protected async Task<HttpResponseMessage> GetContactsAsync(HttpClient client, int pageIndex = 1, int pageSize = 10)
    {
        return await client.GetAsync($"/api/contact?pageIndex={pageIndex}&pageSize={pageSize}");
    }

    protected async Task<HttpResponseMessage> PostPartnerLogoAsync(HttpClient client, int partnerId, Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        return await client.PostAsync(PartnerContactLogoSpec.PostPartnerLogoEndpoint(partnerId), content);
    }

    protected async Task<HttpResponseMessage> PutContactPhotoAsync(HttpClient client, int contactId, Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        return await client.PutAsync(PartnerContactLogoSpec.PutContactPhotoEndpoint(contactId), content);
    }
}
