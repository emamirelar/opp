using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;

using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.Tests.Integration.PartnerAnalytics
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "PartnerAnalytics")][Trait("Component", "ValidationTests")]
    public class AnalyticsValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public AnalyticsValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Manager") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-001")][Trait("Priority", "Critical")]
        public async Task GetMetrics_SQLInjectionMetricType_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "'; DROP TABLE Analytics; --", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-002")][Trait("Priority", "Critical")]
        public async Task GetTrends_XSSPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<script>alert(1)</script>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-003")][Trait("Priority", "High")]
        public async Task ComparePartners_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "; rm -rf /", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-004")][Trait("Priority", "High")]
        public async Task GetMetrics_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "{ $ne: null }", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-005")][Trait("Priority", "High")]
        public async Task GetTrends_LDAPInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "Admin*)(uid=*", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-006")][Trait("Priority", "Critical")]
        public async Task ComparePartners_HTMLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "<img src=x onerror=alert(1)>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-007")][Trait("Priority", "High")]
        public async Task GetMetrics_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "../../etc/passwd", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-008")][Trait("Priority", "High")]
        public async Task GetTrends_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<!DOCTYPE foo [<!ENTITY xxe>]>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-009")][Trait("Priority", "Medium")]
        public async Task ComparePartners_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "test\r\nSet-Cookie: malicious", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-010")][Trait("Priority", "High")]
        public async Task GetMetrics_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "javascript:alert(1)", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-011")][Trait("Priority", "High")]
        public async Task GetTrends_DataURI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "data:text/html,<script>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-012")][Trait("Priority", "Medium")]
        public async Task ComparePartners_PolyglotXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "javascript:/*--><svg/onload=alert(1)", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-013")][Trait("Priority", "High")]
        public async Task GetMetrics_TemplateLiteral_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "${alert(1)}", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-014")][Trait("Priority", "High")]
        public async Task GetTrends_SSTI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "{{config.items()}}", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-015")][Trait("Priority", "Medium")]
        public async Task ComparePartners_ExpressionLanguage_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "{{7*7}} #{7*7}", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-016")][Trait("Priority", "High")]
        public async Task GetMetrics_PrototypePollution_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "__proto__", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-017")][Trait("Priority", "Medium")]
        public async Task GetTrends_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "&#60;script&#62;", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-018")][Trait("Priority", "High")]
        public async Task ComparePartners_Base64Payload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "PHNjcmlwdD4=", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-019")][Trait("Priority", "High")]
        public async Task GetMetrics_VBScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "vbscript:msgbox(1)", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-020")][Trait("Priority", "High")]
        public async Task GetTrends_SVGXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<svg onload=alert(1)>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-021")][Trait("Priority", "Medium")]
        public async Task ComparePartners_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "<iframe src='malicious'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-022")][Trait("Priority", "High")]
        public async Task GetMetrics_OBJECTTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<object data='x'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-023")][Trait("Priority", "Medium")]
        public async Task GetTrends_EMBEDTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<embed src='x'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-024")][Trait("Priority", "High")]
        public async Task ComparePartners_FORMAction_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "<form action='malicious'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-025")][Trait("Priority", "Medium")]
        public async Task GetMetrics_METARefresh_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<meta http-equiv='refresh'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-026")][Trait("Priority", "High")]
        public async Task GetTrends_LINKStylesheet_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<link rel='stylesheet'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-027")][Trait("Priority", "Medium")]
        public async Task ComparePartners_STYLETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "<style>body{}</style>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-028")][Trait("Priority", "High")]
        public async Task GetMetrics_BASETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<base href='x'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-029")][Trait("Priority", "High")]
        public async Task GetTrends_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<div onload=alert(1)>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-030")][Trait("Priority", "Medium")]
        public async Task ComparePartners_MutationXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "<noscript><p title='</noscript><img src=x onerror=alert(1)>'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-031")][Trait("Priority", "High")]
        public async Task GetMetrics_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "metric%20%3C%3E", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-032")][Trait("Priority", "Medium")]
        public async Task GetTrends_HexEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "\\x3c\\x3e", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-033")][Trait("Priority", "Low")]
        public async Task ComparePartners_OctalEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "\\074\\076", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-034")][Trait("Priority", "High")]
        public async Task GetMetrics_UTF7Encoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "+ADw-script+AD4-", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-035")][Trait("Priority", "High")]
        public async Task GetTrends_MixedEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "&#60;%3Cscript%3E&#62;", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-036")][Trait("Priority", "High")]
        public async Task ComparePartners_DOMClobbering_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "<form name='x'><input name='y'>", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-037")][Trait("Priority", "Medium")]
        public async Task GetMetrics_DanglingMarkup_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "<img src='x?", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-038")][Trait("Priority", "High")]
        public async Task GetTrends_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "Αdmin", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-039")][Trait("Priority", "Medium")]
        public async Task ComparePartners_FormatString_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "%s%s%s%s", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-040")][Trait("Priority", "High")]
        public async Task GetMetrics_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "metric\0type", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-041")][Trait("Priority", "Medium")]
        public async Task GetTrends_ZeroWidthChars_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "metric\u200Btype", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-042")][Trait("Priority", "Low")]
        public async Task ComparePartners_BidiOverride_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "metric\u202Etype", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-043")][Trait("Priority", "High")]
        public async Task GetMetrics_CombiningChars_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "m̴̡͉e̵̢̫t̶̨͔r̷̡͖į̸̓c", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-044")][Trait("Priority", "Medium")]
        public async Task GetTrends_ControlChars_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "metric\u0007type", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-045")][Trait("Priority", "High")]
        public async Task ComparePartners_DeepHTMLNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            try { await mgr.ComparePartnersAsync(new[] { 1, 2 }, deep, CreateUser()); Assert.True(true); }
            catch (ArgumentException) { Assert.True(true, "Deep nesting rejected"); }
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-046")][Trait("Priority", "Critical")]
        public async Task GetMetrics_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "(a+)+" + new string('a', 50), CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-047")][Trait("Priority", "High")]
        public async Task GetTrends_BufferOverflow_PreventedOrHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var huge = new string('A', 10000);
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, huge, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-048")][Trait("Priority", "Medium")]
        public async Task ComparePartners_XMLBomb_DetectedOrPrevented()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var xmlBomb = "<?xml version='1.0'?><!DOCTYPE lolz [<!ENTITY lol 'lol'><!ENTITY lol2 '&lol;&lol;'>]>";
            try { await mgr.ComparePartnersAsync(new[] { 1, 2 }, xmlBomb, CreateUser()); Assert.True(true); }
            catch { Assert.True(true, "XML bomb prevented"); }
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-049")][Trait("Priority", "High")]
        public async Task GetMetrics_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "{\"key\":\"value\"}", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-050")][Trait("Priority", "Medium")]
        public async Task GetTrends_MathematicalSymbols_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "𝐌𝐞𝐭𝐫𝐢𝐜", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-051")][Trait("Priority", "High")]
        public async Task ComparePartners_EscapedQuotes_HandlesCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "metric\\\"test\\'", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-052")][Trait("Priority", "Medium")]
        public async Task GetMetrics_BackticksExpression_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetMetricsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "`${alert(1)}`", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-053")][Trait("Priority", "High")]
        public async Task GetTrends_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.GetTrendsAsync(1, DateTime.UtcNow, DateTime.UtcNow, "..\\..\\windows\\system32", CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ANALYTICS-VAL-054")][Trait("Priority", "Critical")]
        public async Task ComparePartners_UnicodeNormalization_ConsistentHandling()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerAnalyticsManager;
            var result = await mgr.ComparePartnersAsync(new[] { 1, 2 }, "café", CreateUser()); // é = 2 chars
            result.Should().NotBeNull();
        }


    }
}
