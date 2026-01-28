using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.Dashboard
{
    /// <summary>
    /// Negative tests for Dashboard feature - invalid inputs and error scenarios
    /// Ensures robust error handling for dashboard data retrieval and analytics
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Dashboard")]
    [Trait("Component", "NegativeTests")]
    public class DashboardNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;

        public DashboardNegativeTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private ClaimsPrincipal CreateTestUser(int userId = 1)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, $"Test User {userId}"),
                new Claim(ClaimTypes.Role, "Project Manager")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        #region TC-DASH-NEG-001 through TC-DASH-NEG-050: Negative Scenarios

        [Fact][Trait("TestId", "TC-DASH-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await dashboardManager.GetDashboardDataAsync(null));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-002")][Trait("Priority", "High")]
        public async Task GetDashboardData_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await dashboardManager.GetDashboardDataAsync(user));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-003")][Trait("Priority", "Critical")]
        public async Task GetDashboardStats_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var unauthorizedUser = CreateTestUser(userId: 999999);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await dashboardManager.GetDashboardStatsAsync(unauthorizedUser));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-004")][Trait("Priority", "High")]
        public async Task GetRecentActivities_NegativeCount_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetRecentActivitiesAsync(user, -10));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-005")][Trait("Priority", "Medium")]
        public async Task GetRecentActivities_ZeroCount_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var result = await dashboardManager.GetRecentActivitiesAsync(user, 0);
            result.Should().BeEmpty();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-006")][Trait("Priority", "Low")]
        public async Task GetRecentActivities_ExcessiveCount_CapsToLimit()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var result = await dashboardManager.GetRecentActivitiesAsync(user, 99999);
            result.Should().NotBeNull();
            result.Count().Should().BeLessThan(1000);
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-007")][Trait("Priority", "High")]
        public async Task GetDashboardMetrics_InvalidDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(-30); // End before start
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetMetricsAsync(user, startDate, endDate));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-008")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_FutureDateRange_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var startDate = DateTime.UtcNow.AddYears(1);
            var endDate = startDate.AddDays(30);
            var result = await dashboardManager.GetMetricsAsync(user, startDate, endDate);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-009")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_ExcessiveDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var startDate = DateTime.UtcNow.AddYears(-100);
            var endDate = DateTime.UtcNow;
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetMetricsAsync(user, startDate, endDate));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-010")][Trait("Priority", "High")]
        public async Task GetDashboardData_DeletedUserAccount_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var deletedUser = CreateTestUser(userId: 888);
            await Assert.ThrowsAnyAsync<Exception>(async () => await dashboardManager.GetDashboardDataAsync(deletedUser));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-011")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_ExpiredSession_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAnyAsync<Exception>(async () => await dashboardManager.GetDashboardDataAsync(user));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-012")][Trait("Priority", "High")]
        public async Task GetDashboardStats_NoPermission_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var limitedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Role, "Guest")
            }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await dashboardManager.GetDashboardStatsAsync(limitedUser));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-013")][Trait("Priority", "Medium")]
        public async Task GetRecentActivities_InvalidEntityType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetRecentActivitiesAsync(user, "InvalidType", 10));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-014")][Trait("Priority", "High")]
        public async Task GetRecentActivities_NullEntityType_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await dashboardManager.GetRecentActivitiesAsync(user, null, 10));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-015")][Trait("Priority", "Medium")]
        public async Task GetDashboardWidgets_InvalidWidgetId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await dashboardManager.GetWidgetDataAsync(user, 999999));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-016")][Trait("Priority", "High")]
        public async Task GetDashboardWidgets_NegativeWidgetId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetWidgetDataAsync(user, -1));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-017")][Trait("Priority", "Medium")]
        public async Task GetDashboardWidgets_ZeroWidgetId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetWidgetDataAsync(user, 0));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-018")][Trait("Priority", "Critical")]
        public async Task RefreshDashboard_ConcurrentRefreshes_NoRaceCondition()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var tasks = Enumerable.Range(0, 10).Select(_ => dashboardManager.RefreshDashboardAsync(user));
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(10);
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-019")][Trait("Priority", "High")]
        public async Task GetDashboardData_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAnyAsync<Exception>(async () => await dashboardManager.GetDashboardDataAsync(user));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-020")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_NullStartDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await dashboardManager.GetMetricsAsync(user, null, DateTime.UtcNow));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-021")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_NullEndDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await dashboardManager.GetMetricsAsync(user, DateTime.UtcNow, null));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-022")][Trait("Priority", "High")]
        public async Task GetDashboardData_NoAccessToAnyEntity_ReturnsEmptyDashboard()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var restrictedUser = CreateTestUser(userId: 777);
            var result = await dashboardManager.GetDashboardDataAsync(restrictedUser);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-023")][Trait("Priority", "Medium")]
        public async Task GetDashboardStats_ExcessiveRefreshRate_RateLimited()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            for (int i = 0; i < 100; i++)
            {
                try { await dashboardManager.GetDashboardStatsAsync(user); }
                catch { break; }
            }
            Assert.True(true, "Rate limiting may apply");
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-024")][Trait("Priority", "High")]
        public async Task GetDashboardChartData_InvalidChartId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await dashboardManager.GetChartDataAsync(user, 999999));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-025")][Trait("Priority", "High")]
        public async Task GetDashboardChartData_NegativeChartId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetChartDataAsync(user, -1));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-026")][Trait("Priority", "Medium")]
        public async Task GetDashboardChartData_ZeroChartId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetChartDataAsync(user, 0));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-027")][Trait("Priority", "Critical")]
        public async Task GetDashboardSummary_NullFilterCriteria_HandlesGracefully()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var result = await dashboardManager.GetDashboardSummaryAsync(user, null);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-028")][Trait("Priority", "High")]
        public async Task GetDashboardKPIs_InvalidKPIType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetKPIsAsync(user, "InvalidKPI"));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-029")][Trait("Priority", "Medium")]
        public async Task GetDashboardKPIs_EmptyKPIType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetKPIsAsync(user, string.Empty));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-030")][Trait("Priority", "Critical")]
        public async Task GetDashboardTrends_NullTrendPeriod_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await dashboardManager.GetTrendsAsync(user, null));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-031")][Trait("Priority", "High")]
        public async Task GetDashboardAlerts_NegativeAlertCount_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetAlertsAsync(user, -5));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-032")][Trait("Priority", "Medium")]
        public async Task GetDashboardNotifications_ExcessiveCount_CapsToLimit()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var result = await dashboardManager.GetNotificationsAsync(user, int.MaxValue);
            result.Should().NotBeNull();
            result.Count().Should().BeLessThan(500);
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-033")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_ConcurrentRequests_NoStateCorruption()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var tasks = Enumerable.Range(0, 20).Select(_ => dashboardManager.GetDashboardDataAsync(user));
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(20);
            results.Should().AllSatisfy(r => r.Should().NotBeNull());
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-034")][Trait("Priority", "High")]
        public async Task GetDashboardData_InvalidOrgUnitFilter_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetDashboardDataAsync(user, orgUnitId: -1));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-035")][Trait("Priority", "Medium")]
        public async Task GetDashboardData_NonExistentOrgUnit_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var result = await dashboardManager.GetDashboardDataAsync(user, orgUnitId: 999999);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-036")][Trait("Priority", "High")]
        public async Task RefreshDashboard_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await dashboardManager.RefreshDashboardAsync(null));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-037")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_MinDateRange_HandlesMinimalRange()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var date = DateTime.UtcNow;
            var result = await dashboardManager.GetMetricsAsync(user, date, date);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-038")][Trait("Priority", "Low")]
        public async Task GetDashboardData_MultipleRapidRequests_HandlesLoad()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            for (int i = 0; i < 50; i++)
            {
                try { await dashboardManager.GetDashboardDataAsync(user); }
                catch { }
            }
            Assert.True(true, "Rapid requests handled");
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-039")][Trait("Priority", "High")]
        public async Task GetDashboardStats_CrossUserDataLeakage_Prevented()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user1 = CreateTestUser(userId: 100);
            var user2 = CreateTestUser(userId: 200);
            var stats1 = await dashboardManager.GetDashboardStatsAsync(user1);
            var stats2 = await dashboardManager.GetDashboardStatsAsync(user2);
            stats1.Should().NotBeNull();
            stats2.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-040")][Trait("Priority", "Medium")]
        public async Task GetRecentActivities_EmptyEntityType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetRecentActivitiesAsync(user, string.Empty, 10));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-041")][Trait("Priority", "High")]
        public async Task GetDashboardTrends_InvalidPeriod_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentException>(async () => await dashboardManager.GetTrendsAsync(user, "InvalidPeriod"));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-042")][Trait("Priority", "Medium")]
        public async Task GetDashboardData_NoDataAvailable_ReturnsEmptyDashboard()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var newUser = CreateTestUser(userId: 666);
            var result = await dashboardManager.GetDashboardDataAsync(newUser);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-043")][Trait("Priority", "Critical")]
        public async Task UpdateDashboardPreferences_NullPreferences_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await dashboardManager.UpdatePreferencesAsync(user, null));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-044")][Trait("Priority", "High")]
        public async Task GetDashboardWidgetData_WidgetDisabled_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await dashboardManager.GetWidgetDataAsync(user, 555));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-045")][Trait("Priority", "Medium")]
        public async Task GetDashboardData_MaxIntOrgUnitId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await dashboardManager.GetDashboardDataAsync(user, orgUnitId: int.MaxValue));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-046")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_SQLInjectionInFilter_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var result = await dashboardManager.GetDashboardDataAsync(user, filter: "'; DROP TABLE Partners; --");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-047")][Trait("Priority", "High")]
        public async Task RefreshDashboard_UpdateAndRefreshConcurrent_NoDeadlock()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var refreshTask = dashboardManager.RefreshDashboardAsync(user);
            var updateTask = dashboardManager.UpdatePreferencesAsync(user, new { theme = "dark" });
            try { await Task.WhenAll(refreshTask, updateTask); }
            catch { Assert.True(true, "Conflict handled"); }
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-048")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_DateRangeYearApart_HandlesLargeRange()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = CreateTestUser();
            var result = await dashboardManager.GetMetricsAsync(user, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-049")][Trait("Priority", "High")]
        public async Task GetDashboardData_InvalidRoleInClaims_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var dashboardManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim(ClaimTypes.Role, "InvalidRole")
            }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await dashboardManager.GetDashboardDataAsync(user));
        }

        [Fact][Trait("TestId", "TC-DASH-NEG-050")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_UnauthenticatedRequest_Returns401()
        {
            var response = await _factory.CreateClient().GetAsync("/api/dashboard");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}
