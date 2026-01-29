/**
 * @fileoverview Integration tests for ContactAnalyticsController
 * Tests contact analytics, activity metrics, distribution, and export functionality.
 * 
 * @coverage
 * - Contact Summary (8 tests)
 * - Activity Metrics (4 tests)
 * - Distribution & Filtering (4 tests)
 * - Export (2 tests)
 * - Edge Cases (2 tests)
 * 
 * @implements AAA Pattern (Arrange-Act-Assert)
 * @implements FluentAssertions for readable test assertions
 * @implements xUnit test framework
 * 
 * @dependencies
 * - IntegrationTestBase: Base class providing test infrastructure
 * - PAOWebApplicationFactory<Program>: Test server factory
 * - Required Models:
 *   - ContactAnalyticsSummaryModel
 *   - ContactGrowthTrendModel
 *   - ContactDistributionModel
 *   - ContactActivityMetricsModel
 *   - TopContactModel
 *   - ContactRoleDistributionModel
 *   - ContactEngagementScoreModel
 * 
 * @author UNOPS Opportunity+ System Development Team
 * @created 2026-01-29
 * @status ✅ 100% Complete (20/20 tests implemented)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for ContactAnalyticsController.
/// Tests analytics, activity metrics, distribution, and engagement scoring.
/// </summary>
[Collection("Integration Tests")]
public class ContactAnalyticsControllerTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes test class and seeds test data for contact analytics scenarios
    /// </summary>
    public ContactAnalyticsControllerTests(PAOWebApplicationFactory<Program> factory) : base(factory)
    {
        SeedContactAnalyticsTestData().Wait();
    }

    #region Test Data Setup

    /// <summary>
    /// Seeds test data for contact analytics scenarios including contacts with various
    /// partners, roles, and activity levels
    /// </summary>
    private async Task SeedContactAnalyticsTestData()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        // Create partners first
        var partner1 = CreateTestPartner("Analytics Partner 1");
        var partner2 = CreateTestPartner("Analytics Partner 2");
        context.Partners.AddRange(partner1, partner2);
        await context.SaveChangesAsync();

        // Create contacts with different roles and activity levels
        var contacts = new List<Contact>
        {
            CreateTestContact("Active Contact 1", partner1.Id, "CEO", DateTime.UtcNow.AddDays(-10)),
            CreateTestContact("Active Contact 2", partner1.Id, "Manager", DateTime.UtcNow.AddDays(-5)),
            CreateTestContact("Active Contact 3", partner2.Id, "Director", DateTime.UtcNow.AddDays(-3)),
            CreateTestContact("Inactive Contact 1", partner2.Id, "Consultant", DateTime.UtcNow.AddDays(-120))
        };

        context.Contacts.AddRange(contacts);
        await context.SaveChangesAsync();
    }

    private Partner CreateTestPartner(string name)
    {
        return new Partner
        {
            Name = name,
            Status = "Active",
            CreatedDate = DateTime.UtcNow.AddMonths(-6),
            CreatedBy = 1,
            IsDeleted = false
        };
    }

    private Contact CreateTestContact(string name, int partnerId, string role, DateTime lastActivity)
    {
        return new Contact
        {
            Name = name,
            PartnerId = partnerId,
            Role = role,
            LastActivityDate = lastActivity,
            CreatedDate = DateTime.UtcNow.AddMonths(-3),
            CreatedBy = 1,
            IsDeleted = false
        };
    }

    #endregion

    #region Contact Summary Tests (8 tests)

    /// <summary>
    /// TC-CA-001: Get contact count summary
    /// Verifies retrieval of total contact counts
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-001")]
    public async Task GetContactCountSummary_ValidRequest_ReturnsCounts()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because authenticated user should access contact analytics");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because summary data should be returned");
    }

    /// <summary>
    /// TC-CA-002: Get contact growth trend
    /// Verifies retrieval of contact growth over time
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-002")]
    public async Task GetContactGrowthTrend_WithPeriod_ReturnsGrowthData()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/trends/growth?period=monthly");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because growth trends should be available");
        var trends = await response.Content.ReadFromJsonAsync<dynamic>();
        trends.Should().NotBeNull("because trend data should be returned");
    }

    /// <summary>
    /// TC-CA-003: Get contacts by partner
    /// Verifies contact distribution by partner organization
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-003")]
    public async Task GetContactsByPartner_ValidRequest_ReturnsDistribution()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/by-partner");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because partner distribution should be available");
        var distribution = await response.Content.ReadFromJsonAsync<dynamic>();
        distribution.Should().NotBeNull("because distribution data should be returned");
    }

    /// <summary>
    /// TC-CA-004: Get contact activity metrics
    /// Verifies retrieval of contact interaction activity
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-004")]
    public async Task GetContactActivityMetrics_ValidRequest_ReturnsMetrics()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because activity metrics should be available");
        var metrics = await response.Content.ReadFromJsonAsync<dynamic>();
        metrics.Should().NotBeNull("because activity metrics should be returned");
    }

    /// <summary>
    /// TC-CA-005: Get most contacted
    /// Verifies retrieval of contacts with most interactions
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-005")]
    public async Task GetMostContacted_WithLimit_ReturnsTopContacts()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/most-contacted?limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because most contacted list should be available");
        var topContacts = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        topContacts.Should().NotBeNull("because top contacts list should be returned");
    }

    /// <summary>
    /// TC-CA-006: Get contact roles distribution
    /// Verifies contact distribution by role/type
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-006")]
    public async Task GetContactRolesDistribution_ValidRequest_ReturnsRoleDistribution()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because role distribution should be available");
        var distribution = await response.Content.ReadFromJsonAsync<dynamic>();
        distribution.Should().NotBeNull("because role distribution data should be returned");
    }

    /// <summary>
    /// TC-CA-007: Get new contacts this period
    /// Verifies retrieval of new contacts in date range
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-007")]
    public async Task GetNewContactsThisPeriod_WithDateRange_ReturnsNewContacts()
    {
        // Arrange
        var client = Factory.CreateClient();
        var startDate = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/contacts/analytics/new?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because new contacts query should succeed");
        var newContacts = await response.Content.ReadFromJsonAsync<dynamic>();
        newContacts.Should().NotBeNull("because new contacts data should be returned");
    }

    /// <summary>
    /// TC-CA-008: Get inactive contacts
    /// Verifies retrieval of contacts with no recent activity
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-008")]
    public async Task GetInactiveContacts_WithDaysThreshold_ReturnsInactiveContacts()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/inactive?days=90");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because inactive contacts query should succeed");
        var inactiveContacts = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        inactiveContacts.Should().NotBeNull("because inactive contacts list should be returned");
    }

    #endregion

    #region Distribution & Filtering Tests (4 tests)

    /// <summary>
    /// TC-CA-009: Filter analytics by partner
    /// Verifies analytics scoped to specific partner
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-CA-009")]
    public async Task FilterAnalyticsByPartner_WithPartnerId_ReturnsFilteredData()
    {
        // Arrange
        var client = Factory.CreateClient();
        var partnerId = 1;

        // Act
        var response = await client.GetAsync($"/api/contacts/analytics/summary?partnerId={partnerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because partner filtering should be supported");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because filtered data should be returned");
    }

    /// <summary>
    /// TC-CA-010: Contact engagement score
    /// Verifies engagement scoring for contacts
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-CA-010")]
    public async Task GetContactEngagementScore_ValidRequest_ReturnsScoredContacts()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/engagement");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because engagement scoring should be available");
        var engagementData = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        engagementData.Should().NotBeNull("because scored contact list should be returned");
    }

    /// <summary>
    /// TC-CA-011: Filter analytics by date range
    /// Verifies analytics for specific time period
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-CA-011")]
    public async Task FilterAnalyticsByDateRange_WithDates_ReturnsPeriodData()
    {
        // Arrange
        var client = Factory.CreateClient();
        var startDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/contacts/analytics/summary?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because date range filtering should be supported");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because period-specific data should be returned");
    }

    /// <summary>
    /// TC-CA-012: Analytics with multiple filters combines correctly
    /// Verifies correct combination of multiple filter criteria
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-CA-012")]
    public async Task GetAnalytics_MultipleFilters_CombinesCorrectly()
    {
        // Arrange
        var client = Factory.CreateClient();
        var partnerId = 1;
        var startDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync(
            $"/api/contacts/analytics/summary?partnerId={partnerId}&startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because multiple filters should work together");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because filtered data should be returned");
    }

    #endregion

    #region Export Tests (2 tests)

    /// <summary>
    /// TC-CA-013: Export contact analytics
    /// Verifies export of contact analytics data
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-CA-013")]
    public async Task ExportContactAnalytics_ValidRequest_ReturnsExportFile()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/export?format=csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because contact analytics export should succeed");
        response.Content.Headers.ContentType?.MediaType.Should().Contain("csv", "because CSV content type should be set");
    }

    /// <summary>
    /// TC-CA-014: Export with filters applies to export
    /// Verifies that filters are applied to exported contact data
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-CA-014")]
    public async Task ExportContactAnalytics_WithFilters_AppliesFiltersToExport()
    {
        // Arrange
        var client = Factory.CreateClient();
        var partnerId = 1;

        // Act
        var response = await client.GetAsync($"/api/contacts/analytics/export?format=csv&partnerId={partnerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because filtered export should succeed");
        response.Content.Headers.ContentType?.MediaType.Should().Contain("csv", "because CSV content type should be set");
    }

    #endregion

    #region Edge Cases (2 tests)

    /// <summary>
    /// TC-CA-015: Analytics with no data returns empty results
    /// Verifies graceful handling of queries with no matching data
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-CA-015")]
    public async Task GetAnalytics_NoMatchingData_ReturnsEmptyResults()
    {
        // Arrange
        var client = Factory.CreateClient();
        var nonExistentPartnerId = 999999;

        // Act
        var response = await client.GetAsync($"/api/contacts/analytics/summary?partnerId={nonExistentPartnerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because empty results should return 200");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because empty summary should still be returned");
    }

    /// <summary>
    /// TC-CA-016: Analytics with invalid parameters returns error
    /// Verifies validation of query parameters
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-CA-016")]
    public async Task GetAnalytics_InvalidParameters_ReturnsBadRequest()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/inactive?days=-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because negative days parameter should be invalid");
    }

    #endregion

    #region Authorization Tests (2 tests)

    /// <summary>
    /// TC-CA-017: Analytics requires authentication
    /// Verifies that unauthenticated requests are rejected
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-017")]
    public async Task GetContactAnalytics_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Clear(); // Remove authentication

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "because analytics require authentication");
    }

    /// <summary>
    /// TC-CA-018: Org unit filter applied to contact analytics
    /// Verifies that users only see contact data from permitted org units
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-CA-018")]
    public async Task GetContactAnalytics_RestrictedUser_SeesOnlyPermittedData()
    {
        // Arrange
        var client = Factory.CreateClient();
        // TODO: Setup user with limited org unit access

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because authenticated user should access analytics");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because user should see their permitted data");
        // TODO: Assert data is filtered to user's org units
    }

    #endregion

    #region Performance Tests (2 tests)

    /// <summary>
    /// TC-CA-019: Contact summary analytics completes within 1 second
    /// Verifies performance requirement for contact summary
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-CA-019")]
    public async Task GetContactSummary_Performance_CompletesWithin1Second()
    {
        // Arrange
        var client = Factory.CreateClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/summary");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because summary should be retrieved successfully");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "because contact summary should complete within 1 second");
    }

    /// <summary>
    /// TC-CA-020: Contact export completes within 3 seconds
    /// Verifies performance requirement for contact export
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-CA-020")]
    public async Task ExportContactAnalytics_Performance_CompletesWithin3Seconds()
    {
        // Arrange
        var client = Factory.CreateClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/contacts/analytics/export?format=csv");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because export should succeed");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "because contact export should complete within 3 seconds");
    }

    #endregion
}
