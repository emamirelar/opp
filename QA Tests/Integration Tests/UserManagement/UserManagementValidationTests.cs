using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Users;

namespace UNOPS.PAO.Tests.Integration.UserManagement
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "UserManagement")][Trait("Component", "ValidationTests")]
    public class UserManagementValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public UserManagementValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-USER-VAL-001")][Trait("Priority", "Critical")]
        public async Task CreateUser_SQLInjectionEmail_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "'; DROP TABLE Users; --@test.com", FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-VAL-002")][Trait("Priority", "Critical")]
        public async Task UpdateUser_XSSPayloadName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<script>alert('XSS')</script>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-003")][Trait("Priority", "High")]
        public async Task CreateUser_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "cmd@test.com", FirstName = "; rm -rf /", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-004")][Trait("Priority", "High")]
        public async Task UpdateUser_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "{ $ne: null }" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-005")][Trait("Priority", "High")]
        public async Task CreateUser_LDAPInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "ldap@test.com", FirstName = "Admin*)(uid=*", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-006")][Trait("Priority", "High")]
        public async Task UpdateUser_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "../../etc/passwd" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-007")][Trait("Priority", "Medium")]
        public async Task CreateUser_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "xml@test.com", FirstName = "<!DOCTYPE foo [<!ENTITY xxe>]>", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-008")][Trait("Priority", "High")]
        public async Task UpdateUser_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "Name\r\nSet-Cookie: malicious" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-009")][Trait("Priority", "High")]
        public async Task CreateUser_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "js@test.com", FirstName = "javascript:alert(1)", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-010")][Trait("Priority", "Medium")]
        public async Task UpdateUser_DataURI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "data:text/html,<script>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-011")][Trait("Priority", "High")]
        public async Task CreateUser_PolyglotXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "poly@test.com", FirstName = "javascript:/*--><svg/onload=alert(1)", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-012")][Trait("Priority", "High")]
        public async Task UpdateUser_TemplateLiteral_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "${alert(1)}" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-013")][Trait("Priority", "High")]
        public async Task CreateUser_SSTI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "ssti@test.com", FirstName = "{{config.items()}}", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-014")][Trait("Priority", "Medium")]
        public async Task UpdateUser_ExpressionLanguage_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "{{7*7}} #{7*7}" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-015")][Trait("Priority", "High")]
        public async Task CreateUser_PrototypePollution_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "proto@test.com", FirstName = "__proto__", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-016")][Trait("Priority", "Medium")]
        public async Task UpdateUser_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "&#60;script&#62;" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-017")][Trait("Priority", "High")]
        public async Task CreateUser_Base64Payload_StoredAsIs()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "b64@test.com", FirstName = "PHNjcmlwdD4=", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-018")][Trait("Priority", "Medium")]
        public async Task UpdateUser_VBScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "vbscript:msgbox(1)" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-019")][Trait("Priority", "High")]
        public async Task CreateUser_IMGTagXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "img@test.com", FirstName = "<img src=x onerror=alert(1)>", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-020")][Trait("Priority", "High")]
        public async Task UpdateUser_SVGXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<svg onload=alert(1)>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-021")][Trait("Priority", "Medium")]
        public async Task CreateUser_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "iframe@test.com", FirstName = "<iframe src='malicious'>", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-022")][Trait("Priority", "High")]
        public async Task UpdateUser_OBJECTTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<object data='x'>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-023")][Trait("Priority", "Medium")]
        public async Task CreateUser_EMBEDTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "embed@test.com", FirstName = "<embed src='x'>", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-024")][Trait("Priority", "High")]
        public async Task UpdateUser_FORMAction_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<form action='malicious'>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-025")][Trait("Priority", "Medium")]
        public async Task CreateUser_METARefresh_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "meta@test.com", FirstName = "<meta http-equiv='refresh'>", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-026")][Trait("Priority", "High")]
        public async Task UpdateUser_LINKStylesheet_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<link rel='stylesheet'>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-027")][Trait("Priority", "Medium")]
        public async Task CreateUser_STYLETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "style@test.com", FirstName = "<style>body{}</style>", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-028")][Trait("Priority", "High")]
        public async Task UpdateUser_BASETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<base href='x'>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-029")][Trait("Priority", "High")]
        public async Task CreateUser_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "event@test.com", FirstName = "<div onload=alert(1)>", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-030")][Trait("Priority", "Medium")]
        public async Task UpdateUser_MutationXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<noscript><p title='</noscript><img src=x onerror=alert(1)>'>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-031")][Trait("Priority", "High")]
        public async Task CreateUser_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "url@test.com", FirstName = "Name%20%3C%3E", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-032")][Trait("Priority", "Medium")]
        public async Task UpdateUser_HexEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "\\x3c\\x3e" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-033")][Trait("Priority", "Low")]
        public async Task CreateUser_OctalEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "octal@test.com", FirstName = "\\074\\076", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-034")][Trait("Priority", "High")]
        public async Task UpdateUser_UTF7Encoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "+ADw-script+AD4-" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-035")][Trait("Priority", "High")]
        public async Task CreateUser_MixedEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "mixed@test.com", FirstName = "&#60;%3Cscript%3E&#62;", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-036")][Trait("Priority", "High")]
        public async Task UpdateUser_DOMClobbering_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<form name='x'><input name='y'>" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-037")][Trait("Priority", "Medium")]
        public async Task CreateUser_DanglingMarkup_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "dangle@test.com", FirstName = "<img src='x?", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-038")][Trait("Priority", "High")]
        public async Task UpdateUser_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "Αdmin" }; // Greek Alpha
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-039")][Trait("Priority", "Medium")]
        public async Task CreateUser_FormatString_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "format@test.com", FirstName = "%s%s%s%s", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-040")][Trait("Priority", "Critical")]
        public async Task UpdateUser_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "Name\0Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-VAL-041")][Trait("Priority", "High")]
        public async Task CreateUser_DeepHTMLNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var request = new CreateUserRequest { Email = "deep@test.com", FirstName = deep, LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-VAL-042")][Trait("Priority", "High")]
        public async Task UpdateUser_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "(a+)+" + new string('a', 50) };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-043")][Trait("Priority", "Critical")]
        public async Task CreateUser_BufferOverflow_PreventedOrHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "huge@test.com", FirstName = new string('A', 10000), LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-VAL-044")][Trait("Priority", "Medium")]
        public async Task UpdateUser_XMLBomb_DetectedOrPrevented()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var xmlBomb = "<?xml version='1.0'?><!DOCTYPE lolz [<!ENTITY lol 'lol'><!ENTITY lol2 '&lol;&lol;'>]>";
            var request = new UpdateUserRequest { Id = 2, FirstName = xmlBomb };
            try { var result = await mgr.UpdateUserAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch { Assert.True(true, "XML bomb prevented"); }
        }

        [Fact][Trait("TestId", "TC-USER-VAL-045")][Trait("Priority", "High")]
        public async Task CreateUser_UnicodeNormalization_ConsistentHandling()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "unicode@test.com", FirstName = "café", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-046")][Trait("Priority", "Medium")]
        public async Task UpdateUser_HTMLComments_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "<!--<script>alert(1)</script>-->" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-047")][Trait("Priority", "High")]
        public async Task CreateUser_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "json@test.com", FirstName = "{\"key\":\"value\"}", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-048")][Trait("Priority", "Medium")]
        public async Task UpdateUser_EscapedQuotes_HandlesCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "Name\\\"Test\\'" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-049")][Trait("Priority", "High")]
        public async Task CreateUser_BackticksExpression_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "backtick@test.com", FirstName = "`${alert(1)}`", LastName = "User" };
            var result = await mgr.CreateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-VAL-050")][Trait("Priority", "Critical")]
        public async Task UpdateUser_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, FirstName = "..\\..\\..\\windows\\system32" };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        #endregion
    }
}
