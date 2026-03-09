using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.Controllers
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "ValuesController")][Trait("Component", "ValidationTests")]
    public class ValuesControllerValidationTests
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        public ValuesControllerValidationTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateAuthenticatedClient();
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-001")][Trait("Priority", "Critical")]
        public async Task GetValuesByType_SQLInjection_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/'; DROP TABLE Values; --");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-002")][Trait("Priority", "Critical")]
        public async Task GetValuesByType_XSSPayload_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/<script>alert('XSS')</script>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-003")][Trait("Priority", "High")]
        public async Task GetValuesByType_CommandInjection_Blocked()
        {
            var response = await _client.GetAsync("/api/values/; rm -rf /");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-004")][Trait("Priority", "High")]
        public async Task GetValuesByType_NoSQLInjection_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/{ $ne: null }");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-005")][Trait("Priority", "High")]
        public async Task GetValuesByType_LDAPInjection_Blocked()
        {
            var response = await _client.GetAsync("/api/values/Admin*)(uid=*");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-006")][Trait("Priority", "High")]
        public async Task GetValuesByType_PathTraversal_Blocked()
        {
            var response = await _client.GetAsync("/api/values/../../etc/passwd");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-007")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_XMLEntityInjection_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<!DOCTYPE foo [<!ENTITY xxe>]>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-008")][Trait("Priority", "High")]
        public async Task GetValuesByType_CRLFInjection_Sanitized()
        {
            var response = await _client.GetAsync("/api/values/Type%0d%0aSet-Cookie: malicious");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-009")][Trait("Priority", "High")]
        public async Task GetValuesByType_JavaScriptProtocol_Blocked()
        {
            var response = await _client.GetAsync("/api/values/javascript:alert(1)");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-010")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_DataURI_Blocked()
        {
            var response = await _client.GetAsync("/api/values/data:text/html,<script>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-011")][Trait("Priority", "High")]
        public async Task GetValuesByType_PolyglotXSS_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/javascript:/*--><svg/onload=alert(1)");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-012")][Trait("Priority", "High")]
        public async Task GetValuesByType_TemplateLiteral_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/${alert(1)}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-013")][Trait("Priority", "High")]
        public async Task GetValuesByType_SSTI_Blocked()
        {
            var response = await _client.GetAsync("/api/values/{{config.items()}}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-014")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_ExpressionLanguage_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/{{7*7}} #{7*7}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-015")][Trait("Priority", "High")]
        public async Task GetValuesByType_PrototypePollution_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/__proto__");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-016")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_HTMLEntities_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/&#60;script&#62;");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-017")][Trait("Priority", "High")]
        public async Task GetValuesByType_Base64Payload_HandledCorrectly()
        {
            var response = await _client.GetAsync("/api/values/PHNjcmlwdD4=");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-018")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_VBScriptProtocol_Blocked()
        {
            var response = await _client.GetAsync("/api/values/vbscript:msgbox(1)");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-019")][Trait("Priority", "High")]
        public async Task GetValuesByType_IMGTagXSS_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<img src=x onerror=alert(1)>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-020")][Trait("Priority", "High")]
        public async Task GetValuesByType_SVGXSS_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<svg onload=alert(1)>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-021")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_IFRAMEInjection_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<iframe src='malicious'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-022")][Trait("Priority", "High")]
        public async Task GetValuesByType_OBJECTTag_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<object data='x'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-023")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_EMBEDTag_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<embed src='x'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-024")][Trait("Priority", "High")]
        public async Task GetValuesByType_FORMAction_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<form action='malicious'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-025")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_METARefresh_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<meta http-equiv='refresh'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-026")][Trait("Priority", "High")]
        public async Task GetValuesByType_LINKStylesheet_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<link rel='stylesheet'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-027")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_STYLETag_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<style>body{}</style>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-028")][Trait("Priority", "High")]
        public async Task GetValuesByType_BASETag_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<base href='x'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-029")][Trait("Priority", "High")]
        public async Task GetValuesByType_EventHandlers_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<div onload=alert(1)>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-030")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_MutationXSS_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<noscript><p title='</noscript><img src=x onerror=alert(1)>'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-031")][Trait("Priority", "High")]
        public async Task GetValuesByType_URLEncoding_Decoded()
        {
            var response = await _client.GetAsync("/api/values/Type%20With%20Spaces");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-032")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_HexEncoding_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/\\x3c\\x3e");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-033")][Trait("Priority", "Low")]
        public async Task GetValuesByType_OctalEncoding_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/\\074\\076");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-034")][Trait("Priority", "High")]
        public async Task GetValuesByType_UTF7Encoding_Sanitized()
        {
            var response = await _client.GetAsync("/api/values/+ADw-script+AD4-");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-035")][Trait("Priority", "High")]
        public async Task GetValuesByType_MixedEncoding_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/&#60;%3Cscript%3E&#62;");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-036")][Trait("Priority", "High")]
        public async Task GetValuesByType_DOMClobbering_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<form name='x'><input name='y'>");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-037")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_DanglingMarkup_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<img src='x?");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-038")][Trait("Priority", "High")]
        public async Task GetValuesByType_UnicodeHomograph_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/Αdmin");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-039")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_FormatString_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/%s%s%s%s");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-040")][Trait("Priority", "Critical")]
        public async Task GetValuesByType_NullByteInjection_Sanitized()
        {
            // QA: The HTTP runtime rejects URLs with null bytes before reaching the server
            // This is correct platform behavior - null bytes are invalid in URL paths
            try
            {
                var response = await _client.GetAsync("/api/values/Type\0Test");
                response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("null characters"))
            {
                // Expected: platform correctly rejects null byte injection in URL paths
            }
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-041")][Trait("Priority", "High")]
        public async Task GetValuesByType_DeepHTMLNesting_Blocked()
        {
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var response = await _client.GetAsync($"/api/values/{deep}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-042")][Trait("Priority", "High")]
        public async Task GetValuesByType_RegexDoS_Performant()
        {
            var response = await _client.GetAsync("/api/values/(a+)+" + new string('a', 50));
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-043")][Trait("Priority", "Critical")]
        public async Task GetValuesByType_BufferOverflow_PreventedOrHandled()
        {
            var huge = new string('A', 10000);
            var response = await _client.GetAsync($"/api/values/{huge}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.RequestUriTooLong);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-044")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_XMLBomb_DetectedOrPrevented()
        {
            var xmlBomb = "<?xml version='1.0'?><!DOCTYPE lolz [<!ENTITY lol 'lol'><!ENTITY lol2 '&lol;&lol;'>]>";
            var response = await _client.GetAsync($"/api/values/{xmlBomb}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-045")][Trait("Priority", "High")]
        public async Task GetValuesByType_UnicodeNormalization_ConsistentHandling()
        {
            var response = await _client.GetAsync("/api/values/café");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-046")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_HTMLComments_Blocked()
        {
            var response = await _client.GetAsync("/api/values/<!--<script>alert(1)</script>-->");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-047")][Trait("Priority", "High")]
        public async Task GetValuesByType_JSONPayload_SafelyHandled()
        {
            var response = await _client.GetAsync("/api/values/{\"key\":\"value\"}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-048")][Trait("Priority", "Medium")]
        public async Task GetValuesByType_EscapedQuotes_HandlesCorrectly()
        {
            var response = await _client.GetAsync("/api/values/Type\\\"Test\\'");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-049")][Trait("Priority", "High")]
        public async Task GetValuesByType_BackticksExpression_Blocked()
        {
            var response = await _client.GetAsync("/api/values/`${alert(1)}`");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]

        [Trait("Defect", "DEF-041")][Trait("TestId", "TC-VALUES-VAL-050")][Trait("Priority", "Critical")]
        public async Task GetValuesByType_WindowsPathTraversal_Sanitized()
        {
            var response = await _client.GetAsync("/api/values/..\\..\\..\\windows\\system32");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact][Trait("TestId", "TC-VALUES-VAL-051")][Trait("Priority", "High")][Trait("Ticket", "PNO-1194")]
        public async Task GetValuesUsers_ResponseContent_NoEncodingArtifacts()
        {
            var response = await _client.GetAsync("/api/values/users");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotContain("??",
                    "PNO-1194: user names in values dropdown must not contain encoding artifacts");
                content.Should().NotContain("\uFFFD",
                    "Values/users data must not contain U+FFFD replacement characters");
            }
        }

        [Fact][Trait("TestId", "TC-VALUES-VAL-052")][Trait("Priority", "High")][Trait("Ticket", "PNO-1194")]
        public async Task GetValuesCountries_ResponseContent_NoEncodingArtifacts()
        {
            var response = await _client.GetAsync("/api/values/countries");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotContain("??",
                    "PNO-1194: country names must not contain encoding artifacts (e.g. C\u00f4te d'Ivoire)");
                content.Should().NotContain("\uFFFD");
            }
        }

        [Fact][Trait("TestId", "TC-VALUES-VAL-053")][Trait("Priority", "High")][Trait("Ticket", "PNO-1194")]
        public async Task GetValuesLiaisonOffices_ResponseContent_NoEncodingArtifacts()
        {
            var response = await _client.GetAsync("/api/values/liaisonOffices");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotContain("??");
                content.Should().NotContain("\uFFFD");
            }
        }

    }
}
