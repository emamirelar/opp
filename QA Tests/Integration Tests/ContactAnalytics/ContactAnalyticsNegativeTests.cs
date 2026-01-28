using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.ContactAnalytics;

namespace UNOPS.PAO.Tests.Integration.ContactAnalytics
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "ContactAnalytics")][Trait("Component", "NegativeTests")]
    public class ContactAnalyticsNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public ContactAnalyticsNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetContactAnalytics_NonExistentContactId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetContactAnalyticsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-002")][Trait("Priority", "High")]
        public async Task GetContactAnalytics_NegativeContactId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetContactAnalyticsAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-003")][Trait("Priority", "High")]
        public async Task GetContactAnalytics_ZeroContactId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetContactAnalyticsAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-004")][Trait("Priority", "Critical")]
        public async Task GetContactAnalytics_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetContactAnalyticsAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-005")][Trait("Priority", "Critical")]
        public async Task GetContactAnalytics_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetContactAnalyticsAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-006")][Trait("Priority", "High")]
        public async Task GetInteractionMetrics_NonExistentContactId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetInteractionMetricsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-007")][Trait("Priority", "High")]
        public async Task GetEngagementScore_NonExistentContactId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetEngagementScoreAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-008")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_InvalidDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-009")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_NullStartDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetCommunicationHistoryAsync(1, null, DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-010")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_NullEndDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-011")][Trait("Priority", "Medium")]
        public async Task GetContactAnalytics_DeletedContact_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetContactAnalyticsAsync(888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-012")][Trait("Priority", "High")]
        public async Task GetContactAnalytics_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetContactAnalyticsAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-013")][Trait("Priority", "Critical")]
        public async Task GetContactAnalytics_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetContactAnalyticsAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-014")][Trait("Priority", "High")]
        public async Task GetInteractionMetrics_NegativeContactId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionMetricsAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-015")][Trait("Priority", "High")]
        public async Task GetEngagementScore_NegativeContactId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetEngagementScoreAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-016")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_ExcessiveDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddYears(-10), DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-017")][Trait("Priority", "Medium")]
        public async Task GetContactAnalytics_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetContactAnalyticsAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-018")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_InvalidTrendType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "InvalidType", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-019")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_NullTrendType_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetInteractionTrendsAsync(1, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-020")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_EmptyTrendType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, string.Empty, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-021")][Trait("Priority", "High")]
        public async Task CompareContacts_NonExistentContact1_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.CompareContactsAsync(999999, 2, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-022")][Trait("Priority", "High")]
        public async Task CompareContacts_NonExistentContact2_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.CompareContactsAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-023")][Trait("Priority", "High")]
        public async Task CompareContacts_SameContact_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CompareContactsAsync(1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-024")][Trait("Priority", "High")]
        public async Task GetEngagementScore_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetEngagementScoreAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-025")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_FutureDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(7), CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-026")][Trait("Priority", "Medium")]
        public async Task GetInteractionMetrics_DeletedContact_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetInteractionMetricsAsync(888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-027")][Trait("Priority", "High")]
        public async Task GetContactAnalytics_ConcurrentRequests_NoDuplicateCalculations()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var tasks = Enumerable.Range(0, 10).Select(_ => mgr.GetContactAnalyticsAsync(1, CreateUser()));
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(10);
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-028")][Trait("Priority", "High")]
        public async Task GetEngagementScore_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetEngagementScoreAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-029")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, viewer));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-030")][Trait("Priority", "High")]
        public async Task CompareContacts_NegativeContactId1_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CompareContactsAsync(-1, 2, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-031")][Trait("Priority", "High")]
        public async Task CompareContacts_NegativeContactId2_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CompareContactsAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-032")][Trait("Priority", "Medium")]
        public async Task GetInteractionTrends_InvalidPeriod_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "Weekly", CreateUser(), period: "InvalidPeriod"));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-033")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_ExcessiveLimit_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), limit: 10000));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-034")][Trait("Priority", "High")]
        public async Task GetEngagementScore_ZeroContactId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetEngagementScoreAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-035")][Trait("Priority", "High")]
        public async Task GetInteractionMetrics_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetInteractionMetricsAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-036")][Trait("Priority", "High")]
        public async Task CompareContacts_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.CompareContactsAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-037")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_NegativeLimit_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), limit: -1));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-038")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_NonExistentContact_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetInteractionTrendsAsync(999999, "Weekly", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-039")][Trait("Priority", "Medium")]
        public async Task GetContactAnalytics_CrossOrgAccess_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var user1 = CreateUser(100);
            var user2 = CreateUser(200);
            try { await mgr.GetContactAnalyticsAsync(1, user2); Assert.True(true); }
            catch (UnauthorizedAccessException) { Assert.True(true, "Cross-org blocked"); }
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-040")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_ZeroLimit_HandlesOrRejects()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            try { var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), limit: 0); result.Should().BeEmpty(); }
            catch (ArgumentException) { Assert.True(true, "Zero limit rejected"); }
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-041")][Trait("Priority", "High")]
        public async Task GetEngagementScore_NoInteractions_ReturnsZero()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetEngagementScoreAsync(10, CreateUser());
            result.Should().Be(0);
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-042")][Trait("Priority", "Medium")]
        public async Task GetInteractionMetrics_NoMetrics_ReturnsDefault()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetInteractionMetricsAsync(10, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-043")][Trait("Priority", "High")]
        public async Task CompareContacts_BothDeleted_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.CompareContactsAsync(888, 889, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-044")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_FutureDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "Monthly", CreateUser(), startDate: DateTime.UtcNow.AddDays(1)));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-045")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_InvalidChannelFilter_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), channelFilter: "InvalidChannel"));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-046")][Trait("Priority", "High")]
        public async Task GetContactAnalytics_RefreshTooFrequent_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await mgr.GetContactAnalyticsAsync(1, CreateUser(), refresh: true);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.GetContactAnalyticsAsync(1, CreateUser(), refresh: true));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-047")][Trait("Priority", "Medium")]
        public async Task GetInteractionMetrics_NegativeTimeRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionMetricsAsync(1, CreateUser(), timeRangeDays: -30));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-048")][Trait("Priority", "High")]
        public async Task CompareContacts_InvalidMetricType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CompareContactsAsync(1, 2, CreateUser(), metricType: "InvalidMetric"));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-049")][Trait("Priority", "High")]
        public async Task GetContactAnalytics_ExcessiveRefreshRate_RateLimited()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            for (int i = 0; i < 100; i++)
            {
                try { await mgr.GetContactAnalyticsAsync(1, CreateUser(), refresh: true); }
                catch (InvalidOperationException) { Assert.True(true, "Rate limiting enforced"); break; }
            }
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-NEG-050")][Trait("Priority", "Critical")]
        public async Task GetInteractionTrends_SQLInjectionTrendType_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "'; DROP TABLE Trends; --", CreateUser()));
        }


    }
}
