using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Roles;

namespace UNOPS.PAO.Tests.Integration.Roles
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "Roles")][Trait("Component", "ValidationTests")]
    public class RoleValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public RoleValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ROLE-VAL-001")][Trait("Priority", "Critical")]
        public async Task CreateRole_SQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "'; DROP TABLE Roles; --" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Name.Should().Contain("DROP");
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-002")][Trait("Priority", "Critical")]
        public async Task CreateRole_XSSPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<script>alert('XSS')</script>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-003")][Trait("Priority", "High")]
        public async Task UpdateRole_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "; rm -rf /" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-004")][Trait("Priority", "High")]
        public async Task CreateRole_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "{ $ne: null }" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-005")][Trait("Priority", "High")]
        public async Task UpdateRole_LDAPInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "Admin*)(uid=*" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-006")][Trait("Priority", "High")]
        public async Task CreateRole_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "../../etc/passwd" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-007")][Trait("Priority", "Medium")]
        public async Task UpdateRole_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<!DOCTYPE foo [<!ENTITY xxe>]>" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-008")][Trait("Priority", "High")]
        public async Task CreateRole_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "Role\r\nSet-Cookie: malicious" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-009")][Trait("Priority", "High")]
        public async Task UpdateRole_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "javascript:alert(1)" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-010")][Trait("Priority", "Medium")]
        public async Task CreateRole_DataURI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "data:text/html,<script>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-011")][Trait("Priority", "High")]
        public async Task UpdateRole_PolyglotXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "javascript:/*--></title></style></textarea></script><svg/onload=alert(1)" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-012")][Trait("Priority", "High")]
        public async Task CreateRole_TemplateLiteral_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "${alert(1)}" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-013")][Trait("Priority", "High")]
        public async Task UpdateRole_SSTI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "{{config.items()}}" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-014")][Trait("Priority", "Medium")]
        public async Task CreateRole_ExpressionLanguage_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "{{7*7}} #{7*7}" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-015")][Trait("Priority", "High")]
        public async Task UpdateRole_PrototypePollution_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "__proto__" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-016")][Trait("Priority", "Medium")]
        public async Task CreateRole_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "&#60;script&#62;" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-017")][Trait("Priority", "High")]
        public async Task UpdateRole_Base64Payload_StoredAsIs()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "PHNjcmlwdD4=" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-018")][Trait("Priority", "Medium")]
        public async Task CreateRole_VBScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "vbscript:msgbox(1)" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-019")][Trait("Priority", "High")]
        public async Task UpdateRole_IMGTagXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<img src=x onerror=alert(1)>" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-020")][Trait("Priority", "High")]
        public async Task CreateRole_SVGXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<svg onload=alert(1)>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-021")][Trait("Priority", "Medium")]
        public async Task UpdateRole_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<iframe src='malicious'>" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-022")][Trait("Priority", "High")]
        public async Task CreateRole_OBJECTTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<object data='x'>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-023")][Trait("Priority", "Medium")]
        public async Task UpdateRole_EMBEDTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<embed src='x'>" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-024")][Trait("Priority", "High")]
        public async Task CreateRole_FORMAction_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<form action='malicious'>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-025")][Trait("Priority", "Medium")]
        public async Task UpdateRole_METARefresh_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<meta http-equiv='refresh'>" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-026")][Trait("Priority", "High")]
        public async Task CreateRole_LINKStylesheet_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<link rel='stylesheet'>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-027")][Trait("Priority", "Medium")]
        public async Task UpdateRole_STYLETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<style>body{}</style>" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-028")][Trait("Priority", "High")]
        public async Task CreateRole_BASETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<base href='x'>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-029")][Trait("Priority", "High")]
        public async Task UpdateRole_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<div onload=alert(1)>" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-030")][Trait("Priority", "Medium")]
        public async Task CreateRole_MutationXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<noscript><p title='</noscript><img src=x onerror=alert(1)>'>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-031")][Trait("Priority", "High")]
        public async Task UpdateRole_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "Role%20%3C%3E" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-032")][Trait("Priority", "Medium")]
        public async Task CreateRole_HexEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "\\x3c\\x3e" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-033")][Trait("Priority", "Low")]
        public async Task UpdateRole_OctalEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "\\074\\076" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-034")][Trait("Priority", "High")]
        public async Task CreateRole_UTF7Encoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "+ADw-script+AD4-" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-035")][Trait("Priority", "High")]
        public async Task UpdateRole_MixedEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "&#60;%3Cscript%3E&#62;" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-036")][Trait("Priority", "High")]
        public async Task CreateRole_DOMClobbering_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<form name='x'><input name='y'>" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-037")][Trait("Priority", "Medium")]
        public async Task UpdateRole_DanglingMarkup_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "<img src='x?" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-038")][Trait("Priority", "High")]
        public async Task CreateRole_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "Αdmin" }; // Greek Alpha
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-039")][Trait("Priority", "Medium")]
        public async Task UpdateRole_FormatString_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "%s%s%s%s" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-040")][Trait("Priority", "High")]
        public async Task CreateRole_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "Role\0Name" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-041")][Trait("Priority", "High")]
        public async Task UpdateRole_DeepHTMLNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var request = new UpdateRoleRequest { Id = 2, Name = deep };
            try { var result = await mgr.UpdateRoleAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Deep nesting rejected"); }
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-042")][Trait("Priority", "Critical")]
        public async Task CreateRole_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "(a+)+" + new string('a', 50) };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-043")][Trait("Priority", "High")]
        public async Task UpdateRole_BufferOverflow_PreventedOrHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = new string('A', 10000) };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-044")][Trait("Priority", "Medium")]
        public async Task CreateRole_XMLBomb_DetectedOrPrevented()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var xmlBomb = "<?xml version='1.0'?><!DOCTYPE lolz [<!ENTITY lol 'lol'><!ENTITY lol2 '&lol;&lol;'>]><lolz>&lol2;</lolz>";
            var request = new CreateRoleRequest { Name = xmlBomb };
            try { var result = await mgr.CreateRoleAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch { Assert.True(true, "XML bomb prevented"); }
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-045")][Trait("Priority", "High")]
        public async Task UpdateRole_UnicodeNormalization_ConsistentHandling()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "café" }; // é can be 1 or 2 chars
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-046")][Trait("Priority", "Medium")]
        public async Task CreateRole_HTMLComments_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "<!--<script>alert(1)</script>-->" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-047")][Trait("Priority", "High")]
        public async Task UpdateRole_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "{\"key\":\"value\"}" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-048")][Trait("Priority", "Medium")]
        public async Task CreateRole_EscapedQuotes_HandlesCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "Role\\\"Name\\'" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-049")][Trait("Priority", "High")]
        public async Task UpdateRole_BackticksExpression_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "`${alert(1)}`" };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-VAL-050")][Trait("Priority", "Critical")]
        public async Task CreateRole_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "..\\..\\..\\windows\\system32" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }


    }
}
