/**
 * @fileoverview PNO-1166 RegenerateGoOpportunityPdfs test fixture base.
 * Provides shared setup for SystemAdminController RegenerateGoOpportunityPdfs endpoint tests.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Base fixture for PNO-1166 RegenerateGoOpportunityPdfs integration tests.
/// Uses Integration Tests collection for shared PAOWebApplicationFactory.
/// </summary>
public abstract class PNO1166RegeneratePdfFixtureBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    protected PNO1166RegeneratePdfFixtureBase(PAOWebApplicationFactory<Program> factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Creates an authenticated HTTP client with CanRunSeedings permission (admin user).
    /// </summary>
    protected HttpClient CreateAuthenticatedClient()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    /// <summary>
    /// Creates an unauthenticated client (no auth headers).
    /// </summary>
    protected HttpClient CreateUnauthenticatedClient()
    {
        return Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// POSTs to RegenerateGoOpportunityPdfs endpoint with optional onlyMissing query param.
    /// </summary>
    protected async Task<HttpResponseMessage> PostRegeneratePdfsAsync(HttpClient client, bool? onlyMissing = null)
    {
        var url = PNO1166RegeneratePdfSpec.EndpointPath;
        if (onlyMissing.HasValue)
            url += $"?onlyMissing={onlyMissing.Value.ToString().ToLowerInvariant()}";
        return await client.PostAsync(url, null);
    }

    /// <summary>
    /// Deserializes the RegenerateGoOpportunityPdfs response.
    /// </summary>
    protected static RegeneratePdfResponse? ParseResponse(string json)
    {
        return JsonSerializer.Deserialize<RegeneratePdfResponse>(json, JsonOptions);
    }
}

/// <summary>
/// Response model for RegenerateGoOpportunityPdfs endpoint.
/// </summary>
public class RegeneratePdfResponse
{
    public string? Message { get; set; }
    public int TotalProcessed { get; set; }
    public int SubmissionSuccess { get; set; }
    public int SubmissionFailed { get; set; }
    public int SubmissionSkipped { get; set; }
    public int ApprovalSuccess { get; set; }
    public int ApprovalFailed { get; set; }
    public int ApprovalSkipped { get; set; }
    public List<RegeneratePdfResultItem>? Results { get; set; }
}

/// <summary>
/// Per-opportunity result in the response.
/// </summary>
public class RegeneratePdfResultItem
{
    public int OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public bool SubmissionGenerated { get; set; }
    public bool? SubmissionSuccess { get; set; }
    public string? SubmissionError { get; set; }
    public bool SubmissionSkipped { get; set; }
    public bool ApprovalGenerated { get; set; }
    public bool? ApprovalSuccess { get; set; }
    public string? ApprovalError { get; set; }
    public bool ApprovalSkipped { get; set; }
}
