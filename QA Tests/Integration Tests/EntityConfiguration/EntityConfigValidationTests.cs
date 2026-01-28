using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.EntityConfiguration;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.EntityConfiguration
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "EntityConfiguration")][Trait("Component", "ValidationTests")]
    public class EntityConfigValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public EntityConfigValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ENTITY-VAL-001")][Trait("Priority", "Critical")]
        public async Task CreateEntityConfig_SQLInjectionName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "'; DROP TABLE Entities; --", DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-002")][Trait("Priority", "Critical")]
        public async Task UpdateEntityConfig_XSSPayloadDisplayName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "XSSEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<script>alert('XSS')</script>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-003")][Trait("Priority", "High")]
        public async Task AddField_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "CmdEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "; rm -rf /", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-004")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "NoSQLEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "{ $ne: null }" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-005")][Trait("Priority", "High")]
        public async Task AddField_LDAPInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "LDAPEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "Admin*)(uid=*", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-006")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "PathEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "../../etc/passwd" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-007")][Trait("Priority", "Medium")]
        public async Task AddField_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "XMLEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<!DOCTYPE foo [<!ENTITY xxe>]>", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-008")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "CRLFEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "Name\r\nSet-Cookie: malicious" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-009")][Trait("Priority", "High")]
        public async Task AddField_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "JSEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "javascript:alert(1)", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-010")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_DataURI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "DataURIEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "data:text/html,<script>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-011")][Trait("Priority", "High")]
        public async Task AddField_PolyglotXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "PolyglotEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "javascript:/*--><svg/onload=alert(1)", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-012")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_TemplateLiteral_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "TemplateEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "${alert(1)}" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-013")][Trait("Priority", "High")]
        public async Task AddField_SSTI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "SSTIEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "{{config.items()}}", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-014")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_ExpressionLanguage_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "ExprEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "{{7*7}} #{7*7}" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-015")][Trait("Priority", "High")]
        public async Task AddField_PrototypePollution_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "ProtoEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "__proto__", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-016")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "HTMLEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "&#60;script&#62;" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-017")][Trait("Priority", "High")]
        public async Task AddField_Base64Payload_StoredAsIs()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "B64Entity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "PHNjcmlwdD4", FieldType = "String" };
            var result = await mgr.AddFieldAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-018")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_VBScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "VBSEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "vbscript:msgbox(1)" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-019")][Trait("Priority", "High")]
        public async Task AddField_IMGTagXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "IMGEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<img src=x onerror=alert(1)>", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-020")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_SVGXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "SVGEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<svg onload=alert(1)>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-021")][Trait("Priority", "Medium")]
        public async Task AddField_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "IFrameEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<iframe src='malicious'>", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-022")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_OBJECTTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "ObjectEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<object data='x'>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-023")][Trait("Priority", "Medium")]
        public async Task AddField_EMBEDTag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "EmbedEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<embed src='x'>", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-024")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_FORMAction_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "FormEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<form action='malicious'>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-025")][Trait("Priority", "Medium")]
        public async Task AddField_METARefresh_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "MetaEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<meta http-equiv='refresh'>", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-026")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_LINKStylesheet_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "LinkEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<link rel='stylesheet'>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-027")][Trait("Priority", "Medium")]
        public async Task AddField_STYLETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "StyleEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<style>body{}</style>", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-028")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_BASETag_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "BaseEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<base href='x'>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-029")][Trait("Priority", "High")]
        public async Task AddField_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "EventEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<div onload=alert(1)>", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-030")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_MutationXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "MutationEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<noscript><p title='</noscript><img src=x onerror=alert(1)>'>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-031")][Trait("Priority", "High")]
        public async Task AddField_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "URLEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "Field%20%3C%3E", FieldType = "String" };
            var result = await mgr.AddFieldAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-032")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_HexEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "HexEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "\\x3c\\x3e" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-033")][Trait("Priority", "Low")]
        public async Task AddField_OctalEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "OctalEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "\\074\\076", FieldType = "String" };
            var result = await mgr.AddFieldAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-034")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_UTF7Encoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "UTF7Entity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "+ADw-script+AD4-" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-035")][Trait("Priority", "High")]
        public async Task AddField_MixedEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "MixedEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "&#60;%3Cscript%3E&#62;", FieldType = "String" };
            var result = await mgr.AddFieldAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-036")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_DOMClobbering_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "DOMEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<form name='x'><input name='y'>" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-037")][Trait("Priority", "Medium")]
        public async Task AddField_DanglingMarkup_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "DangleEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "<img src='x?", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-038")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "HomographEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "Αdmin" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-039")][Trait("Priority", "Medium")]
        public async Task AddField_FormatString_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "FormatEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "%s%s%s%s", FieldType = "String" };
            var result = await mgr.AddFieldAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-040")][Trait("Priority", "Critical")]
        public async Task UpdateEntityConfig_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "NullByteEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "Name\0Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-041")][Trait("Priority", "High")]
        public async Task AddField_DeepHTMLNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "DeepNestEntity", DisplayName = "Test" }, CreateUser());
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = deep, FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-042")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "RegexEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "(a+)+" + new string('a', 50) };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-043")][Trait("Priority", "Critical")]
        public async Task AddField_BufferOverflow_PreventedOrHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "BufferEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = new string('A', 10000), FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-044")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_XMLBomb_DetectedOrPrevented()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "BombEntity", DisplayName = "Test" }, CreateUser());
            var xmlBomb = "<?xml version='1.0'?><!DOCTYPE lolz [<!ENTITY lol 'lol'><!ENTITY lol2 '&lol;&lol;'>]>";
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = xmlBomb };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-045")][Trait("Priority", "High")]
        public async Task AddField_UnicodeNormalization_ConsistentHandling()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "NormEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "café", FieldType = "String" };
            var result = await mgr.AddFieldAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-046")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_HTMLComments_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "CommentEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "<!--<script>alert(1)</script>-->" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-047")][Trait("Priority", "High")]
        public async Task AddField_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "JSONEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "{\"key\":\"value\"}", FieldType = "String" };
            var result = await mgr.AddFieldAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-048")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_EscapedQuotes_HandlesCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "QuoteEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "Name\\\"Test\\'" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-049")][Trait("Priority", "High")]
        public async Task AddField_BackticksExpression_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "BacktickEntity", DisplayName = "Test" }, CreateUser());
            var request = new AddFieldRequest { EntityConfigId = entity.Id, FieldName = "`${alert(1)}`", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-VAL-050")][Trait("Priority", "Critical")]
        public async Task UpdateEntityConfig_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var entity = await mgr.CreateEntityConfigAsync(new CreateEntityConfigRequest { EntityName = "WinPathEntity", DisplayName = "Test" }, CreateUser());
            var request = new UpdateEntityConfigRequest { Id = entity.Id, DisplayName = "..\\..\\..\\windows\\system32" };
            var result = await mgr.UpdateEntityConfigAsync(request, CreateUser());
            result.Should().NotBeNull();
        }


    }
}
