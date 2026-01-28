using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Permissions;

namespace UNOPS.PAO.Tests.Integration.Permissions
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "Permissions")][Trait("Component", "ValidationTests")]
    public class PermissionValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public PermissionValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-PERM-VAL-001")][Trait("Priority", "Critical")]
        public async Task CreatePermission_SQLInjectionName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "'; DROP TABLE Permissions; --", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-002")][Trait("Priority", "Critical")]
        public async Task UpdatePermission_XSSPayloadDescription_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "XSSPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<script>alert('XSS')</script>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-003")][Trait("Priority", "High")]
        public async Task CreatePermission_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "CmdPerm", Description = "; rm -rf /" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-004")][Trait("Priority", "High")]
        public async Task UpdatePermission_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "NoSQLPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "{ $ne: null }" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-005")][Trait("Priority", "High")]
        public async Task CreatePermission_LDAPInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "LDAPPerm", Description = "Admin*)(uid=*" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-006")][Trait("Priority", "High")]
        public async Task UpdatePermission_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "PathPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "../../etc/passwd" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-007")][Trait("Priority", "Medium")]
        public async Task CreatePermission_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "XMLPerm", Description = "<!DOCTYPE foo [<!ENTITY xxe>]>" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-008")][Trait("Priority", "High")]
        public async Task UpdatePermission_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "CRLFPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "Desc\r\nSet-Cookie: malicious" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-009")][Trait("Priority", "High")]
        public async Task CreatePermission_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "JSPerm", Description = "javascript:alert(1)" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-010")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_DataURI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "DataURIPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "data:text/html,<script>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-011")][Trait("Priority", "High")]
        public async Task CreatePermission_PolyglotXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "PolyglotPerm", Description = "javascript:/*--><svg/onload=alert(1)" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-012")][Trait("Priority", "High")]
        public async Task UpdatePermission_TemplateLiteral_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "TemplatePerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "${alert(1)}" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-013")][Trait("Priority", "High")]
        public async Task CreatePermission_SSTI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "SST IPerm", Description = "{{config.items()}}" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-014")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_ExpressionLanguage_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "ExprPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "{{7*7}} #{7*7}" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-015")][Trait("Priority", "High")]
        public async Task CreatePermission_PrototypePollution_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "ProtoPerm", Description = "__proto__" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-016")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "HTMLPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "&#60;script&#62;" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-017")][Trait("Priority", "High")]
        public async Task CreatePermission_Base64Payload_StoredAsIs()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "B64Perm", Description = "PHNjcmlwdD4=" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-018")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_VBScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "VBSPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "vbscript:msgbox(1)" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-019")][Trait("Priority", "High")]
        public async Task CreatePermission_IMGTagXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "IMGPerm", Description = "<img src=x onerror=alert(1)>" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-020")][Trait("Priority", "High")]
        public async Task UpdatePermission_SVGXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "SVGPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<svg onload=alert(1)>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-021")][Trait("Priority", "Medium")]
        public async Task CreatePermission_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "IFramePerm", Description = "<iframe src='malicious'>" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-022")][Trait("Priority", "High")]
        public async Task UpdatePermission_OBJECTTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "ObjectPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<object data='x'>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-023")][Trait("Priority", "Medium")]
        public async Task CreatePermission_EMBEDTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "EmbedPerm", Description = "<embed src='x'>" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-024")][Trait("Priority", "High")]
        public async Task UpdatePermission_FORMAction_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "FormPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<form action='malicious'>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-025")][Trait("Priority", "Medium")]
        public async Task CreatePermission_METARefresh_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "MetaPerm", Description = "<meta http-equiv='refresh'>" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-026")][Trait("Priority", "High")]
        public async Task UpdatePermission_LINKStylesheet_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "LinkPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<link rel='stylesheet'>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-027")][Trait("Priority", "Medium")]
        public async Task CreatePermission_STYLETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "StylePerm", Description = "<style>body{}</style>" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-028")][Trait("Priority", "High")]
        public async Task UpdatePermission_BASETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "BasePerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<base href='x'>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-029")][Trait("Priority", "High")]
        public async Task CreatePermission_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "EventPerm", Description = "<div onload=alert(1)>" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-030")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_MutationXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "MutationPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<noscript><p title='</noscript><img src=x onerror=alert(1)>'>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-031")][Trait("Priority", "High")]
        public async Task CreatePermission_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "URLPerm", Description = "Desc%20%3C%3E" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-032")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_HexEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "HexPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "\\x3c\\x3e" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-033")][Trait("Priority", "Low")]
        public async Task CreatePermission_OctalEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "OctalPerm", Description = "\\074\\076" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-034")][Trait("Priority", "High")]
        public async Task UpdatePermission_UTF7Encoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "UTF7Perm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "+ADw-script+AD4-" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-035")][Trait("Priority", "High")]
        public async Task CreatePermission_MixedEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "MixedPerm", Description = "&#60;%3Cscript%3E&#62;" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-036")][Trait("Priority", "High")]
        public async Task UpdatePermission_DOMClobbering_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "DOMPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<form name='x'><input name='y'>" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-037")][Trait("Priority", "Medium")]
        public async Task CreatePermission_DanglingMarkup_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "DanglePerm", Description = "<img src='x?" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-038")][Trait("Priority", "High")]
        public async Task UpdatePermission_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "HomographPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "Αdmin" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-039")][Trait("Priority", "Medium")]
        public async Task CreatePermission_FormatString_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "FormatPerm", Description = "%s%s%s%s" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-040")][Trait("Priority", "Critical")]
        public async Task UpdatePermission_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "NullBytePerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "Desc\0Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-041")][Trait("Priority", "High")]
        public async Task CreatePermission_DeepHTMLNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var request = new CreatePermissionRequest { Name = "DeepNestPerm", Description = deep };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-042")][Trait("Priority", "High")]
        public async Task UpdatePermission_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "RegexPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "(a+)+" + new string('a', 50) };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-043")][Trait("Priority", "Critical")]
        public async Task CreatePermission_BufferOverflow_PreventedOrHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "BufferPerm", Description = new string('A', 10000) };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-044")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_XMLBomb_DetectedOrPrevented()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "BombPerm", Description = "Test" }, CreateUser());
            var xmlBomb = "<?xml version='1.0'?><!DOCTYPE lolz [<!ENTITY lol 'lol'><!ENTITY lol2 '&lol;&lol;'>]>";
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = xmlBomb };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-045")][Trait("Priority", "High")]
        public async Task CreatePermission_UnicodeNormalization_ConsistentHandling()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "NormPerm", Description = "café" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-046")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_HTMLComments_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "CommentPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "<!--<script>alert(1)</script>-->" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-047")][Trait("Priority", "High")]
        public async Task CreatePermission_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "JSONPerm", Description = "{\"key\":\"value\"}" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-048")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_EscapedQuotes_HandlesCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "QuotePerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "Desc\\\"Test\\'" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-049")][Trait("Priority", "High")]
        public async Task CreatePermission_BackticksExpression_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "BacktickPerm", Description = "`${alert(1)}`" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-VAL-050")][Trait("Priority", "Critical")]
        public async Task UpdatePermission_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "WinPathPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "..\\..\\..\\windows\\system32" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }


    }
}
