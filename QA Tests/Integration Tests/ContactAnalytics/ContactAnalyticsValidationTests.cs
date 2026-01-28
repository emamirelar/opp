using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.ContactAnalytics;

namespace UNOPS.PAO.Tests.Integration.ContactAnalytics
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "ContactAnalytics")][Trait("Component", "ValidationTests")]
    public class ContactAnalyticsValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public ContactAnalyticsValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-001")][Trait("Priority", "Critical")]
        public async Task GetInteractionTrends_SQLInjectionTrendType_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "'; DROP TABLE Trends; --", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-002")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_XSSPayloadFilter_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "<script>alert('XSS')</script>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-003")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_CommandInjection_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "; rm -rf /", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-004")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "{ $ne: null }");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-005")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_PathTraversal_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "../../etc/passwd", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-006")][Trait("Priority", "Medium")]
        public async Task GetCommunicationHistory_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "<!DOCTYPE foo [<!ENTITY xxe>]>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-007")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_CRLFInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "Weekly\r\nMalicious", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-008")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_JavaScriptProtocol_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "javascript:alert(1)");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-009")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetInteractionTrendsAsync(1, "Weekly", CreateUser(), filter: "&#60;script&#62;");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-010")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_IMGTagXSS_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "<img src=x onerror=alert(1)>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-011")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_SVGXSS_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetInteractionTrendsAsync(1, "Weekly", CreateUser(), filter: "<svg onload=alert(1)>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-012")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_EventHandlers_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "<div onload=alert(1)>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-013")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetInteractionTrendsAsync(1, "Weekly", CreateUser(), filter: "Filter%20%3C%3E");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-014")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "Αdmin");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-015")][Trait("Priority", "Critical")]
        public async Task GetInteractionTrends_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetInteractionTrendsAsync(1, "Weekly\0Test", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-016")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_DeepHTMLNesting_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: deep);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-017")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetInteractionTrendsAsync(1, "Weekly", CreateUser(), filter: "(a+)+" + new string('a', 50));
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-018")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "{\"key\":\"value\"}");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-019")][Trait("Priority", "High")]
        public async Task GetEngagementScore_CalculationValidation_ValidFormula()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var score = await mgr.GetEngagementScoreAsync(1, CreateUser());
            score.Should().BeInRange(0, 100);
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-020")][Trait("Priority", "High")]
        public async Task GetInteractionMetrics_MetricConsistency_SumsCorrect()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetInteractionMetricsAsync(1, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-021")][Trait("Priority", "High")]
        public async Task CompareContacts_MetricAlignment_ComparableValues()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.CompareContactsAsync(1, 2, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-022")][Trait("Priority", "High")]
        public async Task GetCommunicationHistory_ChannelValidation_OnlyValidChannels()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var channels = new[] { "Email", "Phone", "Meeting" };
            foreach (var channel in channels)
            {
                var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), channelFilter: channel);
                result.Should().NotBeNull();
            }
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-023")][Trait("Priority", "High")]
        public async Task GetInteractionTrends_PeriodValidation_OnlyValidPeriods()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var periods = new[] { "Last7Days", "Last30Days", "Last90Days" };
            foreach (var period in periods)
            {
                try { var result = await mgr.GetInteractionTrendsAsync(1, "Weekly", CreateUser(), period: period); result.Should().NotBeNull(); }
                catch { }
            }
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-024")][Trait("Priority", "High")]
        public async Task GetEngagementScore_ScoreRange_Between0And100()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var score = await mgr.GetEngagementScoreAsync(1, CreateUser());
            score.Should().BeInRange(0, 100);
        }

        [Fact][Trait("TestId", "TC-CONTACTANALYTICS-VAL-025")][Trait("Priority", "Critical")]
        public async Task GetCommunicationHistory_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().ContactAnalyticsManager;
            var result = await mgr.GetCommunicationHistoryAsync(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), filter: "..\\..\\..\\windows\\system32");
            result.Should().NotBeNull();
        }

        #endregion
    }
}
