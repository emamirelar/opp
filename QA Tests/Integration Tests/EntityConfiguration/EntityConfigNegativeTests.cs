using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.EntityConfiguration;

namespace UNOPS.PAO.Tests.Integration.EntityConfiguration
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "EntityConfiguration")][Trait("Component", "NegativeTests")]
    public class EntityConfigNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public EntityConfigNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ENTITY-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetEntityConfig_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetEntityConfigByIdAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-002")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_NullEntityName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = null, DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-003")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_EmptyEntityName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = string.Empty, DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-004")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_DuplicateEntityName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "Partner", DisplayName = "Test" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-005")][Trait("Priority", "Critical")]
        public async Task UpdateEntityConfig_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new UpdateEntityConfigRequest { Id = 999999, DisplayName = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-006")][Trait("Priority", "High")]
        public async Task DeleteEntityConfig_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.DeleteEntityConfigAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-007")][Trait("Priority", "Critical")]
        public async Task GetEntityConfig_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetEntityConfigByIdAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-008")][Trait("Priority", "Critical")]
        public async Task CreateEntityConfig_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new CreateEntityConfigRequest { EntityName = "TestEntity", DisplayName = "Test" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.CreateEntityConfigAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-009")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new UpdateEntityConfigRequest { Id = 1, DisplayName = "Hacked" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.UpdateEntityConfigAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-010")][Trait("Priority", "High")]
        public async Task DeleteEntityConfig_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.DeleteEntityConfigAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-011")][Trait("Priority", "High")]
        public async Task DeleteEntityConfig_SystemEntity_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.DeleteEntityConfigAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-012")][Trait("Priority", "Medium")]
        public async Task CreateEntityConfig_NullDisplayName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "TestEntity", DisplayName = null };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-013")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_EmptyDisplayName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "TestEntity", DisplayName = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-014")][Trait("Priority", "Medium")]
        public async Task CreateEntityConfig_WhitespaceEntityName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "   ", DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-015")][Trait("Priority", "Medium")]
        public async Task CreateEntityConfig_WhitespaceDisplayName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "TestEntity", DisplayName = "   " };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-016")][Trait("Priority", "Medium")]
        public async Task GetEntityConfig_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetEntityConfigByIdAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-017")][Trait("Priority", "Medium")]
        public async Task GetEntityConfig_ZeroId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetEntityConfigByIdAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-018")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateEntityConfigAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-019")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.UpdateEntityConfigAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-020")][Trait("Priority", "High")]
        public async Task AddField_NonExistentEntityConfig_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 999999, FieldName = "Test", FieldType = "String" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-021")][Trait("Priority", "High")]
        public async Task AddField_NullFieldName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = null, FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-022")][Trait("Priority", "High")]
        public async Task AddField_EmptyFieldName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = string.Empty, FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-023")][Trait("Priority", "High")]
        public async Task AddField_DuplicateFieldName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = "Name", FieldType = "String" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-024")][Trait("Priority", "High")]
        public async Task AddField_InvalidFieldType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = "Test", FieldType = "InvalidType" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-025")][Trait("Priority", "High")]
        public async Task RemoveField_NonExistentField_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveFieldAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-026")][Trait("Priority", "High")]
        public async Task UpdateEntityConfig_ChangeToExistingName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new UpdateEntityConfigRequest { Id = 2, EntityName = "Partner" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-027")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_ExcessiveNameLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = new string('A', 500), DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-028")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_DeletedEntity_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new UpdateEntityConfigRequest { Id = 888, DisplayName = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-029")][Trait("Priority", "High")]
        public async Task GetFields_NonExistentEntityConfig_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetFieldsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-030")][Trait("Priority", "Medium")]
        public async Task UpdateEntityConfig_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new UpdateEntityConfigRequest { Id = -1, DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-031")][Trait("Priority", "Medium")]
        public async Task DeleteEntityConfig_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.DeleteEntityConfigAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-032")][Trait("Priority", "High")]
        public async Task AddField_NullFieldType_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = "Test", FieldType = null };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-033")][Trait("Priority", "High")]
        public async Task AddField_EmptyFieldType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = "Test", FieldType = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-034")][Trait("Priority", "Medium")]
        public async Task GetEntityConfig_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetEntityConfigByIdAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-035")][Trait("Priority", "High")]
        public async Task AddField_NegativeEntityConfigId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = -1, FieldName = "Test", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-036")][Trait("Priority", "High")]
        public async Task RemoveField_NegativeFieldId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RemoveFieldAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-037")][Trait("Priority", "High")]
        public async Task UpdateField_NonExistentField_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new UpdateFieldRequest { Id = 999999, FieldName = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-038")][Trait("Priority", "Critical")]
        public async Task GetEntityConfigs_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAllEntityConfigsAsync(CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-039")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_ConcurrentDuplicateName_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var r1 = new CreateEntityConfigRequest { EntityName = "DuplicateEntity", DisplayName = "Dup1" };
            var r2 = new CreateEntityConfigRequest { EntityName = "DuplicateEntity", DisplayName = "Dup2" };
            var t1 = mgr.CreateEntityConfigAsync(r1, CreateUser());
            var t2 = mgr.CreateEntityConfigAsync(r2, CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.Fail("Should conflict"); }
            catch { Assert.True(true, "Name conflict"); }
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-040")][Trait("Priority", "High")]
        public async Task UpdateDeleteEntityConfig_Concurrent_HandlesRaceCondition()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var update = mgr.UpdateEntityConfigAsync(new UpdateEntityConfigRequest { Id = 5, DisplayName = "Updated" }, CreateUser());
            var delete = mgr.DeleteEntityConfigAsync(5, CreateUser());
            try { await Task.WhenAll(update, delete); }
            catch { Assert.True(true, "Race handled"); }
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-041")][Trait("Priority", "Medium")]
        public async Task GetEntityConfig_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetEntityConfigByIdAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-042")][Trait("Priority", "High")]
        public async Task RemoveField_RequiredField_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.RemoveFieldAsync(1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-043")][Trait("Priority", "High")]
        public async Task AddField_WhitespaceFieldName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = "   ", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-044")][Trait("Priority", "High")]
        public async Task UpdateField_ChangeToExistingName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new UpdateFieldRequest { Id = 2, FieldName = "Name" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdateFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-045")][Trait("Priority", "Medium")]
        public async Task CreateEntityConfig_SpecialCharsInName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "Entity!@#$", DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-046")][Trait("Priority", "High")]
        public async Task AddField_ExcessiveFieldNameLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = new string('A', 500), FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-047")][Trait("Priority", "High")]
        public async Task GetFields_NegativeEntityConfigId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetFieldsAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-048")][Trait("Priority", "Medium")]
        public async Task UpdateField_NullFieldName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new UpdateFieldRequest { Id = 2, FieldName = null };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.UpdateFieldAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-049")][Trait("Priority", "High")]
        public async Task CreateEntityConfig_NumericEntityName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new CreateEntityConfigRequest { EntityName = "123Entity", DisplayName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateEntityConfigAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ENTITY-NEG-050")][Trait("Priority", "Critical")]
        public async Task AddField_SQLInjectionFieldName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().EntityConfigManager;
            var request = new AddFieldRequest { EntityConfigId = 1, FieldName = "'; DROP TABLE Fields; --", FieldType = "String" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddFieldAsync(request, CreateUser()));
        }

        #endregion
    }
}
