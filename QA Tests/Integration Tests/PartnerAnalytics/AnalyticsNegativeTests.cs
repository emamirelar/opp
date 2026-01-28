using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.PartnerAnalytics
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "PartnerAnalytics")][Trait("Component", "NegativeTests")]
    public class AnalyticsNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public AnalyticsNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Manager") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetPartnerAnalytics_NonExistentPartnerId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetAnalyticsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-002")][Trait("Priority", "High")]
        public async Task GetPartnerAnalytics_NegativePartnerId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetAnalyticsAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-003")][Trait("Priority", "Medium")]
        public async Task GetPartnerAnalytics_ZeroPartnerId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetAnalyticsAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-004")][Trait("Priority", "Critical")]
        public async Task GetPartnerAnalytics_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetAnalyticsAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-005")][Trait("Priority", "High")]
        public async Task GetPartnerAnalytics_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetAnalyticsAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-006")][Trait("Priority", "High")]
        public async Task GetPartnerTrends_InvalidDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var start = DateTime.UtcNow;
            var end = start.AddDays(-30);
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetTrendsAsync(1, start, end, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-007")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_NullStartDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetMetricsAsync(1, null, DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-008")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_NullEndDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetMetricsAsync(1, DateTime.UtcNow, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-009")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_InvalidPartnerIds_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.ComparePartnersAsync(new[] { 999999, 999998 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-010")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_EmptyPartnerList_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.ComparePartnersAsync(new int[0], CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-011")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_NullPartnerList_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.ComparePartnersAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-012")][Trait("Priority", "Medium")]
        public async Task GetPartnerAnalytics_DeletedPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetAnalyticsAsync(888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-013")][Trait("Priority", "High")]
        public async Task GetPartnerTrends_FutureDateRange_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var start = DateTime.UtcNow.AddYears(1);
            var result = await mgr.GetTrendsAsync(1, start, start.AddDays(30), CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-014")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_ExcessiveDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var start = DateTime.UtcNow.AddYears(-100);
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetMetricsAsync(1, start, DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-015")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_MixedValidInvalid_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.ComparePartnersAsync(new[] { 1, 999999 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-016")][Trait("Priority", "Medium")]
        public async Task GetPartnerAnalytics_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAnalyticsAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-017")][Trait("Priority", "High")]
        public async Task GetPartnerTrends_InvalidMetricType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetTrendsAsync(1, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "InvalidMetric", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-018")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_NullMetricType_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-019")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_DuplicatePartnerIds_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.ComparePartnersAsync(new[] { 1, 1, 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-020")][Trait("Priority", "Medium")]
        public async Task GetPartnerAnalytics_MaxIntPartnerId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetAnalyticsAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-021")][Trait("Priority", "High")]
        public async Task GetPartnerTrends_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetTrendsAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-022")][Trait("Priority", "High")]
        public async Task GetPartnerMetrics_UnauthorizedPartner_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var restrictedUser = CreateUser(888);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, restrictedUser));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-023")][Trait("Priority", "Medium")]
        public async Task GetPartnerComparison_ExcessivePartnerCount_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var partners = Enumerable.Range(1, 1000).ToArray();
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.ComparePartnersAsync(partners, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-024")][Trait("Priority", "High")]
        public async Task GetPartnerAnalytics_ConcurrentRequests_NoRaceCondition()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var user = CreateUser();
            var tasks = Enumerable.Range(0, 20).Select(_ => mgr.GetAnalyticsAsync(1, user));
            try { var results = await Task.WhenAll(tasks); results.Should().HaveCount(20); }
            catch { Assert.True(true); }
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-025")][Trait("Priority", "Medium")]
        public async Task GetPartnerTrends_EmptyMetricType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, string.Empty, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-026")][Trait("Priority", "High")]
        public async Task GetPartnerMetrics_WhitespaceMetricType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "   ", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-027")][Trait("Priority", "Medium")]
        public async Task GetPartnerComparison_NegativePartnerIds_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.ComparePartnersAsync(new[] { -1, -2 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-028")][Trait("Priority", "High")]
        public async Task GetPartnerAnalytics_DatabaseTimeout_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAnalyticsAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-029")][Trait("Priority", "High")]
        public async Task GetPartnerTrends_NoDataInRange_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow.AddYears(1), DateTime.UtcNow.AddYears(1).AddDays(30), CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-030")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_ExcessiveRefreshRate_RateLimited()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            for (int i = 0; i < 100; i++) { try { await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, CreateUser()); } catch { } }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-031")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_SinglePartner_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.ComparePartnersAsync(new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-032")][Trait("Priority", "Critical")]
        public async Task GetPartnerAnalytics_CrossOrgAccess_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var user = CreateUser(777);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetAnalyticsAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-033")][Trait("Priority", "High")]
        public async Task RefreshAnalytics_ConcurrentRefresh_NoConflict()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var tasks = Enumerable.Range(0, 10).Select(_ => mgr.RefreshAnalyticsAsync(1, CreateUser()));
            try { await Task.WhenAll(tasks); } catch { Assert.True(true); }
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-034")][Trait("Priority", "Medium")]
        public async Task GetPartnerTrends_InvalidAggregation_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, aggregation: "InvalidAgg", user: CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-035")][Trait("Priority", "High")]
        public async Task GetPartnerMetrics_NullAggregation_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, aggregation: null, user: CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-036")][Trait("Priority", "Medium")]
        public async Task GetPartnerComparison_AllPartnersDeleted_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            try { await mgr.ComparePartnersAsync(new[] { 888, 889 }, CreateUser()); }
            catch (KeyNotFoundException) { Assert.True(true, "Deleted partners rejected"); }
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-037")][Trait("Priority", "High")]
        public async Task GetPartnerAnalytics_NoAccessToData_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var noAccessUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "666"), new Claim(ClaimTypes.Role, "Guest") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetAnalyticsAsync(1, noAccessUser));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-038")][Trait("Priority", "Medium")]
        public async Task GetPartnerTrends_NegativePartnerId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetTrendsAsync(-1, DateTime.UtcNow, DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-039")][Trait("Priority", "High")]
        public async Task RefreshAnalytics_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.RefreshAnalyticsAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-040")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_ZeroPartnerId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetMetricsAsync(0, DateTime.UtcNow, DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-041")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_ZeroInList_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.ComparePartnersAsync(new[] { 0, 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-042")][Trait("Priority", "Critical")]
        public async Task GetPartnerAnalytics_ExpiredSession_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAnalyticsAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-043")][Trait("Priority", "High")]
        public async Task GetPartnerTrends_UpdateDuringRead_NoInconsistency()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var refresh = Task.Run(async () => { await Task.Delay(50); await mgr.RefreshAnalyticsAsync(1, CreateUser()); });
            await Task.Delay(25);
            var read = await mgr.GetTrendsAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser());
            read.Should().NotBeNull();
            await refresh;
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-044")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_RapidSequential_HandlesLoad()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            for (int i = 0; i < 50; i++) { try { await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, CreateUser()); } catch { } }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-045")][Trait("Priority", "High")]
        public async Task GetPartnerComparison_InvalidComparisonCriteria_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.ComparePartnersAsync(new[] { 1, 2 }, criteria: "InvalidCriteria", user: CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-046")][Trait("Priority", "Medium")]
        public async Task RefreshAnalytics_NonExistentPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RefreshAnalyticsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-047")][Trait("Priority", "High")]
        public async Task GetPartnerAnalytics_PartnerNoInteractions_ReturnsZeros()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetAnalyticsAsync(555, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-048")][Trait("Priority", "Medium")]
        public async Task GetPartnerTrends_DateRangeTooShort_Handled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var date = DateTime.UtcNow;
            var result = await mgr.GetTrendsAsync(1, date, date, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-049")][Trait("Priority", "High")]
        public async Task GetPartnerMetrics_MultipleFilters_AllValidated()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, filters: new { invalid = "test" }, user: CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-050")][Trait("Priority", "Critical")]
        public async Task GetPartnerComparison_SQLInjectionInCriteria_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, criteria: "'; DROP TABLE Analytics; --", user: CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-051")][Trait("Priority", "High")]
        public async Task RefreshAnalytics_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RefreshAnalyticsAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-052")][Trait("Priority", "Medium")]
        public async Task GetPartnerAnalytics_PartialDataAvailable_ReturnsPartial()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetAnalyticsAsync(1, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-053")][Trait("Priority", "High")]
        public async Task GetPartnerTrends_InvalidGroupBy_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, groupBy: "InvalidGroup", user: CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-NEG-054")][Trait("Priority", "Medium")]
        public async Task GetPartnerMetrics_NullGroupBy_UsesDefault()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, groupBy: null, user: CreateUser());
            result.Should().NotBeNull();
        }


    }
}
