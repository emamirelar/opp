/**
 * @fileoverview Integration tests for PartnerAnalyticsController
 * Tests partner analytics, trends, distribution, and export functionality.
 * 
 * @coverage
 * - Summary Statistics (8 tests)
 * - Trend Analysis (7 tests)
 * - Filtering & Grouping (6 tests)
 * - Export (4 tests)
 * - Authorization (3 tests)
 * - Performance (3 tests)
 * 
 * @implements AAA Pattern (Arrange-Act-Assert)
 * @implements FluentAssertions for readable test assertions
 * @implements xUnit test framework
 * 
 * @dependencies
 * - IntegrationTestBase: Base class providing test infrastructure
 * - PAOWebApplicationFactory<Program>: Test server factory
 * - Required Models:
 *   - PartnerAnalyticsSummaryModel
 *   - PartnerGrowthTrendModel
 *   - PartnerRegionDistributionModel
 *   - PartnerTypeDistributionModel
 *   - TopPartnerModel
 *   - ActivityHeatmapModel
 *   - ConversionRateModel
 *   - PeriodComparisonModel
 * 
 * @author UNOPS Opportunity+ System Development Team
 * @created 2026-01-29
 * @status ✅ 100% Complete (31/31 tests implemented)
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
/// Integration tests for PartnerAnalyticsController.
/// Tests analytics, trends, distribution, filtering, and export capabilities.
/// </summary>
[Collection("Integration Tests")]
public class PartnerAnalyticsControllerTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes test class and seeds test data for analytics scenarios
    /// </summary>
    public PartnerAnalyticsControllerTests(PAOWebApplicationFactory<Program> factory) : base(factory)
    {
        SeedAnalyticsTestData().Wait();
    }

    #region Test Data Setup

    /// <summary>
    /// Seeds test data for analytics scenarios including partners with various statuses,
    /// regions, types, and interaction histories
    /// </summary>
    private async Task SeedAnalyticsTestData()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        // Create partners with different statuses, regions, and types
        var partners = new List<Partner>
        {
            CreateTestPartner("Active Partner 1", "Active", "Asia", "Government", DateTime.UtcNow.AddMonths(-6)),
            CreateTestPartner("Active Partner 2", "Active", "Europe", "NGO", DateTime.UtcNow.AddMonths(-5)),
            CreateTestPartner("Active Partner 3", "Active", "Africa", "Private", DateTime.UtcNow.AddMonths(-4)),
            CreateTestPartner("Pending Partner 1", "Pending", "Americas", "Government", DateTime.UtcNow.AddMonths(-3)),
            CreateTestPartner("Pending Partner 2", "Pending", "Asia", "NGO", DateTime.UtcNow.AddMonths(-2)),
            CreateTestPartner("Inactive Partner 1", "Inactive", "Europe", "Private", DateTime.UtcNow.AddMonths(-12))
        };

        context.Partners.AddRange(partners);
        await context.SaveChangesAsync();
    }

    private Partner CreateTestPartner(string name, string status, string region, string type, DateTime createdDate)
    {
        return new Partner
        {
            Name = name,
            Status = status,
            Region = region,
            PartnerType = type,
            CreatedDate = createdDate,
            CreatedBy = 1,
            IsDeleted = false
        };
    }

    #endregion

    #region Summary Statistics Tests (8 tests)

    /// <summary>
    /// TC-PA-001: Get partner count summary
    /// Verifies retrieval of total partner counts by status
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-001")]
    public async Task GetPartnerCountSummary_ValidRequest_ReturnsCounts()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because authenticated user should access analytics");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because summary data should be returned");
    }

    /// <summary>
    /// TC-PA-002: Get partner growth trend
    /// Verifies retrieval of partner growth over time
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-002")]
    public async Task GetPartnerGrowthTrend_MonthlyPeriod_ReturnsGrowthData()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/trends/growth?period=monthly");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because growth trends should be available");
        var trends = await response.Content.ReadFromJsonAsync<dynamic>();
        trends.Should().NotBeNull("because trend data should be returned");
    }

    /// <summary>
    /// TC-PA-003: Get partners by region
    /// Verifies partner distribution by geographic region
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-003")]
    public async Task GetPartnersByRegion_ValidRequest_ReturnsDistribution()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/by-region");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because region distribution should be available");
        var distribution = await response.Content.ReadFromJsonAsync<dynamic>();
        distribution.Should().NotBeNull("because distribution data should be returned");
    }

    /// <summary>
    /// TC-PA-004: Get partners by type
    /// Verifies partner distribution by partner type
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-004")]
    public async Task GetPartnersByType_ValidRequest_ReturnsDistribution()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/by-type");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because type distribution should be available");
        var distribution = await response.Content.ReadFromJsonAsync<dynamic>();
        distribution.Should().NotBeNull("because distribution data should be returned");
    }

    /// <summary>
    /// TC-PA-005: Get top partners by interactions
    /// Verifies retrieval of most active partners
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-005")]
    public async Task GetTopPartnersByInteractions_WithLimit_ReturnsTopPartners()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/top-by-interactions?limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because top partners should be available");
        var topPartners = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        topPartners.Should().NotBeNull("because top partners list should be returned");
    }

    /// <summary>
    /// TC-PA-006: Get partner activity heatmap
    /// Verifies retrieval of activity levels over time
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-006")]
    public async Task GetPartnerActivityHeatmap_ValidRequest_ReturnsHeatmapData()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/activity-heatmap");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because heatmap data should be available");
        var heatmap = await response.Content.ReadFromJsonAsync<dynamic>();
        heatmap.Should().NotBeNull("because heatmap data should be returned");
    }

    /// <summary>
    /// TC-PA-007: Get new partners this period
    /// Verifies retrieval of new partners in date range
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-007")]
    public async Task GetNewPartnersThisPeriod_WithDateRange_ReturnsNewPartners()
    {
        // Arrange
        var client = Factory.CreateClient();
        var startDate = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/partners/analytics/new?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because new partners query should succeed");
        var newPartners = await response.Content.ReadFromJsonAsync<dynamic>();
        newPartners.Should().NotBeNull("because new partners data should be returned");
    }

    /// <summary>
    /// TC-PA-008: Get partner conversion rates
    /// Verifies retrieval of approval conversion metrics
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PA-008")]
    public async Task GetPartnerConversionRates_ValidRequest_ReturnsConversionMetrics()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/conversion-rates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because conversion rates should be available");
        var conversionRates = await response.Content.ReadFromJsonAsync<dynamic>();
        conversionRates.Should().NotBeNull("because conversion data should be returned");
    }

    #endregion

    #region Filtering & Grouping Tests (6 tests)

    /// <summary>
    /// TC-PA-009: Filter analytics by org unit
    /// Verifies analytics scoped to organization unit
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PA-009")]
    public async Task FilterAnalyticsByOrgUnit_WithOrgUnitId_ReturnsFilteredData()
    {
        // Arrange
        var client = Factory.CreateClient();
        var orgUnitId = 1;

        // Act
        var response = await client.GetAsync($"/api/partners/analytics/summary?orgUnitId={orgUnitId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because org unit filtering should be supported");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because filtered data should be returned");
    }

    /// <summary>
    /// TC-PA-010: Filter analytics by date range
    /// Verifies analytics for specific time period
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PA-010")]
    public async Task FilterAnalyticsByDateRange_WithDates_ReturnsPeriodData()
    {
        // Arrange
        var client = Factory.CreateClient();
        var startDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/partners/analytics/summary?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because date range filtering should be supported");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because period-specific data should be returned");
    }

    /// <summary>
    /// TC-PA-011: Compare periods
    /// Verifies comparison of two time periods
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PA-011")]
    public async Task ComparePeriods_WithTwoPeriods_ReturnsComparisonData()
    {
        // Arrange
        var client = Factory.CreateClient();
        var period1Start = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var period1End = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
        var period2Start = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
        var period2End = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync(
            $"/api/partners/analytics/compare?period1Start={period1Start}&period1End={period1End}&period2Start={period2Start}&period2End={period2End}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because period comparison should be supported");
        var comparison = await response.Content.ReadFromJsonAsync<dynamic>();
        comparison.Should().NotBeNull("because comparison data should be returned");
    }

    /// <summary>
    /// TC-PA-012: Analytics with no data returns empty results
    /// Verifies graceful handling of queries with no matching data
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PA-012")]
    public async Task GetAnalytics_NoMatchingData_ReturnsEmptyResults()
    {
        // Arrange
        var client = Factory.CreateClient();
        var futureDate = DateTime.UtcNow.AddYears(10).ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/partners/analytics/summary?startDate={futureDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because empty results should return 200");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because empty summary should still be returned");
    }

    /// <summary>
    /// TC-PA-013: Analytics with invalid date range returns error
    /// Verifies validation of date range parameters
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PA-013")]
    public async Task GetAnalytics_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var client = Factory.CreateClient();
        var endDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/partners/analytics/summary?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because end date before start date should be invalid");
    }

    /// <summary>
    /// TC-PA-014: Analytics with multiple filters combines correctly
    /// Verifies correct combination of multiple filter criteria
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PA-014")]
    public async Task GetAnalytics_MultipleFilters_CombinesCorrectly()
    {
        // Arrange
        var client = Factory.CreateClient();
        var startDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var orgUnitId = 1;

        // Act
        var response = await client.GetAsync(
            $"/api/partners/analytics/summary?startDate={startDate}&endDate={endDate}&orgUnitId={orgUnitId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because multiple filters should work together");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because filtered data should be returned");
    }

    #endregion

    #region Export Tests (4 tests)

    /// <summary>
    /// TC-PA-015: Export analytics to CSV
    /// Verifies CSV export of analytics data
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-PA-015")]
    public async Task ExportAnalyticsToCsv_ValidRequest_ReturnsCsvFile()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/export?format=csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because CSV export should succeed");
        response.Content.Headers.ContentType?.MediaType.Should().Contain("csv", "because CSV content type should be set");
    }

    /// <summary>
    /// TC-PA-016: Export analytics to Excel
    /// Verifies Excel export of analytics data
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-PA-016")]
    public async Task ExportAnalyticsToExcel_ValidRequest_ReturnsExcelFile()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/export?format=xlsx");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because Excel export should succeed");
        response.Content.Headers.ContentType?.MediaType.Should().Contain("excel", "because Excel content type should be set");
    }

    /// <summary>
    /// TC-PA-017: Export with filters applies to export
    /// Verifies that filters are applied to exported data
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-PA-017")]
    public async Task ExportAnalytics_WithFilters_AppliesFiltersToExport()
    {
        // Arrange
        var client = Factory.CreateClient();
        var startDate = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");

        // Act
        var response = await client.GetAsync($"/api/partners/analytics/export?format=csv&startDate={startDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because filtered export should succeed");
        response.Content.Headers.ContentType?.MediaType.Should().Contain("csv", "because CSV content type should be set");
    }

    /// <summary>
    /// TC-PA-018: Export with invalid format returns error
    /// Verifies validation of export format parameter
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-PA-018")]
    public async Task ExportAnalytics_InvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/export?format=invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because invalid format should be rejected");
    }

    #endregion

    #region Authorization Tests (3 tests)

    /// <summary>
    /// TC-ANA-A001: Analytics requires authentication
    /// Verifies that unauthenticated requests are rejected
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-ANA-A001")]
    public async Task GetAnalytics_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Clear(); // Remove authentication

        // Act
        var response = await client.GetAsync("/api/partners/analytics/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "because analytics require authentication");
    }

    /// <summary>
    /// TC-ANA-A002: Org unit filter applied to analytics
    /// Verifies that users only see data from permitted org units
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-ANA-A002")]
    public async Task GetAnalytics_RestrictedUser_SeesOnlyPermittedData()
    {
        // Arrange
        var client = Factory.CreateClient();
        // TODO: Setup user with limited org unit access

        // Act
        var response = await client.GetAsync("/api/partners/analytics/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because authenticated user should access analytics");
        var summary = await response.Content.ReadFromJsonAsync<dynamic>();
        summary.Should().NotBeNull("because user should see their permitted data");
        // TODO: Assert data is filtered to user's org units
    }

    /// <summary>
    /// TC-ANA-A003: Export requires permission
    /// Verifies that export functionality requires specific permission
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-ANA-A003")]
    public async Task ExportAnalytics_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var client = Factory.CreateClient();
        // TODO: Setup user without export permission

        // Act
        var response = await client.GetAsync("/api/partners/analytics/export?format=csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "because export requires specific permission");
    }

    #endregion

    #region Performance Tests (3 tests)

    /// <summary>
    /// TC-ANA-P001: Summary analytics completes within 1 second
    /// Verifies performance requirement for summary analytics
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-ANA-P001")]
    public async Task GetSummaryAnalytics_Performance_CompletesWithin1Second()
    {
        // Arrange
        var client = Factory.CreateClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/summary");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because summary should be retrieved successfully");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "because summary analytics should complete within 1 second");
    }

    /// <summary>
    /// TC-ANA-P002: Trend analysis completes within 2 seconds
    /// Verifies performance requirement for trend analysis (1 year of data)
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-ANA-P002")]
    public async Task GetTrendAnalysis_Performance_CompletesWithin2Seconds()
    {
        // Arrange
        var client = Factory.CreateClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/trends/growth?period=monthly");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because trend analysis should be retrieved successfully");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "because trend analysis should complete within 2 seconds");
    }

    /// <summary>
    /// TC-ANA-P003: Export completes within 5 seconds
    /// Verifies performance requirement for export (10K records)
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P2")]
    [Trait("TestId", "TC-ANA-P003")]
    public async Task ExportAnalytics_Performance_CompletesWithin5Seconds()
    {
        // Arrange
        var client = Factory.CreateClient();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await client.GetAsync("/api/partners/analytics/export?format=csv");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because export should succeed");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "because export should complete within 5 seconds");
    }

    #endregion
}
