using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;

namespace UNOPS.PAO.Business.Tests.OfficesFeature;

/// <summary>
/// Base fixture for Offices Feature integration tests (HTTP API).
/// Uses organization-hierarchy API as office hierarchy data source (PNO-1213).
/// </summary>
public abstract class OfficesFeatureIntegrationFixtureBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected const string OrganizationHierarchyBase = "/api/organization-hierarchy";

    protected OfficesFeatureIntegrationFixtureBase(PAOWebApplicationFactory<Program> factory)
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
}
