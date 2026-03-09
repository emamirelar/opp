/**
 * @fileoverview Admin, Access Control & Validation test fixture base.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.AdminAccessValidation;

/// <summary>
/// Base fixture for Admin, Access Control &amp; Validation integration tests.
/// </summary>
public abstract class AdminAccessValidationFixtureBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    protected AdminAccessValidationFixtureBase(PAOWebApplicationFactory<Program> factory)
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

    protected async Task<HttpResponseMessage> PostCreateOpportunityAsync(HttpClient client, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(AdminAccessValidationSpec.OpportunityBase, content);
    }

    protected async Task<HttpResponseMessage> PostCreateFromPartnerAsync(HttpClient client, int partnerId, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(AdminAccessValidationSpec.CreateOpportunityFromPartner(partnerId), content);
    }
}
