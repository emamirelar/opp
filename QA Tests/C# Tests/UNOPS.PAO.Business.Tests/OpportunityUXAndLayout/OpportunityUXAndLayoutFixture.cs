/**
 * @fileoverview Opportunity UX & Layout test fixture base.
 * Provides shared setup for Opportunity UX/Layout API tests.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityUXAndLayout;

/// <summary>
/// Base fixture for Opportunity UX & Layout integration tests.
/// </summary>
public abstract class OpportunityUXAndLayoutFixtureBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    protected OpportunityUXAndLayoutFixtureBase(PAOWebApplicationFactory<Program> factory)
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

    protected async Task<HttpResponseMessage> GetOpportunityAsync(HttpClient client, int id)
    {
        return await client.GetAsync(OpportunityUXAndLayoutSpec.GetOpportunityEndpoint(id));
    }

    protected async Task<HttpResponseMessage> GetCommentsAsync(HttpClient client, string entityType, int entityId, bool includeReplies = true)
    {
        var url = OpportunityUXAndLayoutSpec.GetCommentsEndpoint(entityType, entityId) + $"?includeReplies={includeReplies}";
        return await client.GetAsync(url);
    }

    protected async Task<HttpResponseMessage> PostCommentAsync(HttpClient client, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(OpportunityUXAndLayoutSpec.CreateCommentEndpoint, content);
    }

    protected async Task<HttpResponseMessage> GetRisksAsync(HttpClient client, int opportunityId)
    {
        return await client.GetAsync(OpportunityUXAndLayoutSpec.GetRisksEndpoint(opportunityId));
    }
}
