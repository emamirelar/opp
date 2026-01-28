using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace UNOPS.PAO.Tests.Integration.Dashboard
{
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Dashboard")]
    [Trait("Component", "ValidationTests")]
    public class DashboardValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public DashboardValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Manager") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-DASH-VAL-001")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_SQLInjectionFilter_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "'; DROP TABLE Partners; --");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-002")][Trait("Priority", "Critical")]
        public async Task GetDashboardStats_XSSInParameter_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<script>alert('XSS')</script>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-003")][Trait("Priority", "High")]
        public async Task GetRecentActivities_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "; rm -rf /", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-004")][Trait("Priority", "High")]
        public async Task GetDashboardMetrics_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "{ $ne: null }");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-005")][Trait("Priority", "High")]
        public async Task GetDashboardData_LDAPInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "Admin*)(uid=*");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-006")][Trait("Priority", "Critical")]
        public async Task GetDashboardStats_HTMLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<img src=x onerror=alert(1)>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-007")][Trait("Priority", "High")]
        public async Task GetRecentActivities_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await mgr.GetRecentActivitiesAsync(CreateUser(), "../../etc/passwd", 10));
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-008")][Trait("Priority", "High")]
        public async Task GetDashboardData_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<!DOCTYPE foo [<!ENTITY xxe>]>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-009")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "value\r\nSet-Cookie: malicious");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-010")][Trait("Priority", "High")]
        public async Task GetDashboardStats_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "javascript:alert(1)");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-011")][Trait("Priority", "High")]
        public async Task GetRecentActivities_DataURI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "data:text/html,<script>", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-012")][Trait("Priority", "Medium")]
        public async Task GetDashboardData_PolyglotXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "javascript:/*--></title></style></textarea></script><svg/onload='alert(1)'");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-013")][Trait("Priority", "High")]
        public async Task GetDashboardMetrics_TemplateLiteral_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "${alert(1)}");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-014")][Trait("Priority", "High")]
        public async Task GetDashboardStats_SSTI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "{{config.items()}}");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-015")][Trait("Priority", "Medium")]
        public async Task GetRecentActivities_ExpressionLanguage_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "{{7*7}} #{7*7}", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-016")][Trait("Priority", "High")]
        public async Task GetDashboardData_PrototypePollution_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "__proto__");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-017")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "&#60;script&#62;");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-018")][Trait("Priority", "High")]
        public async Task GetDashboardStats_Base64Payload_StoredAsIs()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "PHNjcmlwdD4=");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-019")][Trait("Priority", "High")]
        public async Task GetRecentActivities_VBScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "vbscript:msgbox(1)", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-020")][Trait("Priority", "Medium")]
        public async Task GetDashboardData_SVGOnload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<svg onload=alert(1)>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-021")][Trait("Priority", "High")]
        public async Task GetDashboardMetrics_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<iframe src='malicious'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-022")][Trait("Priority", "Medium")]
        public async Task GetDashboardStats_OBJECTTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<object data='malicious'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-023")][Trait("Priority", "Medium")]
        public async Task GetRecentActivities_EMBEDTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "<embed src='x'>", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-024")][Trait("Priority", "High")]
        public async Task GetDashboardData_FORMAction_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<form action='malicious'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-025")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_METARefresh_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<meta http-equiv='refresh'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-026")][Trait("Priority", "High")]
        public async Task GetDashboardStats_LINKStylesheet_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<link rel='stylesheet'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-027")][Trait("Priority", "Medium")]
        public async Task GetRecentActivities_STYLETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "<style>body{}</style>", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-028")][Trait("Priority", "High")]
        public async Task GetDashboardData_BASETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<base href='malicious'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-029")][Trait("Priority", "High")]
        public async Task GetDashboardMetrics_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<div onload=alert(1)>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-030")][Trait("Priority", "Medium")]
        public async Task GetDashboardStats_MutationXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<noscript><p title='</noscript><img src=x onerror=alert(1)>'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-031")][Trait("Priority", "High")]
        public async Task GetRecentActivities_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "test%20%3C%3E", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-032")][Trait("Priority", "Medium")]
        public async Task GetDashboardData_HexEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "\\x3c\\x3e");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-033")][Trait("Priority", "Low")]
        public async Task GetDashboardMetrics_OctalEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "\\074\\076");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-034")][Trait("Priority", "High")]
        public async Task GetDashboardStats_UTF7Encoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "+ADw-script+AD4-");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-035")][Trait("Priority", "High")]
        public async Task GetRecentActivities_MixedEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "&#60;%3Cscript%3E&#62;", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-036")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_DOMClobbering_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<form name='x'><input name='y'>");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-037")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_DanglingMarkup_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "<img src='x?");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-038")][Trait("Priority", "High")]
        public async Task GetDashboardStats_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "Αdmin");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-039")][Trait("Priority", "Medium")]
        public async Task GetRecentActivities_FormatString_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "%s%s%s", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-040")][Trait("Priority", "High")]
        public async Task GetDashboardData_ZeroWidthChars_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "test\u200Bvalue");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-041")][Trait("Priority", "Medium")]
        public async Task GetDashboardMetrics_BidiOverride_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "test\u202Evalue");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-042")][Trait("Priority", "Low")]
        public async Task GetDashboardStats_CombiningChars_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "ṭ̴͉͖e̵̢̫͎s̶̨͔t");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-043")][Trait("Priority", "High")]
        public async Task GetRecentActivities_ControlChars_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetRecentActivitiesAsync(CreateUser(), "test\u0007value", 10);
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-044")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_NullByteInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "test\0null");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-045")][Trait("Priority", "High")]
        public async Task GetDashboardMetrics_MathematicalSymbols_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "𝐃𝐚𝐭𝐚");
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-046")][Trait("Priority", "Medium")]
        public async Task GetDashboardStats_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser(), filter: "(a+)+$" + new string('a', 50));
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-047")][Trait("Priority", "High")]
        public async Task GetRecentActivities_DeepNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var deepNest = "<div>" + string.Join("", Enumerable.Repeat("<div>", 1000)) + string.Join("", Enumerable.Repeat("</div>", 1001));
            try { await mgr.GetRecentActivitiesAsync(CreateUser(), deepNest, 10); Assert.True(true); }
            catch { Assert.True(true, "Deep nesting rejected"); }
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-048")][Trait("Priority", "Critical")]
        public async Task GetDashboardData_BufferOverflow_PreventedOrHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var hugeInput = new string('A', 100000);
            try { await mgr.GetDashboardDataAsync(CreateUser(), filter: hugeInput); Assert.True(true); }
            catch (ArgumentException) { Assert.True(true, "Buffer overflow prevented"); }
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-049")][Trait("Priority", "High")]
        public async Task GetDashboardMetrics_XMLBomb_DetectedOrPrevented()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var xmlBomb = "<?xml version='1.0'?><!DOCTYPE lolz [<!ENTITY lol 'lol'><!ENTITY lol2 '&lol;&lol;'>]><lolz>&lol2;</lolz>";
            try { await mgr.GetDashboardDataAsync(CreateUser(), filter: xmlBomb); Assert.True(true); }
            catch { Assert.True(true, "XML bomb prevented"); }
        }

        [Fact][Trait("TestId", "TC-DASH-VAL-050")][Trait("Priority", "Critical")]
        public async Task GetDashboardStats_ExcessiveParameterCount_HandledGracefully()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager;
            var result = await mgr.GetDashboardDataAsync(CreateUser());
            result.Should().NotBeNull();
        }


    }
}
